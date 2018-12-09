// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module FootballMatchProfitBotTrigger

open System
open System.Text
open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetProfitForMatchOdds
/// </summary>
/// <param name="market"></param>
/// <param name="footballMatch"></param>
let GetProfitForMatchOdds(market : Market, footballMatch : FootballMatch) =
    let selectionIndex =
        match footballMatch.HomeScore - footballMatch.AwayScore with
        | goalDifference when goalDifference > 0uy -> 0
        | goalDifference when goalDifference < 0uy -> 1
        | _ -> 2

    let selection = market.Selections.[selectionIndex]

    selection.Profit

/// <summary>
/// GetProfitForCorrectScore
/// </summary>
/// <param name="market"></param>
/// <param name="footballMatch"></param>
let GetProfitForCorrectScore(market : Market, footballMatch : FootballMatch) =
    let selections = market.Selections

    match GetSelectionByName(footballMatch.Score, selections) with
    | Some selection -> selection.Profit
    | None ->
        let selectionName =
            match footballMatch.HomeScore - footballMatch.AwayScore with
            | goalDifference when goalDifference > 0uy -> "Any Other Home Win"
            | goalDifference when goalDifference < 0uy -> "Any Other Away Win"
            | _ -> "Any Other Draw"

        match GetSelectionByName(selectionName, selections) with
        | Some selection -> selection.Profit
        | None -> Nullable()

/// <summary>
/// GetProfitForOverUnder
/// </summary>
/// <param name="market"></param>
/// <param name="footballMatch"></param>
let GetProfitForOverUnder(market : Market, footballMatch : FootballMatch) =
    let selections = market.Selections

    let getUnderGoals(selection : Selection) =
        let data = selection.Name.Split(' ')
        float data.[1]
    
    let underGoals = getUnderGoals(selections.[0])
    let goalsScored = float (footballMatch.HomeScore + footballMatch.AwayScore)
    let selectionIndex = if goalsScored > underGoals then 1 else 0

    selections.[selectionIndex].Profit
    
/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | UpdateMatchScore
    | ReportProfit
    | ReportError of string

/// <summary>
/// FootballMatchProfitBotTrigger
/// </summary>
type FootballMatchProfitBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    let mutable status = TriggerStatus.Initialize
    let mutable footballMatch = nil<FootballMatch>

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            let result =
                match status with
                | TriggerStatus.Initialize ->

                    if market.MarketInfo.BetEventType.Id = 1
                    then
                        if myBfexplorer.OpenBetEvent.IsNone
                        then
                            EndExecutionWithMessage "You cannot execute this bot in Bot executor."
                        else
                            OpenAssociatedMarkets ([| "MATCH_ODDS" |], fun result ->
                                status <- 
                                    if result.IsSuccessResult
                                    then
                                        footballMatch <- GetFootballMatch(result.SuccessResult.[0])
                                        TriggerStatus.UpdateMatchScore
                                    else
                                        TriggerStatus.ReportError "Failed to open Match Odds market."
                            )
                    else
                        EndExecutionWithMessage "You can execute this bot only on a football market."

                | TriggerStatus.UpdateMatchScore ->

                    UpdateFootballMatchScore (footballMatch, fun result -> if result then status <- TriggerStatus.ReportProfit)

                | TriggerStatus.ReportProfit -> this.ReportProfit()

                | TriggerStatus.ReportError message -> EndExecutionWithMessage message

            result

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// ReportProfit
    /// </summary>
    member private this.ReportProfit() =
        let sb = StringBuilder()

        sb.AppendLine(sprintf "\n%s" (footballMatch.ToString())) |> ignore

        myBfexplorer.OpenBetEvent
        |> Option.iter (fun openBetEvent ->
                
                let mutable totalProfit = 0.0

                openBetEvent.OpenMarkets 
                |> Seq.sortBy (fun myMarket -> myMarket.MarketInfo.MarketName)
                |> Seq.iter (fun myMarket -> 
                        let marketName = myMarket.MarketInfo.MarketName

                        let profit =
                            match marketName with
                            | "Match Odds" ->
                                GetProfitForMatchOdds(myMarket, footballMatch)
                            | name when name.StartsWith "Correct Score" ->
                                GetProfitForCorrectScore(myMarket, footballMatch)
                            | name when name.StartsWith "Over/Under" ->
                                GetProfitForCorrectScore(myMarket, footballMatch)
                            | _ -> Nullable()

                        let marketStatus =
                            if profit.HasValue
                            then
                                totalProfit <- totalProfit + profit.Value
                                sprintf "%s: %s" marketName (profit.Value.ToString("C"))
                            else
                                marketName
                        
                        sb.AppendLine(marketStatus) |> ignore
                    )

                sb.AppendFormat("\nTotal profit: {0:C}", totalProfit) |> ignore
            )
        
        EndExecutionWithMessage (sb.ToString())