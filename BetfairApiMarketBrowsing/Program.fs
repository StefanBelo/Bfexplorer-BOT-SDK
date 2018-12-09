// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

open System

open BeloSoft.Data
open BeloSoft.Betfair.API
open BeloSoft.Betfair.API.Models

[<EntryPoint>]
let main argv =
    if argv.Length <> 2
    then
        failwith "Please enter your betfair user name and password!"        
       
    let username, password = argv.[0], argv.[1]

    let betfairServiceProvider = BetfairServiceProvider()

    let accountOperations = betfairServiceProvider.AccountOperations
    let browsingOperations = betfairServiceProvider.BrowsingOperations

    async {   
                 
        let! loginResult = accountOperations.Login(username, password)        

        if loginResult.IsSuccessResult
        then
            let today = DateTime.Today

            let filter = 
                createMarketFilterParameters()
                |> withMarketFilterParameter (MarketStartTime (TimeRange.FromRange(today, today.AddDays(1.0))))
                //|> withMarketFilterParameter (MarketCountries [| "GB" |])
                |> withMarketFilterParameter (EventTypeIds [| 1 |])
                |> withMarketFilterParameter (TurnInPlayEnabled true)
                |> withMarketFilterParameter (InPlayOnly false)

            let marketProjection = [| MarketProjection.EVENT; MarketProjection.MARKET_START_TIME |]
            
            let! marketCataloguesResult = browsingOperations.GetMarketCatalogues(filter, 1000, marketProjection, MarketSort.FIRST_TO_START)

            if marketCataloguesResult.IsSuccessResult
            then
                let marketCatalogues = marketCataloguesResult.SuccessResult

                marketCatalogues
                |> Seq.filter (fun marketCatalogue -> marketCatalogue.marketName.StartsWith "Over/Under")
                |> Seq.iter (fun marketCatalogue -> 
                        let betEvent = marketCatalogue.event

                        printfn "%A: %s - %s, marketId: %s" 
                            betEvent.openDate betEvent.name marketCatalogue.marketName marketCatalogue.marketId
                    )

            do! accountOperations.Logout() |> Async.Ignore
    }
    |> Async.RunSynchronously
    
    0 // return an integer exit code