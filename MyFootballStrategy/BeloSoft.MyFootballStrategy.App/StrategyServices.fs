(*
    Copyright © 2019, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.MyFootballStrategy.App.Services

open System
open System.Collections.ObjectModel
open System.Diagnostics
open System.Threading
open System.Windows.Threading

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.FootballScoreProvider

open BeloSoft.SofaScoreProvider.Models

open BeloSoft.Bfexplorer.PerformgroupProvider
open BeloSoft.Bfexplorer.PerformgroupProvider.Models

open BeloSoft.MyFootballStrategy.Bots.Models

[<AutoOpen>]
module private FootballDataOperations =

    let toHomeAwayValues (value : string) =
        let dataValues =
            if String.IsNullOrEmpty value
            then
                Array.empty
            else
                value.Split(':')

        if dataValues.Length = 2
        then
            uint16 dataValues.[0], uint16 dataValues.[1]
        else
            0us, 0us

    let toMatchStatisticsItemValues (value : string) =
        let home, away = toHomeAwayValues value

        {
            MatchStatisticsValues.Home = home
            MatchStatisticsValues.Away = away
        }

    let toMatchStatisticsItemValuesDifference (valueTotal : string, value : string) =
        let homeTotal, awayTotal = toHomeAwayValues valueTotal
        let home, away = toHomeAwayValues value

        {
            MatchStatisticsValues.Home = homeTotal - home
            MatchStatisticsValues.Away = awayTotal - away
        }

    let toMatchStatisticsItem name values =
        { 
            MatchStatisticsItem.Name = name
            MatchStatisticsItem.Values = values
        }

    let toGroupMatchStatisticsItemsData (statisticsData : StatisticsData) =
        [            
            {
                GroupMatchStatisticsItems.Name = "Shots"
                GroupMatchStatisticsItems.Items = 
                    [
                        toMatchStatisticsItem "Shots off target" (toMatchStatisticsItemValuesDifference (statisticsData.ShotsTotal, statisticsData.ShotsOnTarget))
                        toMatchStatisticsItem "Shots on target" (toMatchStatisticsItemValues statisticsData.ShotsOnTarget)
                    ]
            }

            {
                GroupMatchStatisticsItems.Name = "TVData"
                GroupMatchStatisticsItems.Items = 
                    [
                        toMatchStatisticsItem "Corner kicks" (toMatchStatisticsItemValues statisticsData.Corners)
                        toMatchStatisticsItem "Red cards" (toMatchStatisticsItemValues statisticsData.RedCards)
                    ]
            }
        ]
    
/// <summary>
/// FootballDataUpdateService
/// </summary>
type FootballDataUpdateService(footballMarkets : ObservableCollection<FootballMarket>, bfexplorerService : BfexplorerService) =

    let footballScoreProvider = FootballScoreProvider(bfexplorerService)
    let sofaScoreDataProvider = SofaScoreDataProvider.Instance

    let timer = DispatcherTimer(Interval = TimeSpan.FromSeconds(5.0))

    let mutable matchDataUpdateCounter = 12uy

    let doUpdatePrices (context : SynchronizationContext) = async {
        let footballMarketsToUpdate =
            footballMarkets 
            |> Seq.filter (fun footballMarket -> footballMarket.CanUpdatePrices)
            |> Seq.toList

        let markets = footballMarketsToUpdate |> List.map (fun footballMarket -> footballMarket.Market)
        
        try  
            if markets.Length > 15
            then
                let marketsToUpdate = List.Split 15 markets

                for myMarkets in marketsToUpdate do
                    do! Async.SwitchToContext context
                    do! bfexplorerService.UpdateMarkets(myMarkets) |> Async.Ignore
            else
                do! bfexplorerService.UpdateMarkets(markets) |> Async.Ignore

            footballMarketsToUpdate |> List.iter (fun footballMarket -> footballMarket.OnPricesUpdated())

            do! Async.SwitchToContext context
        with
        | ex -> Debug.WriteLine (toFailureMessage ex)
    }

    let doGetMatchStatisticsWithPerformgroupProvider (market : Market) = async {
        let! result = PerformgroupProvider.GetFootballMatchStatistics(market)

        return
            if result.IsSuccessResult
            then
                DataResult.Success (toGroupMatchStatisticsItemsData result.SuccessResult)
            else
                DataResult.Failure result.FailureMessage
    }

    let executeOnUi context =
        bfexplorerService.UiApplication.ExecuteOnUiContext context

    let switchToUiContext() =
        Async.SwitchToContext bfexplorerService.UiApplication.UiSynchronizationContext

    let tryGetMatchStatistics (context : SynchronizationContext) (footballMarket : FootballMarket) = async {
        let market = footballMarket.Market

        match! sofaScoreDataProvider.GetMatchStatistics(market) with
        | DataResult.Success groupMatchStatisticsItemsData ->

            do! executeOnUi context (fun () -> footballMarket.MatchMatchStatisticsDataType <- MatchMatchStatisticsDataType.SofaScore)

            return DataResult.Success groupMatchStatisticsItemsData

        | _ ->

            match! doGetMatchStatisticsWithPerformgroupProvider market with
            | DataResult.Success groupMatchStatisticsItemsData -> 

                do! executeOnUi context (fun () -> footballMarket.MatchMatchStatisticsDataType <- MatchMatchStatisticsDataType.Performgroup)

                return DataResult.Success groupMatchStatisticsItemsData

            | DataResult.Failure errorMessage -> 

                do! executeOnUi context (fun () -> footballMarket.MatchMatchStatisticsDataType <- MatchMatchStatisticsDataType.NoMatchStatistics)
            
                return DataResult.Failure errorMessage
    }

    let doGetMatchStatistics (context : SynchronizationContext) (footballMarket : FootballMarket) = async {
        let market = footballMarket.Market

        match footballMarket.MatchMatchStatisticsDataType with 
        | MatchMatchStatisticsDataType.SofaScore -> return! sofaScoreDataProvider.GetMatchStatistics(market)
        | MatchMatchStatisticsDataType.Performgroup -> return! doGetMatchStatisticsWithPerformgroupProvider market
        | MatchMatchStatisticsDataType.Unknown -> return! footballMarket |> tryGetMatchStatistics context
        | MatchMatchStatisticsDataType.NoMatchStatistics -> return DataResult.Failure "No match statistics!"                                
    }

    let doUpdateMatchDatas (context : SynchronizationContext) = async {
        let footballMarketsToUpdate =
            footballMarkets 
            |> Seq.filter (fun footballMarket -> footballMarket.CanUpdateMatchData)
            |> Seq.toList

        if not (List.isEmpty footballMarketsToUpdate)
        then
            do! footballScoreProvider.UpdateMatches(footballMarketsToUpdate |> List.map (fun footballMarket -> footballMarket.FootballMatch)) |> Async.Ignore

            do! Async.SwitchToContext context

            let myFootballMarketsToUpdate =
                footballMarketsToUpdate |> List.filter (fun footballMarket -> footballMarket.HaveMatchStatistics)

            let mutable footballMarketsToUpdate = list<FootballMarket * GroupMatchStatisticsItems list>.Empty

            for footballMarket in myFootballMarketsToUpdate do
                match! doGetMatchStatistics context footballMarket with
                | DataResult.Success allMatchStatisticsItems -> footballMarketsToUpdate <- (footballMarket, allMatchStatisticsItems) :: footballMarketsToUpdate                                
                | DataResult.Failure errorMessage -> Debug.WriteLine(errorMessage)

            if not footballMarketsToUpdate.IsEmpty
            then
                do! switchToUiContext()

                footballMarketsToUpdate |> List.iter (fun (footballMarket, allMatchStatisticsItems) -> footballMarket.SetMatchStatistics(allMatchStatisticsItems))
    }

    let doUpdateData() = 
        async {
            let context = SynchronizationContext.Current

            do! doUpdatePrices context

            if matchDataUpdateCounter = 12uy
            then
                do! doUpdateMatchDatas context

                matchDataUpdateCounter <- 1uy
            else
                matchDataUpdateCounter <- matchDataUpdateCounter + 1uy
        }
        |> Async.Start
    
    do
        timer.Tick.Add(fun _ -> doUpdateData())

    /// <summary>
    /// Start
    /// </summary>
    member _this.Start() = async {
        let! result = sofaScoreDataProvider.Initialize()

        if not result.IsSuccessResult
        then
            Debug.WriteLine(result.FailureMessage)

        timer.Start()
    }

    /// <summary>
    /// Stop
    /// </summary>
    member _this.Stop() =
        timer.Stop()

    /// <summary>
    /// Reset
    /// </summary>
    member _this.Reset() =
        sofaScoreDataProvider.Reset()