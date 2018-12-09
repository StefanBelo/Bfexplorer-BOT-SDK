// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module ReportGap

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
        
/// <summary>
/// ReportGap
/// </summary>
type ReportGap(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

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
                market.Selections
                |> Seq.iter (fun s ->
                        if s.Status = SelectionStatus.Active && s.GetBestPrice(BetType.Back) <= 10.0
                        then
                            let priceDifference = s.GetOfferedPriceDifference()

                            if priceDifference >= 2
                            then
                                bfexplorerService.OutputMessage(sprintf "%s: %.2f | %d | %.2f" s.Name s.LastPriceTraded priceDifference (s.GetWeightOfMoney()))
                    )
            
                WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()