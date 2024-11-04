#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let GetIsFootballMatchOdds(marketCatalogs : MarketCatalog list) =
    if marketCatalogs.IsEmpty
    then
        false
    else
        let marketInfo = marketCatalogs.Head.MarketInfo

        marketInfo.BetEventType.Id = 1 (* Football *) && marketInfo.MarketName = "Match Odds"

let GetFavouriteTeam(market : Market) =
    let selections = market.Selections

    let homeTeam = selections.[0]
    let awayTeam = selections.[1]

    if homeTeam.GetBestPrice(BetType.Back) < awayTeam.GetBestPrice(BetType.Back)
    then
        homeTeam
    else
        awayTeam

let GetIsMyMarket(market : Market) =
    let favourite = GetFavouriteTeam(market)
    let price = favourite.GetBestPrice(BetType.Back)

    //printfn "%s: %.2f" favourite.Name price
    price <= 1.5

let bfexplorer : IBfexplorerConsole = nil

async {
    if GetIsFootballMatchOdds(bfexplorer.BetEventBrowserMarketCatalogs)
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
        printf "Please load Football Match Odds markets."
}
|> Async.RunSynchronously