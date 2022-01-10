(*
    Copyright © 2022, Stefan Belopotocan, http://bfexplorer.net
*)

namespace TestBetPlacementSuccession

open System.Diagnostics

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Betfair.StreamingAPI

/// <summary>
/// StreamingApiTestService
/// </summary>
type StreamingApiTestService(bfexplorerService : BfexplorerService, market : Market) =

    let activeMarketStreamingServiceProvider = new ActiveMarketStreamingServiceProvider(bfexplorerService)
    let ordersStreamingServiceProvider = new OrdersStreamingServiceProvider(bfexplorerService, [ market ])
    
    member _this.Start() = async {
        do! ordersStreamingServiceProvider.Start() |> Async.Ignore
    
        let! result = activeMarketStreamingServiceProvider.Start()
    
        if result.IsSuccessResult
        then
            do! activeMarketStreamingServiceProvider.Subscribe(market, 0) |> Async.Ignore
    }

module StreamingApiTest =

    /// <summary>
    /// doCheckAllBetsArePlaced
    /// </summary>
    /// <param name="market"></param>
    let doCheckAllBetsArePlaced (market : Market) =
        market.Bets.Count = MumberOfBetsToPlace

    /// <summary>
    /// doCheckIsBetPlaced
    /// </summary>
    /// <param name="market"></param>
    /// <param name="betOrder"></param>
    let doCheckIsBetPlaced (market : Market, betOrder : BetOrder) =
        let price = betOrder.Price    

        market.Bets |> Seq.exists (fun bet -> bet.Price = price)

    /// <summary>
    /// Execute
    /// </summary>
    /// <param name="market"></param>
    /// <param name="bfexplorerService"></param>
    let ExecuteInOneApiCall (market : Market) (bfexplorerService : BfexplorerService) = async {
        report $"Starting to place 10 bets on the market: {market.MarketFullName}"

        let selection, bets = createMySelectionBets market

        let watch = Stopwatch.StartNew()

        let! result = bfexplorerService.PlaceBets(market, selection, bets, PersistenceType.Lapse)

        if result.IsSuccessResult
        then
            let mutable checkBetStatus = true

            while checkBetStatus do
                let allBetsPlaced = doCheckAllBetsArePlaced market

                if allBetsPlaced
                then
                    watch.Stop()

                    checkBetStatus <- false
                else
                    do! Async.Sleep(10)

            report $"All bets placed in {watch.Elapsed}"
            
            return Result.Success
        else
            return Result.Failure result.FailureMessage
    }

    /// <summary>
    /// Execute
    /// </summary>
    /// <param name="market"></param>
    /// <param name="bfexplorerService"></param>
    let Execute (market : Market) (bfexplorerService : BfexplorerService) = async {
        report $"Starting to place 10 bets on the market: {market.MarketFullName}"

        let selection, bets = createMySelectionBets market

        let watch = Stopwatch.StartNew()

        for bet in bets do
            let! result = bfexplorerService.PlaceBet(market, selection, bet.BetType, bet.Price, bet.Size, PersistenceType.Lapse)

            if result.IsSuccessResult
            then
                let mutable checkBetStatus = true

                while checkBetStatus do
                    let isBetPlaced = doCheckIsBetPlaced (market, bet)

                    if isBetPlaced
                    then
                        checkBetStatus <- false
                    else
                        do! Async.Sleep(10)
        
        watch.Stop()

        report $"All bets placed in {watch.Elapsed}"

        return Result.Success
    }
