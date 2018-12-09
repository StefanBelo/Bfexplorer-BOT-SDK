// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

namespace MyFsharpBot

open System

open BeloSoft.Betfair
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// BetfairSpPrices
/// </summary>
type BetfairSpPrices() =
    member val NearPrice : float = 0.0 with get, set
    member val FarPrice : float = 0.0 with get, set
    member val ActualSP : float = 0.0 with get, set

/// <summary>
/// UpdateSpPricesBotParameters
/// </summary>
[<Sealed>]
type UpdateSpPricesBotParameters() =
    inherit BotParameters()
    
    member val UpdateInterval : TimeSpan = TimeSpan.FromSeconds(5.0) with get, set

module MyBotData =

    let BetfairSpPriceProjection = API.Models.PriceProjection.DefaultBetfairSpPrice()

    let UpdateSelectionSpPrices(selection : Selection, runner : API.Models.Runner) =
        let betfairSpPrices = selection.Data.["bsp"] :?> BetfairSpPrices
        let spPrices = runner.sp

        betfairSpPrices.NearPrice <- spPrices.nearPrice
        betfairSpPrices.FarPrice <- spPrices.farPrice
        betfairSpPrices.ActualSP <- spPrices.actualSP

/// <summary>
/// UpdateSpPricesMarketBot
/// </summary>
[<Sealed>]
type UpdateSpPricesMarketBot(market : Market, parameters : UpdateSpPricesBotParameters, bfexplorerService : IBfexplorerService) as this =
    inherit MarketBot(market, parameters, bfexplorerService)

    let mutable timeToUpdated = DateTime.MinValue

    let isBetfairSpSettled() =
        let selection = market.Selections |> Seq.find isActiveSelection
        
        (selection.Data.["bsp"] :?> BetfairSpPrices).ActualSP <> 0.0

    do
        this.Status <- BotStatus.WaitingForEntryCriteria

        market.Selections
        |> Seq.iter (fun selection -> selection.Data.["bsp"] <- BetfairSpPrices())

    override this.Execute() =
        match this.Status with
        | BotStatus.WaitingForEntryCriteria -> 

            if this.EvaluateCriteria()
            then
                this.Status <- BotStatus.WaitingForOperation

        | BotStatus.WaitingForOperation ->

            if DateTime.Now >= timeToUpdated
            then
                this.UpdateMarketData(MyBotData.BetfairSpPriceProjection, MyBotData.UpdateSelectionSpPrices, fun _ -> ())
                timeToUpdated <- DateTime.Now.Add(parameters.UpdateInterval)
            
            if market.IsInPlay && isBetfairSpSettled()
            then                
                this.Status <- BotStatus.ExecutionEnded

        | _ -> ()