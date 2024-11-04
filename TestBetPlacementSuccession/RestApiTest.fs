(*
    Copyright © 2022, Stefan Belopotocan, http://bfexplorer.net
*)

module RestApiTest

open System.Diagnostics

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service

/// <summary>
/// doCheckAllBetsArePlaced
/// </summary>
/// <param name="market"></param>
/// <param name="bfexplorerService"></param>
let doCheckAllBetsArePlaced (market : Market) (bfexplorerService : BfexplorerService) = async {
    do! bfexplorerService.UpdateMarket market |> Async.Ignore

    return market.Bets.Count = MumberOfBetsToPlace
}

/// <summary>
/// doCheckIsBetPlaced
/// </summary>
/// <param name="market"></param>
/// <param name="betOrder"></param>
/// <param name="bfexplorerService"></param>
let doCheckIsBetPlaced (market : Market, betOrder : BetOrder) (bfexplorerService : BfexplorerService) = async {
    do! bfexplorerService.UpdateMarket market |> Async.Ignore
     
    let price = betOrder.Price    

    return market.Bets |> Seq.exists (fun bet -> bet.Price = price)
}

/// <summary>
/// Execute
/// </summary>
/// <param name="market"></param>
/// <param name="bfexplorerService"></param>
let ExecuteInOneApiCall (market : Market) (bfexplorerService : BfexplorerService) = async {
    report $"Starting to place 10 bets on the market: {market.MarketFullName}"

    let selection, bets = createMySelectionBets market

    let watch = Stopwatch.StartNew ()

    let! result = bfexplorerService.PlaceBets (market, selection, bets, PersistenceType.Lapse)

    if result.IsSuccessResult
    then
        let mutable checkBetStatus = true

        while checkBetStatus do
            let! allBetsPlaced = bfexplorerService |> doCheckAllBetsArePlaced market

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

    let watch = Stopwatch.StartNew ()

    for bet in bets do
        let! result = bfexplorerService.PlaceBet (market, selection, bet.BetType, bet.Price, bet.Size, PersistenceType.Lapse)

        if result.IsSuccessResult
        then
            let mutable checkBetStatus = true

            while checkBetStatus do
                let! isBetPlaced = bfexplorerService |> doCheckIsBetPlaced (market, bet)

                if isBetPlaced
                then
                    checkBetStatus <- false
                else
                    do! Async.Sleep 10
            
    watch.Stop ()

    report $"All bets placed in {watch.Elapsed}"

    return Result.Success
}

