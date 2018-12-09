// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

namespace MyFsharpBot

open System
open DevExpress.Spreadsheet

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// SpreadsheetDemoBotParameters
/// </summary>
[<Sealed>]
type SpreadsheetDemoBotParameters() =
    inherit BotParameters()
    
    member val UpdateInterval : TimeSpan = TimeSpan.FromSeconds(5.0) with get, set

/// <summary>
/// SelectionData
/// </summary>
type SelectionData() =
    let lastPriceTradedHistory = HistoryValue<float>()
    
    member this.LastPriceTradedHistory
        with get() = lastPriceTradedHistory

    member this.SetTrend(lastPriceTraded : float) =
        if lastPriceTradedHistory.SetValue(lastPriceTraded)
        then
            if lastPriceTradedHistory.Value - lastPriceTradedHistory.PreviousValue > 0.0
            then
                1
            else
                -1
        else
            0

/// <summary>
/// SpreadsheetDemoMarketBot
/// </summary>
[<Sealed>]
type SpreadsheetDemoMarketBot(market : Market, parameters : SpreadsheetDemoBotParameters, bfexplorerService : IBfexplorerService) =
    inherit MarketBot(market, parameters, bfexplorerService)

    let bfexplorerSpreadsheet = bfexplorerService.Bfexplorer.BfexplorerSpreadsheet

    let mutable worksheet = nil<Worksheet>
    let mutable timeToUpdated = DateTime.MinValue

    let setupMarketData() =
        let worksheetName = market.Id
        let workbook = bfexplorerSpreadsheet.Document

        worksheet <- 
            if workbook.Worksheets.Contains(worksheetName)
            then
                workbook.Worksheets.[worksheetName]
            else
                workbook.Worksheets.Add(worksheetName)

        try
            bfexplorerSpreadsheet.BeginUpdate()

            worksheet.[0, 0].SetValue(market.MarketFullName)

            market.Selections 
            |> Seq.iteri 
                (fun index selection ->
                    let row = index + 2
                    let selectionData = SelectionData()

                    selectionData.SetTrend(selection.LastPriceTraded) |> ignore
                    selection.Data.["selectionData"] <- selectionData

                    worksheet.[row, 0].SetValue(selection.Name)
                    worksheet.[row, 1].SetValue(selection.LastPriceTraded)
                    worksheet.[row, 2].SetValue(selection.TotalMatched)
                    worksheet.[row, 3].SetValue(0)
                )

            let minPoint = worksheet.ConditionalFormattings.CreateIconSetValue(ConditionalFormattingValueType.Number, "-1", ConditionalFormattingValueOperator.GreaterOrEqual)
            let midPoint = worksheet.ConditionalFormattings.CreateIconSetValue(ConditionalFormattingValueType.Number, "0", ConditionalFormattingValueOperator.GreaterOrEqual)
            let maxPoint = worksheet.ConditionalFormattings.CreateIconSetValue(ConditionalFormattingValueType.Number, "1", ConditionalFormattingValueOperator.GreaterOrEqual)

            let cfRule = worksheet.ConditionalFormattings.AddIconSetConditionalFormatting(worksheet.Range.[sprintf "$D$3:$D$%d" (market.Selections.Count + 2)], IconSetType.Arrows3, [| minPoint; midPoint; maxPoint |])
            
            cfRule.IsCustom <- true

            let mutable cfCustomIcon = ConditionalFormattingCustomIcon()
            
            cfCustomIcon.IconSet <- IconSetType.TrafficLights13
            cfCustomIcon.IconIndex <- 1

            cfRule.SetCustomIcon(1, cfCustomIcon)

            cfRule.ShowValue <- false

        finally
            bfexplorerSpreadsheet.EndUpdate()

        timeToUpdated <- DateTime.Now.Add(parameters.UpdateInterval)

    let updateMarketData() =
        try
            bfexplorerSpreadsheet.BeginUpdate()

            market.Selections
            |> Seq.iteri (fun index selection ->
                    let row = index + 2
                    let selectionData = selection.Data.["selectionData"] :?> SelectionData

                    worksheet.[row, 1].SetValue(selection.LastPriceTraded)
                    worksheet.[row, 2].SetValue(selection.TotalMatched)
                    worksheet.[row, 3].SetValue(selectionData.SetTrend(selection.LastPriceTraded))
                )
        finally
            bfexplorerSpreadsheet.EndUpdate()

        timeToUpdated <- DateTime.Now.Add(parameters.UpdateInterval)

    /// <summary>
    /// Execute
    /// </summary>
    override this.Execute() =
        match this.Status with
        | BotStatus.InitializationInProgress -> 

            if this.EvaluateCriteria()
            then
                if isNullObj bfexplorerSpreadsheet
                then
                    this.OutputMessage("Open bfexplorer spreadsheet application!")
                    this.Status <- BotStatus.ExecutionEnded
                else
                    setupMarketData()
                    this.Status <- BotStatus.WaitingForOperation

        | BotStatus.WaitingForOperation ->

            if DateTime.Now >= timeToUpdated
            then
                updateMarketData()
                
        | _ -> ()