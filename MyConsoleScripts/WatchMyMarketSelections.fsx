#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let getMySelection(market : Market) : Selection option =
    let favourites =
        market.Selections
        |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Active)
        |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
        |> Seq.take 2
        |> Seq.toArray

    let favourite = favourites.[0]
    let secondFavourite = favourites.[1]

    if GetPriceProbability(favourite.LastPriceTraded) - GetPriceProbability(secondFavourite.LastPriceTraded) >= 5.0
    then
        Some favourite
    else
        None

let bfexplorer : IBfexplorerConsole = nil

async {
    let markets = bfexplorer.OpenMarkets |> Seq.sortBy (fun market -> market.MarketInfo.StartTime)

    for market in markets do
        getMySelection(market)
        |> Option.iter (fun selection ->
                bfexplorer.WatchMarketSelections([ { Market = market; Selection = selection } ])
                printfn "Watch the market: %s, selection: %s" market.MarketFullName selection.Name
            )
}
|> Async.RunSynchronously