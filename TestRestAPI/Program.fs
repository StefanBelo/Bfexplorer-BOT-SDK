(*
    Copyright © 2021, Stefan Belopotocan, http://bfexplorer.net
*)

module BeloSoft.Betfair.API.Test

open System
open System.ComponentModel

open BeloSoft.Bfexplorer
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Domain

let getUserNameAndPassword (argv : string[]) =
    if argv.Length = 2
    then
        Some (argv.[0], argv.[1])
    else
        None

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
    match getUserNameAndPassword argv with
    | Some (userName, password) ->

        let bfexplorerService = BfexplorerService(UiApplication = BfexplorerHost(), initializeBotManager = false)

        async {
            let! loginResult = bfexplorerService.Login(userName, password)

            if loginResult.IsSuccessResult
            then
                let marketUpdateService = new MarketUpdateService(bfexplorerService)

                marketUpdateService.OnMarketsOpened.Add(fun markets -> markets |> List.iter (fun market -> printfn "%A" market; setNotify market))

                (* Horse Racing *)
                let filter = [ BetEventFilterParameter.BetEventTypeIds [| 7 |]; BetEventFilterParameter.MarketTypeCodes [| "WIN" |]; BetEventFilterParameter.Countries [| "GB" |] ]

                let! subscribeResult = marketUpdateService.Start(filter)

                if subscribeResult.IsSuccessResult
                then
                    printfn "Successfully subscribed."
                else
                    printfn "Failed to subscribe: %s" subscribeResult.FailureMessage

                printfn "Press any key to exit."

                Console.ReadKey() |> ignore

                marketUpdateService.Stop()
                                                        
                do! bfexplorerService.Logout() |> Async.Ignore
        }
        |> Async.RunSynchronously

    | None -> failwith "Please enter your betfair user name and password!"

    0 // return an integer exit code