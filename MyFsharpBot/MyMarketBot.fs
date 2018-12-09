// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

namespace MyFsharpBot

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// MyMarketBotParameters
/// </summary>
[<Sealed>]
type MyMarketBotParameters() =
    inherit BotParameters()
    
    member val NumberOfIterations : byte = 10uy with get, set

/// <summary>
/// MyMarketBot
/// </summary>
[<Sealed>]
type MyMarketBot(market : Market, parameters : MyMarketBotParameters, bfexplorerService : IBfexplorerService) =
    inherit MarketBot(market, parameters, bfexplorerService)

    let mutable iteration = 0uy

    override this.Execute() =
        if iteration < parameters.NumberOfIterations
        then
            this.OutputMarketMessage(sprintf "Total matched: %.2f" market.TotalMatched)
        else
            this.Status <- BotStatus.ExecutionEnded

        iteration <- iteration + 1uy