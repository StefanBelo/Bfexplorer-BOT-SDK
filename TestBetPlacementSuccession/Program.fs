(*
    Copyright © 2022 - 2024, Stefan Belopotocan, http://bfexplorer.net
*)

namespace TestBetPlacementSuccession

open BeloSoft.Data
open BeloSoft.Bfexplorer
open BeloSoft.Bfexplorer.Service

/// <summary>
/// PlaceBetTestType          
/// </summary>
type PlaceBetTestType =
    | RestApiInOneApiCall
    | RestApiBetByBet
    | StreamingApiInOneApiCall
    | StreamingApiBetByBet

    member this.IsStreamingApiTest
        with get () =
            match this with
            | StreamingApiInOneApiCall | StreamingApiBetByBet -> true
            | _ -> false

module Program =

    /// <summary>
    /// main
    /// </summary>
    /// <param name="argv"></param>
    [<EntryPoint>]
    let main argv =
        match getBetfairCredentialsAndMarketId argv with
        | Some (userName, password, marketId) ->

            let placeBetTestType = PlaceBetTestType.StreamingApiInOneApiCall

            let bfexplorerService = BfexplorerService (initializeBotManager = false, UiApplication = BfexplorerHost ())

            async {
                let! loginResult = bfexplorerService.Login (userName, password)

                if loginResult.IsSuccessResult
                then                   
                    let useStreamingApiTest = placeBetTestType.IsStreamingApiTest

                    if useStreamingApiTest
                    then
                        setStreamingApiDataContext ()

                    match! bfexplorerService.GetMarket marketId with
                    | DataResult.Success market ->

                        let streamingApiTestService = StreamingApiTestService (bfexplorerService, market)

                        if useStreamingApiTest
                        then                            
                            streamingApiTestService.Start () |> Async.Start

                            do! Async.Sleep 2000    
                        
                        let doExecute =
                            match placeBetTestType with
                            | RestApiInOneApiCall -> RestApiTest.ExecuteInOneApiCall
                            | RestApiBetByBet -> RestApiTest.Execute
                            | StreamingApiInOneApiCall -> StreamingApiTest.ExecuteInOneApiCall
                            | StreamingApiBetByBet -> StreamingApiTest.Execute

                        match! bfexplorerService |> doExecute market with
                        | Result.Success -> 
                               
                            sprintf "Placed bets:\n\n%s" (
                                market.Bets
                                |> Seq.map (fun bet -> bet.ToString ())
                                |> String.concat "\n"
                            )
                            |> report

                            do! bfexplorerService.CancelBets market |> Async.Ignore

                        | Result.Failure errorMessage -> report errorMessage

                    | DataResult.Failure errorMessage -> report errorMessage
                
                    do! bfexplorerService.Logout () |> Async.Ignore
            }
            |> Async.RunSynchronously

        | None -> failwith "Please enter your betfair user name and password and a valid market id!"

        0 // return an integer exit code