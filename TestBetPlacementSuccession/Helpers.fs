(*
    Copyright © 2022, Stefan Belopotocan, http://bfexplorer.net
*)

[<AutoOpen>]
module Helpers

open System

open BeloSoft.Betfair.StreamingAPI.Models
open BeloSoft.Bfexplorer.Domain

[<Literal>]
let MumberOfBetsToPlace = 10

/// <summary>
/// getBetfairCredentialsAndMarketId
/// </summary>
/// <param name="argv"></param>
let getBetfairCredentialsAndMarketId (argv : string []) =
    if argv.Length = 3 
    then 
        Some (argv.[0], argv.[1], argv.[2]) 
    else 
        None

/// <summary>
/// report
/// </summary>
/// <param name="message"></param>
let report (message : string) = 
    Console.WriteLine message

/// <summary>
/// setStreamingApiDataContext
/// </summary>
let setStreamingApiDataContext () =
    DataContext.CreatePriceGridDataContext <- fun (priceGridData : PriceGridData) -> PriceGridDataContextStreaming (priceGridData) :> IPriceGridDataContext
    DataContext.UseMarketStreaming <- true

    SelectionExtensions.InitializeSelection <- fun (selection : Selection) -> selection.PriceGridDataEnabled <- true

/// <summary>
/// createMySelectionBets
/// </summary>
/// <param name="market"></param>
let createMySelectionBets (market : Market) =    
    let selection = market.Selections.[0]

    let prices =
        let oddsContext = selection.OddsContext
        
        let mutable index = 0

        let mutable price = oddsContext.GetNextOdds (selection.LastPriceTraded, 10, true)

        [
            while index < MumberOfBetsToPlace && price < OddsData.MaximalOdds do
                price <- oddsContext.GetNextOdds(price, 1, true)
                index <- index + 1

                yield price
        ]

    selection, prices |> List.map (fun price -> BetOrder.Create(BetType.Back, price, 2.0))