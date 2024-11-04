#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let GetIsHorseRacingMarket(marketCatalogs : MarketCatalog list) =
    if marketCatalogs.IsEmpty
    then
        false
    else
        let marketInfo = marketCatalogs.Head.MarketInfo

        marketInfo.BetEventType.Id = 7 (* Horse Racing *)

let GetFavouriteHorse(market : Market) =
    market.Selections
    |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Active)
    |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
    |> Seq.head

let GetIsMyMarket(market : Market) =
    let favourite = GetFavouriteHorse(market)
    let price = favourite.GetBestPrice(BetType.Back)

    price <= 4.0

//let bfexplorer : IBfexplorerConsole = nil

async {
    if GetIsHorseRacingMarket(bfexplorer.BetEventBrowserMarketCatalogs)
    then
        let marketInfos =
            bfexplorer.BetEventBrowserMarketCatalogs
            |> Seq.filter (fun marketCatalog -> marketCatalog.TotalMatched >= 5000.0)
            |> Seq.map (fun marketCatalog -> marketCatalog.MarketInfo)
            |> Seq.toArray

        if marketInfos.Length > 0
        then
            let! result = bfexplorer.GetMyMarkets(marketInfos)

            if result.IsSuccessResult
            then
                let marketsToOpen = result.SuccessResult |> Array.filter GetIsMyMarket

                if marketsToOpen.Length > 0
                then
                    bfexplorer.OpenMyMarkets(marketsToOpen)
                    printf "%d qualified markets open." marketsToOpen.Length
                else
                    printf "No markets to open."
            else
                printf "%s" result.FailureMessage
        else
            printf "No markets to open."
    else
        printf "Please load Horse Racing markets."
}
|> Async.RunSynchronously