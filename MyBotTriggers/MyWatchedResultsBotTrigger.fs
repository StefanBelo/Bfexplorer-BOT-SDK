// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MyWatchedResultsBotTrigger

open System

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetFavourite
/// </summary>
/// <param name="market"></param>
let GetFavourite(market : Market) =
    market.Selections
    |> Seq.filter (fun selection -> selection.Status <> SelectionStatus.Removed)
    |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
    |> Seq.head

/// <summary>
/// MySelectionStrategy
/// </summary>
/// <param name="market"></param>
let MySelectionStrategy(market : Market) =
    [| { MySelectionBet.Selection = GetFavourite(market); MySelectionBet.BetType = BetType.Back } |]

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | WatchResultsToStartMyStrategy
    | ExecuteMyStrategy
    | CheckingMyWatchResults
    | FailedToCheckMyWatchResults
    | WaitToEndMonitoring

/// <summary>
/// MyWatchedResultsBotTrigger
/// </summary>
type MyWatchedResultsBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let myWatchedResults = MyWatchedResultsOperations.RegisterMyStrategy(botName, MySelectionStrategy)
    let myStrategyResults = defaultArg (MyStrategyOperations.GetMyStrategyResults(botName)) nil

    let winningStreak = defaultArg (botTriggerParameters.GetParameter<int>("WinningStreak")) 1
    let timeToExecuteMyStrategy = market.MarketInfo.StartTime.AddSeconds(-(defaultArg (botTriggerParameters.GetParameter<float>("Time")) 15.0))

    let mutable status = TriggerStatus.Initialize
    let mutable timeToCheckWatchedResults = DateTime.MinValue

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | Initialize ->
                
                if isNull myStrategyResults
                then
                    EndExecutionWithMessage "Please use the parameter: UseMyStrategyResults."
                else
                    status <-
                        match myStrategyResults.BettingStreak with
                        | LosingStreak _ -> TriggerStatus.ExecuteMyStrategy
                        | _ -> TriggerStatus.WatchResultsToStartMyStrategy
                        
                    WaitingForOperation

            | WatchResultsToStartMyStrategy ->
                
                this.WatchResultsToStartMyStrategy()    

            | ExecuteMyStrategy ->

                this.ExecuteMyStrategy()

            | CheckingMyWatchResults ->
                
                WaitingForOperation

            | FailedToCheckMyWatchResults ->

                EndExecutionWithMessage "Failed to check my watch results!"

            | WaitToEndMonitoring ->

                let time = DateTime.Now

                if time >= timeToExecuteMyStrategy
                then
                    myWatchedResults.AddMarket(market)
                    EndExecution
                else
                    WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// WatchResultsToStartMyStrategy
    /// </summary>
    member private this.WatchResultsToStartMyStrategy() =
        if myWatchedResults.CanExecuteMyStrategy
        then
            let canExecuteMyStrategy =
                match myWatchedResults.BettingStreak with
                | WinningStreak numberOfBets ->

                    if numberOfBets >= winningStreak
                    then
                        myWatchedResults.ResetResults()
                        true
                    else
                        false

                | _ -> false

            status <- 
                if canExecuteMyStrategy
                then
                    TriggerStatus.ExecuteMyStrategy
                else
                    TriggerStatus.WaitToEndMonitoring

            WaitingForOperation
        else
            let time = DateTime.Now

            if time >= timeToExecuteMyStrategy
            then
                myWatchedResults.AddMarket(market)
                EndExecution
            else
                if time >= timeToCheckWatchedResults
                then
                    this.CheckMyWatchResults()                           
                    timeToCheckWatchedResults <- time.AddSeconds(15.0)

                WaitingForOperation

    /// <summary>
    /// CheckMyWatchResults
    /// </summary>
    member private this.CheckMyWatchResults() =
        status <- TriggerStatus.CheckingMyWatchResults

        Async.StartWithContinuations(
            computation = myWatchedResults.CheckMyWatchedResults(myBfexplorer.BfexplorerService),
            continuation = (fun _ -> status <- TriggerStatus.WatchResultsToStartMyStrategy),
            exceptionContinuation = (fun _ -> status <- TriggerStatus.FailedToCheckMyWatchResults),
            cancellationContinuation = (fun _ -> status <- TriggerStatus.FailedToCheckMyWatchResults)
        )

    /// <summary>
    /// ExecuteMyStrategy
    /// </summary>
    member private this.ExecuteMyStrategy() =
        let time = DateTime.Now

        if time >= timeToExecuteMyStrategy
        then
            let stake = defaultArg (botTriggerParameters.GetParameter<float>("Stake")) 5.0

            let myStake = 
                match myStrategyResults.BettingStreak with
                | LosingStreak numberOfBets ->

                    let numberOfRecoveryBets = defaultArg (botTriggerParameters.GetParameter<int>("NumberOfRecoveryBets")) 3

                    if numberOfBets <= numberOfRecoveryBets
                    then
                        let isReallyLoss = myStrategyResults.TotalProfit < 0.0

                        if isReallyLoss
                        then
                            (stake * 2.0) + abs(myStrategyResults.TotalProfit)
                        else
                            stake * 2.0
                    else
                        // Reset my strategy
                        myStrategyResults.ResetResults()
                        myWatchedResults.AddMarket(market)
                        0.0

                | _ -> stake

            if myStake = 0.0
            then
                EndExecution
            else
                let myBotParameters = [ { MyBotParameter.Name = "Stake"; MyBotParameter.Value = myStake } ]
                ExecuteActionBotWithParameters myBotParameters
        else
            WaitingForOperation
