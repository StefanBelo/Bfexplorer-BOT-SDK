(*
    Copyright © 2021, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.Betfair.StreamingAPI

open System.Collections.ObjectModel
open System.Threading.Tasks.Dataflow

open BeloSoft.Data

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service

/// <summary>
/// MarketUpdateService
/// </summary>
type MarketUpdateService(bfexplorerService : BfexplorerService) as this =

    let markets = ObservableCollection<Market>()
    let onMarketsOpened = Event<Market list>()

    let getActiveMarkets() =
        lock markets (fun () -> markets |> Seq.filter (isClosedMarket >> not) |> Seq.toList)

    let marketMonitoring = ActionBlock<Market list>(fun marketsToUpdate ->
        async {
            try
                if not marketsToUpdate.IsEmpty
                then
                    let myMarketsToUpdate = List.Split 15 marketsToUpdate

                    for myMarkets in myMarketsToUpdate do
                        do! bfexplorerService.UpdateMarkets(myMarkets) |> Async.Ignore

                do! Async.Sleep(this.UpdateInterval)
            finally
                this.DoMarketMonitoring()
        }
        |> Async.RunSynchronously
    )

    /// <summary>
    /// DoMarketMonitoring
    /// </summary>
    member private _this.DoMarketMonitoring() =
        let marketsToUpdate = getActiveMarkets()

        if not marketsToUpdate.IsEmpty
        then
            marketMonitoring.Post(marketsToUpdate) |> ignore

    /// <summary>
    /// Markets
    /// </summary>
    member _this.Markets
        with get() = markets

    /// <summary>
    /// UpdateInterval
    /// </summary>
    member val UpdateInterval : int = 1000 with get, set

    /// <summary>
    /// OnMarketsOpened
    /// </summary>
    [<CLIEvent>]
    member _this.OnMarketsOpened = onMarketsOpened.Publish

    /// <summary>
    /// Start
    /// </summary>
    /// <param name="filter"></param>
    member this.Start(filter : BetEventFilter) = async {
        let! marketCataloguesResult = bfexplorerService.GetMarketCatalogues(filter, 1000)

        if marketCataloguesResult.IsSuccessResult
        then
            let marketIds = 
                marketCataloguesResult.SuccessResult
                |> Seq.map (fun marketCatalogue -> marketCatalogue.MarketInfo.Id)
                |> Seq.toArray

            if marketIds.Length > 0
            then
                let! marketsResult = bfexplorerService.GetMarkets(marketIds)

                return
                    if marketsResult.IsSuccessResult
                    then
                        let newMarkets = marketsResult.SuccessResult

                        lock markets (fun () -> markets.AddRange(newMarkets))

                        onMarketsOpened.Trigger newMarkets

                        this.DoMarketMonitoring()

                        Result.Success
                    else
                        Result.Failure marketsResult.FailureMessage
            else
                return Result.Failure "No markets!"
        else
            return Result.Failure marketCataloguesResult.FailureMessage
    }

    /// <summary>
    /// Stop
    /// </summary>
    member _this.Stop() =
        marketMonitoring.Complete()