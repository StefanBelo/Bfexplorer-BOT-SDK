// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

namespace MyFsharpBot

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// ReportWinnerBot
/// </summary>
[<Sealed>]
type ReportWinnerBot(market : Market, parameters : BotParameters, bfexplorerService : IBfexplorerService) as this =
    inherit MarketBaseBot(market, parameters, bfexplorerService)

    let reportTheWinner() =
        market.Selections
        |> Seq.filter isWinnerSelection
        |> Seq.map (fun selection -> selection.Name)
        |> String.concat "\n"
        |> this.OutputMarketMessage

    /// <summary>
    /// Execute
    /// </summary>
    override _this.Execute() =
        ()

    /// <summary>
    /// EndExecution
    /// </summary>
    override _this.EndExecution() =
        if market.MarketStatus = MarketStatus.Closed
        then
            reportTheWinner()