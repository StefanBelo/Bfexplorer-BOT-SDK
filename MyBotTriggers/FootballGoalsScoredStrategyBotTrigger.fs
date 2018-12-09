// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module FootballGoalsScoredStrategyBotTrigger

open System
open System.Text

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
open BeloSoft.Bfexplorer.FootballScoreProvider
open BeloSoft.Bfexplorer.FootballScoreProvider.API.Models
    
/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | UpdateMatchScore
    | FailedToUpdateMatchScore
    | WaitToGetMatchDetails
    | ReportGoalScored

/// <summary>
/// FootballGoalsScoredStrategyBotTrigger
/// </summary>
type FootballGoalsScoredStrategyBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.Initialize
    let mutable footballMatch = nil<FootballMatch>
    let mutable matchDetailData = nil<MatchDetailData>

    let footballMatchScoreUpdated(result : bool) =
        status <-
            if result
            then
                Async.StartWithContinuations(
                    computation = FootballScoreProvider.GetMatchDetails(footballMatch),
                    continuation = (fun result -> 
                            status <-
                                if result.IsSuccessResult
                                then
                                    matchDetailData <- result.SuccessResult
                                    TriggerStatus.ReportGoalScored
                                else
                                    TriggerStatus.FailedToUpdateMatchScore
                        ),
                    exceptionContinuation = (fun _ -> status <- TriggerStatus.FailedToUpdateMatchScore),
                    cancellationContinuation = (fun _ -> status <- TriggerStatus.FailedToUpdateMatchScore)
                )

                TriggerStatus.WaitToGetMatchDetails
            else
                TriggerStatus.FailedToUpdateMatchScore

    let initialize() =
        if market.MarketInfo.BetEventType.Id = 1 && market.MarketDescription.MarketType = "MATCH_ODDS"
        then
            footballMatch <- GetFootballMatch(market)

            TriggerResult.UpdateFootballMatchScore (footballMatch, footballMatchScoreUpdated)
        else
            TriggerResult.EndExecutionWithMessage "You can execute this bot only on a football market."

    let getGoals(updateDetails : UpdateDetailData[]) =
        if updateDetails.Length > 0
        then
            updateDetails |> Array.map (fun updateDetail -> sprintf "%d' %s" updateDetail.MatchTime updateDetail.Team) |> String.concat ", "
        else
            "no goals scored"

    let reportGoalScored() =
        let sb = StringBuilder()

        sb.AppendLine(footballMatch.ToString()) |> ignore

        let goals = matchDetailData.UpdateDetails |> Array.filter (fun updateDetail -> updateDetail.UpdateType = "Goal")

        if goals.Length > 0
        then
            let firstHalfGoals, secondHalfGoals = goals |> Array.partition (fun updateDetail -> updateDetail.MatchTime <= 45)

            sb
                .AppendLine(sprintf "The first half goals: %s" (getGoals(firstHalfGoals)))
                .AppendLine(sprintf "The second half goals: %s" (getGoals(secondHalfGoals))) |> ignore
        else
            sb.AppendLine("No goals scored.") |> ignore

        myBfexplorer.BfexplorerService.OutputMessage(sb.ToString())
        TriggerResult.EndExecution
                    
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | TriggerStatus.Initialize -> initialize()
            | TriggerStatus.UpdateMatchScore -> UpdateFootballMatchScore (footballMatch, footballMatchScoreUpdated)
            | TriggerStatus.FailedToUpdateMatchScore -> EndExecutionWithMessage "Failed to update the match score."
            | TriggerStatus.WaitToGetMatchDetails -> WaitingForOperation
            | TriggerStatus.ReportGoalScored -> reportGoalScored()

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()