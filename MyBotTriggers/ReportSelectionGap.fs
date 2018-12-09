// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module ReportSelectionGap

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
        
/// <summary>
/// ReportSelectionGap
/// </summary>
type ReportSelectionGap(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let bfexplorerService = myBfexplorer.BfexplorerService
        
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            if market.IsInPlay
            then
                EndExecution
            else
                let priceDifference = selection.GetOfferedPriceDifference()

                if priceDifference >= 2
                then
                    bfexplorerService.OutputMessage(sprintf "%s: %.2f | %d | %.2f" selection.Name selection.LastPriceTraded priceDifference (selection.GetWeightOfMoney()))
            
                WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()