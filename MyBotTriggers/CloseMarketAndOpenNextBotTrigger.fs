// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module CloseMarketAndOpenNextBotTrigger

open System

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | WaitToClose
    | ClosingInProgress

/// <summary>
/// CloseMarketAndOpenNextBotTrigger
/// </summary>
type CloseMarketAndOpenNextBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable time = market.MarketInfo.StartTime.Add(TimeSpan.FromSeconds(defaultArg (botTriggerParameters.GetParameter<float>("Time")) -10.0))
    let mutable triggerStatus = TriggerStatus.WaitToClose

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match triggerStatus with
            | TriggerStatus.WaitToClose ->

                if DateTime.Now >= time
                then
                    triggerStatus <- TriggerStatus.ClosingInProgress
                    time <- time.AddSeconds(5.0)

                    TriggerResult.AlertMessage "This market will be closed in five seconds!"
                else
                    TriggerResult.WaitingForOperation

            | TriggerStatus.ClosingInProgress ->

                if DateTime.Now >= time
                then
                    myBfexplorer.BfexplorerService.Bfexplorer.CloseMarket(market, true)

                    TriggerResult.EndExecution
                else
                    TriggerResult.WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()