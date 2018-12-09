#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Betfair.API.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Trading.dll"

open System.IO

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
  
let myResults = MyStrategyOperations.GetMyStrategyResults("Lay favourite with my staking plan")

myResults
|> Option.iter (fun myStrategyResults -> 
        printfn "%A, Winning bets: %d, Losing bets: %d" myStrategyResults myStrategyResults.TotalWinningBets myStrategyResults.TotalLosingBets

        try
            let filePathName = "D:\Temp\GreyhoundsLayFavourite2.csv"
            use writer = File.CreateText(filePathName)

            writer.WriteLine("Bet Number;My Selection;1st Favourite;2nd Favourite;3rd Favourite;Book Value;Profit;Is Winning Bet")

            let mutable betNumber = 1

            myStrategyResults.SettledMarkets 
            |> Seq.iter (fun market ->
                    if market.SettledProfit.HasValue
                    then
                        let mySelection = getMySelection(market)
                        let threeFavourites = getThreeFavourites(market)
                        let threeFavouritesBookValue = threeFavourites |> Array.sumBy (fun selection -> 100.0 / selection.LastPriceTraded)
                        let profit = market.SettledProfit.Value

                        writer.WriteLine(sprintf "%d;%.2f;%s;%.2f;%.2f;%A"
                            betNumber
                            mySelection.LastPriceTraded (threeFavourites |> Array.map (fun selection -> (sprintf "%.2f" selection.LastPriceTraded)) |> String.concat ";")
                            threeFavouritesBookValue
                            profit (profit > 0.0)
                        )

                        betNumber <- betNumber + 1
                )

            printfn "CSV data has been written to the file: %s" filePathName
        with
        | e -> printfn "Failed to write data to the cvs file: %s" (e.ToString())
    )