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

let bfexplorer : IBfexplorerConsole = nil

async {
    do! Async.SwitchToContext(bfexplorer.BfexplorerUI.UiSynchronizationContext)

    let bfexplorerSpreadsheet = bfexplorer.BfexplorerUI.BfexplorerSpreadsheet

    if isNull bfexplorerSpreadsheet
    then
        printf "Open bfexplorer spreadsheet application!"
    else    
        let worksheetName = "Markets"
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
            |> List.iter (fun market -> 
                    worksheet.[row, 0].SetValue(market.MarketInfo.StartTime)
                    worksheet.[row, 1].SetValue(market.MarketFullName)
                    worksheet.[row, 2].SetValue(market.MarketStatusText)

                    row <- row + 1
                )
        finally
            bfexplorerSpreadsheet.EndUpdate()
}
|> Async.RunSynchronously