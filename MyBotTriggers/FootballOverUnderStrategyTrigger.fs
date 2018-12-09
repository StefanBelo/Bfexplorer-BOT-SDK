// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module FootballOverUnderStrategyTrigger

open System

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// getMarketSettledProfit
/// </summary>
/// <param name="market"></param>
let getMarketSettledProfit(market : Market) =
    if market.HaveMatchedBets
    then
        let winners = market.Selections |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Winner && selection.Profit.HasValue)

        if not(Seq.isEmpty winners)
        then
            let profit = winners |> Seq.sumBy (fun selection -> selection.Profit.Value)
            market.SettledProfit <- Nullable profit

            Some profit
        else
            None
    else
        None

/// <summary>
/// getIsActionBotRunning
/// </summary>
/// <param name="market"></param>
let getIsActionBotRunning(market : Market) =
    market.RunningBots.Count > 0

let [<Literal>] ExecuteOnSelection = 1
let [<Literal>] RequiredProfit = 5.0

/// <summary>
/// getMarketProfitForExecutedSelection
/// </summary>
/// <param name="market"></param>
let getMarketProfitForExecutedSelection(market : Market) =
    let betPosition = market.Selections.[ExecuteOnSelection].BetPosition
    betPosition.ProfitIfWin

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | CheckCurrentScore
    | OpenOverUnderMarket
    | StartMyStrategy
    | CheckMyStrategyResult
    | ReportError of string

/// <summary>
/// FootballOverUnderStrategyTrigger
/// </summary>
type FootballOverUnderStrategyTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.Initialize
    let mutable footballMatch = nil<FootballMatch>
    let mutable executingOnMarket = nil<Market>
    let mutable timeToExecute = DateTime.MaxValue
    let mutable liability = 0.0

    let setTimeToExecute() =
        timeToExecute <- DateTime.Now.AddSeconds(30.0)

    let isInitialBet() =
        liability = 0.0

    let totalScoredGoals() =
        int (footballMatch.HomeScore + footballMatch.AwayScore)

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            let result =
                match status with
                | TriggerStatus.Initialize ->

                    if market.MarketInfo.BetEventType.Id = 1 && market.MarketDescription.MarketType = "MATCH_ODDS"
                    then
                        footballMatch <- GetFootballMatch(market)
                        UpdateFootballMatchScore (footballMatch, fun result -> 
                            status <- 
                                if result 
                                then 
                                    TriggerStatus.OpenOverUnderMarket
                                else 
                                    setTimeToExecute()
                                    TriggerStatus.CheckCurrentScore
                        )
                    else
                        EndExecutionWithMessage "You can execute this bot only on a football market."

                | TriggerStatus.CheckCurrentScore -> 
                    
                    if DateTime.Now >= timeToExecute
                    then
                        UpdateFootballMatchScore (footballMatch, fun result -> 
                            if result 
                            then 
                                status <- TriggerStatus.OpenOverUnderMarket

                            setTimeToExecute()
                        )
                    else
                        WaitingForOperation

                | TriggerStatus.OpenOverUnderMarket -> 

                    let goals =
                        if isInitialBet()
                        then
                            totalScoredGoals()
                        else
                            (defaultArg (botTriggerParameters.GetParameter<int>("AddGoals")) 2) + totalScoredGoals()

                    let marketName = sprintf "OVER_UNDER_%d5" goals

                    OpenAssociatedMarkets ([| marketName |], fun result ->
                        status <-
                            if result.IsSuccessResult
                            then
                                executingOnMarket <- result.SuccessResult.[0]
                                TriggerStatus.StartMyStrategy
                            else
                                // TriggerStatus.ReportError (sprintf "Failed to open the market: %s" marketName)
                                TriggerStatus.CheckCurrentScore
                    )

                | TriggerStatus.StartMyStrategy ->

                    status <- TriggerStatus.CheckMyStrategyResult
                    setTimeToExecute()

                    let isInitialBet = isInitialBet()

                    let botName, myBotParameters = 
                        if isInitialBet
                        then 
                            "Lay trade over", []
                        else 
                            "Place lay on over", [ { MyBotParameter.Name = "Stake"; MyBotParameter.Value = liability + RequiredProfit } ]

                    ExecuteMyActionBotOnMarketSelectionAndContinueToExecute (botName, executingOnMarket, executingOnMarket.Selections.[ExecuteOnSelection], myBotParameters, isInitialBet)

                | TriggerStatus.CheckMyStrategyResult ->

                    if DateTime.Now >= timeToExecute
                    then
                        UpdateFootballMatchScore (footballMatch, fun _ -> setTimeToExecute())
                    else
                        if executingOnMarket.MarketStatus = MarketStatus.Closed
                        then
                            status <- TriggerStatus.CheckCurrentScore

                            match getMarketSettledProfit(executingOnMarket) with
                            | Some profit ->

                                if profit > 0.0
                                then
                                    EndExecutionWithMessage (sprintf "End of my strategy, profit: %.2f" profit)
                                else
                                    liability <- liability + -profit
                                    WaitingForOperation

                            | None -> WaitingForOperation

                        elif isInitialBet()
                        then
                            if getIsActionBotRunning(executingOnMarket)
                            then
                                WaitingForOperation
                            else
                                let profit = getMarketProfitForExecutedSelection(executingOnMarket)
                                EndExecutionWithMessage (sprintf "End of my strategy, profit: %.2f" profit)
                        else
                            WaitingForOperation

                | TriggerStatus.ReportError message -> EndExecutionWithMessage message
        
            result

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()