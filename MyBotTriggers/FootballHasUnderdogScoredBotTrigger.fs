// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module FootballHasUnderdogScoredBotTrigger

open System
open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
    
/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | OpenMatchOddsMarket
    | FailedToOpenMatchOddsMarket
    | UpdateMatchScore
    | GoalScored

/// <summary>
/// FootballHasUnderdogScoredBotTrigger
/// </summary>
type FootballHasUnderdogScoredBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.OpenMatchOddsMarket
    let mutable timeDelayOperation = DateTime.MinValue

    let mutable footballMatch = nil<FootballMatch>
    let mutable favourite = nil<Selection>
    let mutable underdog = nil<Selection>

    let mutable homeScore = 0uy
    let mutable wayScore = 0uy
    let mutable goalScoredByTeam = nil<Selection>

    let setFootballMatchData(result : DataResult<Market[]>) =
        status <- 
            if result.IsSuccessResult
            then
                let matchOddsMarket = result.SuccessResult.[0]
                let selections = matchOddsMarket.Selections

                footballMatch <- GetFootballMatch(matchOddsMarket)
                
                let favouriteIndex, underdogIndex =
                    if selections.[0].LastPriceTraded < selections.[1].LastPriceTraded then 0, 1 else 1, 0

                favourite <- selections.[favouriteIndex]
                underdog <- selections.[underdogIndex]

                myBfexplorer.BfexplorerService.OutputMessage(sprintf "Underdog: %s" underdog.Name)
                    
                UpdateMatchScore
            else
                FailedToOpenMatchOddsMarket

    let openMatchOddsMarket() =
        if market.MarketInfo.BetEventType.Id = 1
        then
            if myBfexplorer.OpenBetEvent.IsNone
            then
                EndExecutionWithMessage "You cannot execute this bot in Bot executor."
            else
                OpenAssociatedMarkets ([| "MATCH_ODDS" |], setFootballMatchData)
        else
            EndExecutionWithMessage "You can execute this bot only on a football market."

    let setTimeDelayOperation() =
        timeDelayOperation <- DateTime.Now.AddSeconds(10.0)

    let isGoalScored(result : bool)  =
        if result && footballMatch.IsUpdated
        then
            if (homeScore + wayScore) <> (footballMatch.HomeScore + footballMatch.AwayScore)
            then                
                goalScoredByTeam <- footballMatch.Market.Selections.[if homeScore <> footballMatch.HomeScore then 0 else 1]
                
                homeScore <- footballMatch.HomeScore
                wayScore <- footballMatch.AwayScore

                status <- GoalScored

        setTimeDelayOperation()

    let goalScoredExecuteAction() =
        status <- UpdateMatchScore

        let message = sprintf "Scored by %s (%s), %s" goalScoredByTeam.Name 
                        (if favourite.Id = goalScoredByTeam.Id then "favourite" else "underdog")
                        (footballMatch.ToString())

        myBfexplorer.BfexplorerService.OutputMessage(message)

        WaitingForOperation
            
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            let result =
                match status with
                | OpenMatchOddsMarket ->

                    openMatchOddsMarket()

                | FailedToOpenMatchOddsMarket ->

                    EndExecutionWithMessage "Failed to open the match odds market."

                | UpdateMatchScore ->

                    if DateTime.Now >= timeDelayOperation
                    then
                        UpdateFootballMatchScore (footballMatch, isGoalScored)
                    else
                        WaitingForOperation

                | GoalScored ->
                    
                    goalScoredExecuteAction()

            result

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()