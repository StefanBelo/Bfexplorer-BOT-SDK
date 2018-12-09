// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module FootballGoalScoredStrategyBotTrigger

open System
open System.Collections.Generic

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

let inline GetNumberOfGoals(footballMatch : FootballMatch) =
    footballMatch.HomeScore + footballMatch.AwayScore

let inline GetOverUnderMarket(numberOfGoals : byte) =
    sprintf "OVER_UNDER_%d5" numberOfGoals
 
let GetMarketTypesToOpen(numberOfGoals : byte) =
    [| 
        GetOverUnderMarket(numberOfGoals)
        GetOverUnderMarket(numberOfGoals + 1uy)
    |]
    
/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | UpdateMatchScore
    | OpenMarkets
    | ExecuteMyStrategy
    | WaitForUpdateOperation
    | EndMyStrategy

/// <summary>
/// FootballGoalScoredStrategyBotTrigger
/// </summary>
type FootballGoalScoredStrategyBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.Initialize
    let mutable footballMatch = nil<FootballMatch>
    let mutable timeToUpdate = DateTime.MinValue
    let mutable goals = 0uy
    let mutable executeOnMarkets = nil<Queue<Market>>

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | Initialize ->

                if market.MarketInfo.BetEventType.Id = 1 && market.MarketDescription.MarketType = "MATCH_ODDS"
                then
                    footballMatch <- GetFootballMatch(market)
                    status <- WaitForUpdateOperation
                    UpdateFootballMatchScore (footballMatch, this.MatchScoreInitialized)
                else
                    EndExecutionWithMessage "You can execute this bot only on a football match odds market."

            | UpdateMatchScore ->

                if DateTime.Now >= timeToUpdate
                then
                    status <- WaitForUpdateOperation
                    UpdateFootballMatchScore (footballMatch, this.MatchScoreUpdated)
                else
                    WaitingForOperation

            | OpenMarkets ->

                status <- WaitForUpdateOperation
                OpenAssociatedMarkets (GetMarketTypesToOpen(goals), this.AssociatedMarketsOpened)

            | ExecuteMyStrategy ->

                this.ExecuteMyStrategy()

            | WaitForUpdateOperation -> WaitingForOperation

            | EndMyStrategy -> EndExecution

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// SetNextUpdateMatchScore
    /// </summary>
    member private this.SetNextUpdateMatchScore() =
        status <- UpdateMatchScore
        timeToUpdate <- DateTime.Now.AddSeconds(15.0)

    /// <summary>
    /// MatchScoreUpdated
    /// </summary>
    /// <param name="result"></param>
    member private this.MatchScoreInitialized(result : bool) =
        this.SetNextUpdateMatchScore()
        goals <- GetNumberOfGoals(footballMatch)

        myBfexplorer.BfexplorerService.OutputMessage(footballMatch.ToString())

    /// <summary>
    /// MatchScoreUpdated
    /// </summary>
    /// <param name="result"></param>
    member private this.MatchScoreUpdated(result : bool) =
        this.SetNextUpdateMatchScore()

        if result && footballMatch.IsUpdated
        then
            let numberOfGoals = GetNumberOfGoals(footballMatch)

            if numberOfGoals <> goals
            then
                goals <- numberOfGoals
                status <- OpenMarkets

            myBfexplorer.BfexplorerService.OutputMessage(footballMatch.ToString())

    /// <summary>
    /// AssociatedMarketsOpened
    /// </summary>
    /// <param name="result"></param>
    member private this.AssociatedMarketsOpened(result : DataResult<Market[]>) =
        status <-
            if result.IsSuccessResult
            then
                executeOnMarkets <- Queue result.SuccessResult
                ExecuteMyStrategy
            else
                EndMyStrategy

    /// <summary>
    /// ExecuteMyStrategy
    /// </summary>    
    member private this.ExecuteMyStrategy() =
        let myMarket = executeOnMarkets.Dequeue()

        if executeOnMarkets.Count = 0
        then
            executeOnMarkets <- nil
            status <- UpdateMatchScore

        if myMarket.MarketStatus = MarketStatus.Open
        then
            ExecuteActionBotOnMarketSelectionAndContinueToExecute (myMarket, myMarket.Selections.[0], true)
        else
            WaitingForOperation