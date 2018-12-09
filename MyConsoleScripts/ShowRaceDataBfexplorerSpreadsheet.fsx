#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.DataAnalysis.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\DevExpress.Data.v15.1.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\DevExpress.Office.v15.1.Core.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\DevExpress.Spreadsheet.v15.1.Core.dll"

open System
open DevExpress.Spreadsheet

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.DataAnalysis.Models

let WritePriceRange(worksheet : Worksheet, row : int, column : int, openPrice : float, priceRange : PriceRange) =
    worksheet.[row, column].SetValue(priceRange.OpenPrice)
    
    let cell = worksheet.[row, column + 1]

    cell.SetValue(priceRange.LowPrice)

    if priceRange.LowPrice < openPrice
    then
        cell.FillColor <- Drawing.Color.LightGreen

    worksheet.[row, column + 2].SetValue(priceRange.HighPrice)
    worksheet.[row, column + 3].SetValue(priceRange.ClosePrice)

let bfexplorer : IBfexplorerConsole = nil

async {
    do! Async.SwitchToContext(bfexplorer.BfexplorerUI.UiSynchronizationContext)

    let bfexplorerSpreadsheet = bfexplorer.BfexplorerUI.BfexplorerSpreadsheet

    if isNull bfexplorerSpreadsheet
    then
        printf "Open bfexplorer spreadsheet application!"
    else    
        let market = bfexplorer.ActiveMarket

        let worksheetName = market.Id
        let workbook = bfexplorerSpreadsheet.Document

        let worksheet =
            if workbook.Worksheets.Contains(worksheetName)
            then
                workbook.Worksheets.[worksheetName]
            else
                workbook.Worksheets.Add(worksheetName)

        try
            bfexplorerSpreadsheet.BeginUpdate()

            worksheet.[0, 0].SetValue(market.MarketFullName)

            let mutable row = 1

            market.Selections
            |> Seq.filter (fun selection -> selection.Status <> SelectionStatus.Removed)
            |> Seq.iter (fun selection -> 
                    
                    selection.GetData<PriceRanges>("PriceRanges")
                    |> Option.iter (fun priceRanges ->
                            worksheet.[row, 0].SetValue(selection.Name)

                            let openPriceRange = priceRanges.Open
                            let openPrice = openPriceRange.OpenPrice

                            WritePriceRange(worksheet, row, 1, openPrice, openPriceRange)
                            WritePriceRange(worksheet, row, 5, openPrice, priceRanges.Close)
                        )
                    row <- row + 1
                )
        finally
            bfexplorerSpreadsheet.EndUpdate()
}
|> Async.RunSynchronously