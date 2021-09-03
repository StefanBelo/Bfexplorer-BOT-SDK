(*
    Copyright © 2021, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.Betfair.StreamingAPI

open System
open System.Diagnostics
open System.Collections.ObjectModel

open BeloSoft.Data
open BeloSoft.Net

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service

open BeloSoft.Betfair.StreamingAPI
open BeloSoft.Betfair.StreamingAPI.Models

[<AutoOpen>]
module BetEventFilterOperations =

    /// <summary>
    /// toMarketFilter
    /// </summary>
    /// <param name="betEventFilter"></param>
    let toMarketFilter (betEventFilter : BetEventFilter) =
        let marketFilter = MarketFilter()

        for parameter in betEventFilter do
            match parameter with
            | MarketIds marketIds -> marketFilter.marketIds <- marketIds
            | BspMarketOnly bspOnly -> marketFilter.bspMarket <- Nullable bspOnly
            // marketFilter.bettingTypes
            | BetEventTypeIds eventTypeIds -> marketFilter.eventTypeIds <- eventTypeIds
            | BetEventIds eventIds -> marketFilter.eventIds <- eventIds
            | TurnInPlayEnabled turnInPlayEnabled -> marketFilter.turnInPlayEnabled <- Nullable turnInPlayEnabled
            | MarketTypeCodes marketTypeCodes -> marketFilter.marketTypes <- marketTypeCodes
            | Venues venues -> marketFilter.venues <- venues
            | Countries countries -> marketFilter.countryCodes <- countries
            | _ -> ()

        marketFilter

/// <summary>
/// MarketUpdateService
/// </summary>
type MarketUpdateService(bfexplorerService : IBfexplorerService) as this =
    inherit BetfairStreamingServiceProvider(bfexplorerService)

    let markets = ObservableCollection<Market>()
    let onMarketsOpened = Event<Market list>()

    let serviceStatus = bfexplorerService.ServiceStatus
    let exchangeRate = serviceStatus.UserCurrency.ExchangeRate

    let mutable initialClk = nil<string>
    let mutable clk = nil<string>

    let doSubscribe (filter : BetEventFilter, dataFilter : MarketDataFilterData) =
        let marketFilter = toMarketFilter filter

        this.PostRequest(MarketSubscriptionRequest.Create initialClk clk Int64.MaxValue this.UpdateInterval marketFilter dataFilter)

    let openNewMarket (marketIds : MarketId list) = async {
        let! result = bfexplorerService.GetMarkets(marketIds |> List.toArray)

        if result.IsSuccessResult
        then
            let newMarkets = result.SuccessResult

            lock markets (fun () -> markets.AddRange(newMarkets))

            onMarketsOpened.Trigger newMarkets
    }

    let getMarket (marketId : MarketId) =
        lock markets (fun () -> markets |> Seq.tryFind (fun market -> market.Id = marketId))

    let canSimulateBetProcessingInPracticeMode (market : Market) =
        serviceStatus.IsPracticeMode && 
            (
                let marketBets = market.Bets
            
                marketBets.ShouldConfirmBetOperation || marketBets.HaveUnmatchedBets
            )

    let updateMarketData (marketChangeData : MarketChangeData, updateTime : DateTime) (market : Market) =
        market.ClearUpdated()

        updateMarketData marketChangeData market updateTime exchangeRate
    
        market.UpdateProfitBalance()

        if canSimulateBetProcessingInPracticeMode market
        then
            simulateBetProcessingInPracticeMode updateTime market
            
        serviceStatus |> MyBots.Execute market

    let updateMarkets (marketChangeDatas : MarketChangeData[]) =
        let updateTime = DateTime.Now

        let mutable newMarkets = list<string>.Empty
                      
        marketChangeDatas 
        |> Array.iter (fun marketChangeData -> 
                let marketId = marketChangeData.id

                match getMarket marketId with
                | Some market -> updateMarketData (marketChangeData, updateTime) market
                | None -> newMarkets <- newMarkets @ [ marketId ]
            )

        newMarkets

    /// <summary>
    /// Markets
    /// </summary>
    member _this.Markets
        with get() = markets

    /// <summary>
    /// UpdateInterval
    /// </summary>
    member val UpdateInterval : int64 = 1000L with get, set

    /// <summary>
    /// OnMarketsOpened
    /// </summary>
    [<CLIEvent>]
    member _this.OnMarketsOpened = onMarketsOpened.Publish

    /// <summary>
    /// Restart
    /// </summary>    
    override _this.Restart() = async {
        return Result.Failure "Not implemented!"
    }

    /// <summary>
    /// ProcessMessage
    /// </summary>
    /// <param name="data"></param>
    override this.ProcessMessage(data : string) = async {
        match data with
        | MarketChange response ->
                
            try
                let message = Json.Deserialize<MarketChangeResponse>(response)

                let doUpdate =
                    match message.ct with
                    | ChangeType.UPDATE -> clk <- message.clk; true
                    | ChangeType.SUB_IMAGE 
                    | ChangeType.RESUB_DELTA -> initialClk <- message.initialClk; true
                    | _ -> false

                let! newMarketIds = 
                    this.ExecuteOnUiContextAndReturn <|
                        (fun () ->
                            if doUpdate && isNotNullObj message.mc
                            then
                                updateMarkets message.mc
                            else
                                list.Empty
                        )

                if not newMarketIds.IsEmpty
                then
                    do! openNewMarket newMarketIds
            with
            | ex -> Debug.WriteLine (toFailureMessage ex)

        | _ -> ()
    }

    /// <summary>
    /// Subscribe
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="dataFilter"></param>
    member this.Subscribe(filter : BetEventFilter, dataFilter : MarketDataFilterData) = async {
        match! this.Start() with
        | Result.Success ->

            match! doSubscribe (filter, dataFilter) with
            | Result.Success ->

                do! Async.Sleep 1000

                return 
                    if this.ConnectionHasSuccessStatus
                    then
                        Result.Success
                    else
                        Result.Failure this.ErrorMessage

            | Result.Failure errorMessage -> return Result.Failure errorMessage
                
        | Result.Failure errorMessage -> return Result.Failure errorMessage
    }