
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Betfair.API.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Trading.dll"

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

let getThreeFavourites (market : Market) =
    market.Selections
    |> Seq.filter (fun selection -> selection.Status <> SelectionStatus.Removed)
    |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
    |> Seq.take 3
    |> Seq.toArray

let getMySelection (market : Market) =
    market.Selections
    |> Seq.find (fun selection -> selection.Bets.HaveBets)
  
let myResults = MyStrategyOperations.GetMyStrategyResults("Lay favourite on greyhounds")

myResults
|> Option.iter (fun myStrategyResults -> 
        printfn "%A, Winning bets: %d, Losing bets: %d" myStrategyResults myStrategyResults.TotalWinningBets myStrategyResults.TotalLosingBets

        myStrategyResults.SettledMarkets |> Seq.iter (fun market -> 
      
                if market.SettledProfit.HasValue
                then
                    let mySelection = getMySelection(market)
                    let threeFavourites = getThreeFavourites(market)
                    let threeFavouritesBookValue = threeFavourites |> Array.sumBy (fun selection -> 100.0 / selection.LastPriceTraded)

                    printfn "%A, Profit: %.2f, My selection: %s, %.2f, Book value: %.2f, Prices: %s" 
                        market market.SettledProfit.Value
                        mySelection.Name mySelection.LastPriceTraded
                        threeFavouritesBookValue
                        (threeFavourites |> Array.map (fun selection -> (sprintf "%.2f" selection.LastPriceTraded)) |> String.concat ", ")
            )

        let totalProfit =
            myStrategyResults.SettledMarkets
            |> Seq.filter (fun market -> market.SettledProfit.HasValue)
            |> Seq.sumBy (fun market -> market.SettledProfit.Value)

        printfn "Total profit: %.2f" totalProfit
    )

    