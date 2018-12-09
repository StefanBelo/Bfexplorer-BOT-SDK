// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module PriceComesBackActionBotTrigger

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | WaitingForFromPrice
    | FromPriceReached
    
/// <summary>
/// PriceComesBackActionBotTrigger
/// </summary>
type PriceComesBackActionBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    
    let fromPrice = defaultArg (botTriggerParameters.GetParameter<float>("FromPrice")) 5.0
    let backToPrice = defaultArg (botTriggerParameters.GetParameter<float>("BackToPrice")) 3.1
    let isFromPriceReached = if fromPrice > backToPrice then (>=) else (<=)
    let isBackToPriceReached = if fromPrice > backToPrice then (<=) else (>=)

    let mutable status = TriggerStatus.WaitingForFromPrice

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =

            let lastPriceTraded = selection.LastPriceTraded
            
            match status with
            | TriggerStatus.WaitingForFromPrice ->

                if isFromPriceReached lastPriceTraded fromPrice
                then
                    status <- TriggerStatus.FromPriceReached

                WaitingForOperation

            | TriggerStatus.FromPriceReached ->

                if isBackToPriceReached lastPriceTraded backToPrice
                then
                    ExecuteActionBot
                else
                    WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()