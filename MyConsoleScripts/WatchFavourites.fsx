#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let GetFavourite(market : Market) =
    market.Selections
    |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Active && selection.LastPriceTraded > 0.0)
    |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
    |> Seq.head

let bfexplorer : IBfexplorerConsole = nil

async {
    bfexplorer.OpenMarkets
    |> Seq.sortBy (fun market -> market.MarketInfo.StartTime)
    |> Seq.iter (fun market -> bfexplorer.WatchMarketSelections([ { Market = market; Selection = GetFavourite(market) } ]))
}
|> Async.RunSynchronously