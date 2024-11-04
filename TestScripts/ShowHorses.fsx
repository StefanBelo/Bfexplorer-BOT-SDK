#I @"C:\Program Files\BeloSoft\Bfexplorer\"

#r "BeloSoft.Data.dll"
#r "BeloSoft.Betfair.API.dll"

#load "MyBetfairCredentials.fsx"
open MyBetfairCredentials

open System.Net

open BeloSoft.Betfair.API
open BeloSoft.Betfair.API.Models

ServicePointManager.Expect100Continue <- false
ServicePointManager.UseNagleAlgorithm <- false

let betfairServiceProvider = BetfairServiceProvider()

let accountOperations = betfairServiceProvider.AccountOperations
let browsingOperations = betfairServiceProvider.BrowsingOperations

let showHorsesData (marketCatalogues : MarketCatalogue seq) =
    marketCatalogues
    |> Seq.iter (fun marketCatalogue -> marketCatalogue.runners |> Seq.iter (fun runner -> printfn "%d: %s" runner.selectionId runner.runnerName))

let showHorseNames country = async {
    let! resultLogin = accountOperations.Login(username, password)

    if resultLogin.IsSuccessResult
    then
        let filter = 
            createMarketFilterParameters()
            |> withMarketFilterParameter (MarketFilterParameter.EventTypeIds [| 7 |])
            |> withMarketFilterParameter (MarketFilterParameter.MarketTypeCodes [| "WIN" |])
            |> withMarketFilterParameter (MarketFilterParameter.MarketCountries [| country |])

        let marketProjection = [| MarketProjection.EVENT; MarketProjection.MARKET_START_TIME; MarketProjection.COMPETITION; MarketProjection.RUNNER_DESCRIPTION; MarketProjection.MARKET_DESCRIPTION |]

        let! resultMarketCatalogues = browsingOperations.GetMarketCatalogues(filter, 100, marketProjection, MarketSort.FIRST_TO_START)

        if resultMarketCatalogues.IsSuccessResult
        then
            showHorsesData resultMarketCatalogues.SuccessResult

        do! accountOperations.Logout() |> Async.Ignore
}

showHorseNames "GB" |> Async.RunSynchronously

