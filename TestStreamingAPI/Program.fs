(*
    Copyright © 2021, Stefan Belopotocan, http://bfexplorer.net
*)

module BeloSoft.Betfair.StreamingAPI.Test

open System
open System.ComponentModel

open BeloSoft.Betfair.StreamingAPI.Models

open BeloSoft.Bfexplorer
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Domain

/// <summary>
/// setNotify
/// </summary>
/// <param name="market"></param>
let setNotify (market : Market) = 
    (market :> INotifyPropertyChanged).PropertyChanged.Add(fun arg -> 
        if arg.PropertyName = "TotalMatched" 
        then 
            printfn "%A: %.2f" market market.TotalMatched
    )

/// <summary>
/// main
/// </summary>
/// <param name="argv"></param>
[<EntryPoint>]
let main argv =
    if argv.Length <> 2
    then
        failwith "Please enter your betfair user name and password!"

    let bfexplorerService = BfexplorerService(UiApplication = BfexplorerHost(), initializeBotManager = false)

    async {
        let! loginResult = 
            let userName, password = argv.[0], argv.[1]

            bfexplorerService.Login(userName, password)

        if loginResult.IsSuccessResult
        then
            let marketUpdateService = new MarketUpdateService(bfexplorerService)

            marketUpdateService.OnMarketsOpened.Add(fun markets -> markets |> List.iter (fun market -> printfn "%A" market; setNotify market))

            let! startResult = marketUpdateService.Start()

            if startResult.IsSuccessResult
            then
                (* Football 
                let filter = [ BetEventFilterParameter.BetEventTypeIds [| 1 |]; BetEventFilterParameter.Countries [| "GB" |]; BetEventFilterParameter.MarketTypeCodes [| "MATCH_ODDS" |] ]
                *)

                (* Tennis
                let filter = [ BetEventFilterParameter.BetEventTypeIds [| 2 |]; BetEventFilterParameter.MarketTypeCodes [| "MATCH_ODDS" |]; ]
                *)
            
                (* Horse Racing *)
                let filter = [ BetEventFilterParameter.BetEventTypeIds [| 7 |]; BetEventFilterParameter.MarketTypeCodes [| "WIN" |]; BetEventFilterParameter.Countries [| "GB" |] ]
            
                (* Greyhound Racings
                let filter = [ BetEventFilterParameter.StartTime (DateRange.Today()); BetEventFilterParameter.BetEventTypeIds [| 4339 |]; BetEventFilterParameter.MarketTypeCodes [| "WIN" |]; BetEventFilterParameter.Countries [| "GB" |] ]
                *)
            
                let! subscribeResult = marketUpdateService.Subscribe(filter, StreamingData.MarketDataFilterForPassiveMarkets)

                if subscribeResult.IsSuccessResult
                then
                    if marketUpdateService.ConnectionHasSuccessStatus
                    then
                        printfn "Successfully subscribed."
                    else
                        printfn "Failed to subscribe: %s" marketUpdateService.ErrorMessage
                else
                    printfn "Failed to subscribe: %s" subscribeResult.FailureMessage

                printfn "Press any key to exit."

                Console.ReadKey() |> ignore

                do! marketUpdateService.Stop()
                                                            
            do! bfexplorerService.Logout() |> Async.Ignore
    }
    |> Async.RunSynchronously

    0 // return an integer exit code