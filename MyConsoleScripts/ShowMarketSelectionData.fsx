#r @"E:\Projects\Bfexplorer\Development\Libraries\XPlot\bin\XPlot.Plotly.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.DataAnalysis.dll"

open System
open XPlot.Plotly
open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.DataAnalysis.Models

let toTimes (startTime : DateTime) (data : TimePriceVolume seq) =
    data |> Seq.map (fun d -> (d.Time - startTime).TotalSeconds) |> Seq.toArray

let toPrices (data : TimePriceVolume seq) =
    data |> Seq.map (fun d -> d.Price) |> Seq.toArray

let toVolumes (data : TimePriceVolume seq) =
    data |> Seq.map (fun d -> d.Volume) |> Seq.toArray

let selectionPriceVolumeChart(title : string, startTime : DateTime, data : TimePriceVolume seq) =
    let timesData = data |> toTimes startTime
    let pricesData = data |> toPrices
    let volumesData = data |> toVolumes

    let volumes = Bar(x = timesData, y = volumesData, name = "Traded Volume") :> Trace
    let prices = Scatter(x = timesData, y = pricesData, name = "Price", yaxis = "y2") :> Trace
    
    [ volumes; prices ]
    |> Plotly.Plot
    |> Plotly.WithLayout (Layout(title = title, height = 800., width = 1200., 
                            xaxis = Xaxis(title = "Time"), 
                            yaxis = Yaxis(title = "Traded Volume", zeroline = true), 
                            yaxis2 = Yaxis(title = "Price", overlaying = "y", side = "right")))
    |> Plotly.Show

let getSelectionsTimeSeries(selections : Selection seq) =
    let selectionsData =
        selections
        |> Seq.choose (fun selection ->
                match SelectionTimeSeries.Get(selection) with
                | Some timeSeries -> Some (selection, timeSeries)
                | None -> None
            )
        |> Seq.toList
        |> List.sortBy (fun (selection, _) -> selection.LastPriceTraded)

    selectionsData |> List.take (min 4 selectionsData.Length)

let selectionsPriceChart(title : string, startTime : DateTime, selections : Selection seq) =
    let selectionsTimeSeries = getSelectionsTimeSeries(selections)
    let timesData = (snd (List.head selectionsTimeSeries)).PreEvent |> toTimes startTime

    selectionsTimeSeries
    |> List.map (fun (selection, selectionTimeSeries) -> Scatter(x = timesData, y = (selectionTimeSeries.PreEvent |> toPrices), name = selection.Name))
    |> Plotly.Plot
    |> Plotly.WithLayout (Layout(title = title, height = 800., width = 1200., xaxis = Xaxis(title = "Time"), yaxis = Yaxis(title = "Price")))
    |> Plotly.Show

let selectionsInPlayPriceChart(title : string, startTime : DateTime, selections : Selection seq) =
    let selectionsTimeSeries = getSelectionsTimeSeries(selections)
    let timesData = (snd (List.head selectionsTimeSeries)).InPlay |> toTimes startTime

    selectionsTimeSeries
    |> List.map (fun (selection, selectionTimeSeries) -> Scatter(x = timesData, y = (selectionTimeSeries.InPlay |> toPrices), name = selection.Name))
    |> Plotly.Plot
    |> Plotly.WithLayout (Layout(title = title, height = 800., width = 1200., xaxis = Xaxis(title = "Time"), yaxis = Yaxis(title = "Price")))
    |> Plotly.Show
    
let showChart(market : Market, selection : Selection) =
    match SelectionTimeSeries.Get(selection) with
    | Some timeSeries ->

        //selectionPriceVolumeChart(sprintf "%s - %s" market.MarketFullName selection.Name, market.MarketInfo.StartTime, timeSeries.PreEvent)
        //selectionPriceVolumeChart(sprintf "%s - %s" market.MarketFullName selection.Name, market.MarketInfo.StartTime, timeSeries.InPlay)
        selectionsPriceChart(market.MarketFullName, market.MarketInfo.StartTime, market.Selections)
        //selectionsInPlayPriceChart(sprintf "%s - In-Play" market.MarketFullName, market.MarketInfo.StartTime, market.Selections)

    | None -> printfn "No time series available!"
    
let bfexplorer : IBfexplorerConsole = nil
showChart(bfexplorer.ActiveMarket, bfexplorer.ActiveSelection)