#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open System
open System.Text

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let ShowOddsDifference(market : Market) =
    let selections =
        market.Selections
        |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Active)
        |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
        |> Seq.toList

    selections
    |> List.pairwise
    |> List.iter (fun element ->
            let current = fst element
            let next = snd element

            let probability = GetPriceProbability current.LastPriceTraded

            printfn "%s: %.2f | %.2f" current.Name probability (probability - (GetPriceProbability next.LastPriceTraded))
        )

    let last = selections |> List.last

    printfn "%s: %.2f" last.Name (GetPriceProbability last.LastPriceTraded)

let bfexplorer : IBfexplorerConsole = nil
let market = bfexplorer.ActiveMarket

ShowOddsDifference(market)