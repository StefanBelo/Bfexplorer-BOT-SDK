#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

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

let GetIsMyFavouriteMarket(market : Market) =
    let favourite = GetFavouriteTeam(market)
    let price = favourite.GetBestPrice(BetType.Back)
    printfn "%s %s: %.2f" market.MarketFullName favourite.Name price

    price <= 1.5

let GetIsMyDrawMarket(market : Market) =
    let draw = market.Selections.[2]
    let price = draw.GetBestPrice(BetType.Back)
    printfn "%s %s: %.2f" market.MarketFullName draw.Name price

    price <= 4.0

let GetIsMyCorrectScoreAndOverMarket(markets : Market[]) =
    let correctScoreMarket = markets.[0]
    let overUnderMarket = markets.[1]

    let score0_0 = correctScoreMarket.Selections.[0]
    let scorePrice = score0_0.GetBestPrice(BetType.Back)
    printfn "%s %s: %.2f" correctScoreMarket.MarketFullName score0_0.Name scorePrice

    let over_25 = overUnderMarket.Selections.[1]
    let overPrice = over_25.GetBestPrice(BetType.Back)
    printfn "%s %s: %.2f" overUnderMarket.MarketFullName over_25.Name overPrice

    scorePrice >= 15.0 && overPrice <= 1.8

let bfexplorer : IBfexplorerConsole = nil

async {
    if GetIsFootballMatchOdds(bfexplorer.BetEventBrowserMarketCatalogs)
    then
        let marketInfos =
            bfexplorer.BetEventBrowserMarketCatalogs
            |> Seq.filter (fun marketCatalog -> marketCatalog.TotalMatched >= 50000.0)
            |> Seq.map (fun marketCatalog -> marketCatalog.MarketInfo)
            |> Seq.toArray

        if marketInfos.Length > 0
        then
            let! result = bfexplorer.GetMyMarkets(marketInfos)

            if result.IsSuccessResult
            then
                let marketsToOpen = result.SuccessResult |> Array.filter GetIsMyDrawMarket

                if marketsToOpen.Length > 0
                then
                    printf "%d qualified Match Odds markets." marketsToOpen.Length

                    for market in marketsToOpen do
                        let! resultAssociatedMarkets = bfexplorer.GetMyAssociatedMarkets(market.MarketInfo, [| "CORRECT_SCORE"; "OVER_UNDER_25" |])

                        if resultAssociatedMarkets.IsSuccessResult
                        then
                            let associatedMarkets = resultAssociatedMarkets.SuccessResult

                            if GetIsMyCorrectScoreAndOverMarket(associatedMarkets)
                            then
                                bfexplorer.OpenMyMarkets( [ market ] @ (associatedMarkets |> Array.toList))
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