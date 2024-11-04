#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\DevExpress.Data.v15.1.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\DevExpress.Office.v15.1.Core.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\DevExpress.Spreadsheet.v15.1.Core.dll"

open System
open DevExpress.Spreadsheet

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let GetFavourites(market : Market) =
    market.Selections 
    |> Seq.filter (fun selection -> selection.Status <> SelectionStatus.Removed)
    |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
    |> Seq.toArray

let GetFavouriteIndex(selections : Selection[], mySelection : Selection) =
    (selections |> Array.findIndex (fun s -> s.Id = mySelection.Id)) + 1

let GetWinners(market : Market) =
    market.Selections 
    |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Winner)
    |> Seq.toArray

let bfexplorer : IBfexplorerConsole = nil

async {
    do! Async.SwitchToContext(bfexplorer.BfexplorerUI.UiSynchronizationContext)

    let bfexplorerSpreadsheet = bfexplorer.BfexplorerUI.BfexplorerSpreadsheet

    if isNull bfexplorerSpreadsheet
    then
        printf "Open bfexplorer spreadsheet application!"
    else    
        let worksheetName = "Winners"
        let workbook = bfexplorerSpreadsheet.Document

        let worksheet =
            if workbook.Worksheets.Contains(worksheetName)
            then
                workbook.Worksheets.[worksheetName]
            else
                workbook.Worksheets.Add(worksheetName)

        try
            bfexplorerSpreadsheet.BeginUpdate()

            let mutable row = 0

            bfexplorer.OpenMarkets
            |> List.filter (fun market -> market.MarketStatus = MarketStatus.Closed)
            |> List.iter (fun market -> 
                    worksheet.[row, 0].SetValue(market.MarketInfo.StartTime)
                    worksheet.[row, 1].SetValue(market.MarketFullName)

                    let favourites = GetFavourites(market)
                    
                    let mutable column = 2

                    GetWinners(market) |> Array.iter (fun selection -> 
                            worksheet.[row, column].SetValue(selection.Name)
                            worksheet.[row, column + 1].SetValue(selection.LastPriceTraded)
                            worksheet.[row, column + 2].SetValue(GetFavouriteIndex(favourites, selection))

                            column <- column + 3
                        )
                    
                    row <- row + 1
                )
        finally
            bfexplorerSpreadsheet.EndUpdate()
}
|> Async.RunSynchronously