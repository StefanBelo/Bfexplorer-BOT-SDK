// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module ReopenMarketBotTrigger

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | DeactivateMonitoring
    | Reopen
    | StartMyActionBot

/// <summary>
/// ReopenMarketBotTrigger
/// </summary>
type ReopenMarketBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.DeactivateMonitoring

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | DeactivateMonitoring ->
                DeactiveMarketMonitoringStatus(market)

                market.Selections
                |> Seq.iter (fun mySelection -> 
                        if mySelection.Status = SelectionStatus.Active
                        then
                            mySelection.PriceGridDataEnabled <- true
                    )

                status <- TriggerStatus.Reopen
                WaitingForOperation
            | Reopen ->
                status <- TriggerStatus.StartMyActionBot
                WaitingForOperation
            | StartMyActionBot ->
                ExecuteActionBot

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()