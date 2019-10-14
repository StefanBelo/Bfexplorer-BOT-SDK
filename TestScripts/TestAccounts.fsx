#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Betfair.API.dll"

#load "MyBetfairCredentials.fsx"
open MyBetfairCredentials

open System
open System.Net

open BeloSoft.Data
open BeloSoft.Betfair.API
open BeloSoft.Betfair.API.Models

ServicePointManager.Expect100Continue <- false
ServicePointManager.UseNagleAlgorithm <- false

let betfairServiceProvider = BetfairServiceProvider()

let accountOperations = betfairServiceProvider.AccountOperations
let browsingOperations = betfairServiceProvider.BrowsingOperations
let raceStatusOperations = betfairServiceProvider.RaceStatusOperations

let login username password = 
    async {
        let! loginResult = accountOperations.Login(username, password)

        return
            if loginResult.IsSuccessResult
            then
                true
            else
                Console.WriteLine(sprintf "Failed to login: %s" loginResult.FailureMessage)
                false
    }
    |> Async.RunSynchronously

let logout() =
    accountOperations.Logout() 
    |> Async.Ignore
    |> Async.RunSynchronously

let getMarketCatalogues() =
    let today = DateTime.Now

    let filter = 
        createMarketFilterParameters()
        |> withMarketFilterParameter (MarketStartTime (TimeRange.FromRange(today, today.AddDays(1.0))))
        |> withMarketFilterParameter (EventTypeIds [| 7 |])
        //|> withMarketFilterParameter (MarketCountries [| "GB" |])
        |> withMarketFilterParameter (MarketTypeCodes [| "WIN" |])
        |> withMarketFilterParameter (InPlayOnly false)

    let marketProjection = [| MarketProjection.EVENT; MarketProjection.MARKET_START_TIME |]
            
    async {
        let! marketCataloguesResult = browsingOperations.GetMarketCatalogues(filter, 10, marketProjection, MarketSort.FIRST_TO_START)

        return marketCataloguesResult
    }
    |> Async.RunSynchronously

let getRaceStatus marketId =
    async {
        let! result = raceStatusOperations.GetListRaceDetails([| marketId |])

        return
            if result.IsSuccessResult
            then
                result.SuccessResult.[0].raceStatus.ToString()
            else
                "Unknown"
    }
    |> Async.RunSynchronously

// My test
if login username password
then
    let result = getMarketCatalogues()

    if result.IsSuccessResult
    then
        result.SuccessResult
        |> Seq.iter (fun marketCatalogue -> 
                Console.WriteLine(sprintf "%A: %s %s | %s" marketCatalogue.marketStartTime marketCatalogue.event.venue marketCatalogue.marketName
                    (getRaceStatus marketCatalogue.marketId)
                )
            )

    logout()