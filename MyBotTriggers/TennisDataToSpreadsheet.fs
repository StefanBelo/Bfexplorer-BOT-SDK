// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module TennisDataToSpreadsheet

open System
open System.Collections.Generic

open DevExpress.Spreadsheet
open DevExpress.XtraSpreadsheet

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

let private marketRowIndexes = Dictionary<int, int>()
let mutable private lastRowIndex = 1

let private resetRowIndex() =
    lock marketRowIndexes (fun () ->
        marketRowIndexes.Clear()
        lastRowIndex <- 1
    )

/// <summary>
/// getMarketRowIndex
/// </summary>
/// <param name="market"></param>
let private getMarketRowIndex(market : Market) =
    lock marketRowIndexes (fun () ->
        let marketId = market.MarketInfo.BetEvent.Id

        if marketRowIndexes.ContainsKey(marketId)
        then
            marketRowIndexes.[marketId]
        else
            let index = lastRowIndex
        
            marketRowIndexes.Add(marketId, index)
            lastRowIndex <- lastRowIndex + 2

            index
    )

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | UpdateData

/// <summary>
/// TennisDataToSpreadsheet
/// </summary>
type TennisDataToSpreadsheet(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let updateInterval = TimeSpan.FromSeconds(defaultArg (botTriggerParameters.GetParameter<float>("UpdateInterval")) 2.0)

    let mutable tennisMatch = nil<TennisMatch>

    let mutable bfexplorerSpreadsheet = nil<ISpreadsheetControl>
    let mutable worksheet = nil<Worksheet>
    let mutable rowIndex = 1

    let mutable triggerStatus = TriggerStatus.Initialize
    let mutable timeToUpdate = DateTime.MinValue

    let setTimeToUpdate() =
        timeToUpdate <- DateTime.Now.Add(updateInterval)

    let setSelectionPrice(selection : Selection, betType : BetType, index : int, column : int) =
        let priceValue =
            match selection.GetPrice(betType) with
            | Some price -> sprintf "%.2f" price
            | None -> String.Empty

        worksheet.[rowIndex + index, column].SetValue(priceValue)

    let updateMarketPrices() =
        market.Selections
        |> Seq.iteri (fun index mySelection ->
            setSelectionPrice(mySelection, BetType.Back, index, 5)
            setSelectionPrice(mySelection, BetType.Lay, index, 6)
        )

    let update() =
        try
            bfexplorerSpreadsheet.BeginUpdate()

            if tennisMatch.IsUpdated
            then
                let homePlayer = tennisMatch.HomePlayer
                let awayPlayer = tennisMatch.AwayPlayer

                worksheet.[rowIndex, 1].SetValue(homePlayer.PointsWon)
                worksheet.[rowIndex + 1, 1].SetValue(awayPlayer.PointsWon)

                worksheet.[rowIndex, 2].SetValue(homePlayer.Set1Score)
                worksheet.[rowIndex + 1, 2].SetValue(awayPlayer.Set1Score)

                worksheet.[rowIndex, 3].SetValue(homePlayer.Set2Score)
                worksheet.[rowIndex + 1, 3].SetValue(awayPlayer.Set2Score)

                worksheet.[rowIndex, 4].SetValue(homePlayer.Set3Score)
                worksheet.[rowIndex + 1, 4].SetValue(awayPlayer.Set3Score)

            updateMarketPrices()
        finally
            bfexplorerSpreadsheet.EndUpdate()

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match triggerStatus with
            | TriggerStatus.Initialize ->

                bfexplorerSpreadsheet <- myBfexplorer.BfexplorerService.Bfexplorer.BfexplorerSpreadsheet

                if isNull bfexplorerSpreadsheet
                then
                    TriggerResult.EndExecutionWithMessage "Open bfexplorer spreadsheet application!"
                else
                    if market.MarketInfo.BetEventType.Id = 2 && market.MarketDescription.MarketType = "MATCH_ODDS"
                    then
                        tennisMatch <- GetTennisMatch(market)

                        worksheet <- bfexplorerSpreadsheet.Document.Worksheets.[0]

                        if defaultArg (botTriggerParameters.GetParameter<bool>("Reset")) false
                        then
                            resetRowIndex()

                        rowIndex <- getMarketRowIndex(market)

                        market.Selections |> Seq.iteri (fun index mySelection -> worksheet.[rowIndex + index, 0].SetValue(mySelection.Name))

                        triggerStatus <- TriggerStatus.UpdateData

                        TriggerResult.WaitingForOperation
                    else
                        TriggerResult.EndExecutionWithMessage "You can execute this bot only on a tennis match odds market."
                                
            | TriggerStatus.UpdateData ->

                if DateTime.Now >= timeToUpdate
                then
                    setTimeToUpdate()
                    update()

                    TriggerResult.UpdateTennisMatchScore (tennisMatch, ignore)
                else
                    TriggerResult.WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()