// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.
open System
open BeloSoft.Betfair.API
open BeloSoft.Betfair.API.Models

[<EntryPoint>]
let main argv =
    if argv.Length <> 2
    then
        failwith "Please enter your betfair user name and password!"
       
    // Your betfair login credentials     
    let username, password = argv.[0], argv.[1]

    let betfairServiceProvider = BetfairServiceProvider()
       
    let accountOperations = betfairServiceProvider.AccountOperations
    let browsingOperations = betfairServiceProvider.BrowsingOperations

    // Show market catalog data
    let showMarketCatalogue (marketCatalogue : MarketCatalogue) =    
        let betEvent = marketCatalogue.event                        

        printfn "%A: %s, eventId: %s, marketId: %s, %.2f" betEvent.openDate betEvent.name betEvent.id marketCatalogue.marketId marketCatalogue.totalMatched

    // Get market books (market selections prices)
    let getMarketBooks (marketCatalogue : MarketCatalogue) = async {
        let! result = browsingOperations.GetMarketBooks([| marketCatalogue.marketId |], priceProjection = PriceProjection.DefaultActiveMarket())

        if result.IsSuccessResult
        then
            let marketBook = result.SuccessResult.[0]
            let betEvent = marketCatalogue.event

            printfn "\nMarket: %A, %s\nSelections:\n" betEvent.openDate betEvent.name
                    
            Seq.iter2 (fun (runner : RunnerCatalog) (runnerData : Runner) -> printfn "\t%s: %.2f, %.2f" runner.runnerName runnerData.lastPriceTraded runnerData.totalMatched)
                marketCatalogue.runners marketBook.runners
        else
            printfn "%s" result.FailureMessage
    }

    // Test
    async {                   
        let! loginResult = accountOperations.Login(username, password)        

        if loginResult.IsSuccessResult
        then
            let filter = 
                createMarketFilterParameters()
                |> withMarketFilterParameter (MarketStartTime (TimeRange.Today()))
                //|> withMarketFilterParameter (MarketCountries [| "GB" |])
                |> withMarketFilterParameter (EventTypeIds [| 1 |])
                |> withMarketFilterParameter (MarketTypeCodes [| "MATCH_ODDS" |])

            let marketProjection = [| MarketProjection.EVENT; MarketProjection.MARKET_START_TIME; MarketProjection.COMPETITION; MarketProjection.RUNNER_DESCRIPTION; MarketProjection.MARKET_DESCRIPTION |]
            
            let! marketCataloguesResult = browsingOperations.GetMarketCatalogues(filter, 5, marketProjection, MarketSort.MAXIMUM_TRADED)

            if marketCataloguesResult.IsSuccessResult
            then
                let marketCatalogues = marketCataloguesResult.SuccessResult

                printfn "Market catalogs:\n"

                marketCatalogues |> Seq.iter showMarketCatalogue

                for marketCatalogue in marketCatalogues do
                    do! getMarketBooks marketCatalogue

            do! accountOperations.Logout() |> Async.Ignore
    }
    |> Async.RunSynchronously
    
    printfn "\nEnd of the test."
   
    Console.Read()