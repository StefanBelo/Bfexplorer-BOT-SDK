// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

namespace MyFsharpBot

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// ShowSelectionSpPricesBot
/// </summary>
[<Sealed>]
type ShowSelectionSpPricesBot(market : Market, selection : Selection, parameters : BotParameters, bfexplorerService : IBfexplorerService) as this =
    inherit SelectionBot(market, selection, parameters, bfexplorerService)

    let myLastPriceTraded = HistoryValue<float>()

    do
        this.Status <- 
            if selection.Data.ContainsKey("bsp") 
            then 
                BotStatus.WaitingForOperation
            else 
                BotStatus.ExecutionEnded

    override this.Execute() =
        if selection.IsUpdated && myLastPriceTraded.SetValue(selection.LastPriceTraded)
        then
            let betfairSpPrices = selection.Data.["bsp"] :?> BetfairSpPrices

            this.OutputMessage(sprintf "Last price traded: %.2f, SP near: %.2f, far: %.2f, actual: %.2f" myLastPriceTraded.Value betfairSpPrices.NearPrice betfairSpPrices.FarPrice betfairSpPrices.ActualSP)