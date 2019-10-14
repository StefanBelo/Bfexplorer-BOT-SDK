(*
    Copyright © 2019, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.MyFootballStrategy.App

open System
open System.Collections.ObjectModel
open System.Windows.Data
open System.Windows.Threading

open FSharp.Desktop.UI
open BeloSoft.MyFootballStrategy.UI.Controls

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service

open BeloSoft.Bfexplorer.App.Core

open BeloSoft.MyFootballStrategy.App.Services
open BeloSoft.MyFootballStrategy.Bots.Models
             
/// <summary>
/// StrategyBotExecutorEvents
/// </summary>
type StrategyBotExecutorEvents =
    | Reload
    | Start
    | Stop
    | OpenMarket
    | Delete

/// <summary>
/// StrategyBotExecutorModel
/// </summary>
[<AbstractClass>]
type StrategyBotExecutorModel() as this =
    inherit BaseModel()

    let footballMarkets = ObservableCollection<FootballMarket>()
    let selectedFootballMarkets = ObservableCollection<FootballMarket>()

    let footballDataUpdateService = FootballDataUpdateService(footballMarkets, ApplicationService.BfexplorerService)

    do
        this.IsRunning <- false
        this.IsNavigationPanelVisible <- false

    member _this.FootballMarkets
        with get() = footballMarkets
   
    abstract IsRunning : bool with get, set
    abstract IsNavigationPanelVisible : bool with get, set

    member _this.SelectedFootballMarkets
        with get() = selectedFootballMarkets

    [<DerivedProperty>]
    member this.HaveFootballMarkets
        with get() = this.FootballMarkets.Count > 0

    [<DerivedProperty>]
    member this.HaveSelectedFootballMarkets
        with get() = this.SelectedFootballMarkets.Count > 0

    [<DerivedProperty>]
    member this.CanStart
        with get() = this.FootballMarkets.Count > 0 && not this.IsRunning

    [<DerivedProperty>]
    member this.CanStop
        with get() = this.IsRunning

    member this.Start() = async {
        this.IsRunning <- true
        do! footballDataUpdateService.Start()
    }

    member this.Stop() =
        footballDataUpdateService.Stop()
        this.IsRunning <- false

    member _this.Reset() =
        footballDataUpdateService.Reset()

    member _this.DeleteSelected() =
        footballMarkets.RemoveRange(selectedFootballMarkets |> Seq.toList)

/// <summary>
/// StrategyBotExecutorView
/// </summary>
type StrategyBotExecutorView(control) =
    inherit PartialView<StrategyBotExecutorEvents, StrategyBotExecutorModel, StrategyBotExecutorControl>(control)

    /// <summary>
    /// EventStreams
    /// </summary>
    override this.EventStreams = 
        [
            let barButtonItemClicks = 
                [   
                    this.Root.bReload, Reload
                    this.Root.bStart, Start
                    this.Root.bStop, Stop
                    this.Root.bDelete, Delete
                ] 
                |> List.map (fun (button, event) -> button.ItemClick |> Observable.mapTo event)

            yield! barButtonItemClicks

            yield this.Root.gcMarkets.MouseDoubleClick |> Observable.mapTo OpenMarket
        ]

    /// <summary>
    /// SetBindings
    /// </summary>
    /// <param name="model"></param>
    override this.SetBindings model =
        Binding.OfExpression
            <@
                //this.Root.bNavigationPanel.IsChecked <- coerce model.IsNavigationPanelVisible

                this.Root.bReload.IsEnabled <- model.IsAuthorized
                this.Root.bStart.IsEnabled <- model.CanStart
                this.Root.bStop.IsEnabled <- model.CanStop
                this.Root.bDelete.IsEnabled <- model.HaveSelectedFootballMarkets

                this.Root.gcMarkets.ItemsSource <- model.FootballMarkets
                this.Root.gcMarkets.SelectedItems <- model.SelectedFootballMarkets
            @>

/// <summary>
/// StrategyBotExecutorController
/// </summary>
type StrategyBotExecutorController() =

    let restartTimer = DispatcherTimer(Interval = TimeSpan.FromHours(24.0))

    let reloadMarkets (model : StrategyBotExecutorModel) = async {
        let bfexplorerService = ApplicationService.BfexplorerService

        let filter = [ 
            BetEventFilterParameter.BetEventTypeIds [| 1 |] 
            BetEventFilterParameter.MarketTypeCodes [| "MATCH_ODDS" |] 
            BetEventFilterParameter.TurnInPlayEnabled true
            BetEventFilterParameter.StartTime (DateRange.Today())
        ]
        
        let! result = bfexplorerService.GetMarketCatalogues(filter)

        if result.IsSuccessResult
        then
            let marketIds = 
                result.SuccessResult 
                |> Seq.map (fun marketCatalog -> marketCatalog.MarketInfo.Id)
                |> Seq.toArray

            let! resultGetMarkets = bfexplorerService.GetMarkets(marketIds, true)

            if resultGetMarkets.IsSuccessResult
            then
                let footballMarkets = model.FootballMarkets
                
                footballMarkets.Clear()
                footballMarkets.AddRange(resultGetMarkets.SuccessResult |> Seq.sortBy (fun market -> market.MarketInfo.StartTime) |> Seq.map FootballMarket)
    }

    let watchFavouriteSelection (market : Market) (bfexplorer : IBfexplorer) =
        if canMonitorMarket market
        then
            bfexplorer.WatchMarketSelections(market, [ getFavourites market |> List.head ])

    do
        restartTimer.Tick.Add <| fun _ -> Events.Post ApplicationEvents.RestartExecution

    interface IApplicationEventHandler<StrategyBotExecutorModel> with

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="message"></param>
        /// <param name="model"></param>
        member this.Handle(message : ApplicationEvents, model : StrategyBotExecutorModel) =
            match message with
            | ApplicationEvents.Initialize -> this.Reload(model) |> Async.StartImmediate
            | ApplicationEvents.RestartExecution -> this.RestartExecution(model) |> Async.StartImmediate
            | _ -> ()
    
    interface IController<StrategyBotExecutorEvents, StrategyBotExecutorModel> with

        /// <summary>
        /// InitModel
        /// </summary>
        /// <param name="model"></param>
        member this.InitModel model = 
            Events.Subscribe(this, model)

        /// <summary>
        /// Dispatcher
        /// </summary>
        member this.Dispatcher = function
            | Reload -> Async this.Reload
            | Start -> Async this.Start
            | Stop -> Sync this.Stop
            | OpenMarket -> Async this.OpenMarket
            | Delete -> Sync this.Delete

    /// <summary>
    /// Reload
    /// </summary>
    /// <param name="model"></param>
    member _this.Reload(model : StrategyBotExecutorModel) = async {
        let isRunning = model.IsRunning

        if isRunning
        then
            model.Stop()

        model.Reset()

        do! reloadMarkets model

        if isRunning
        then
            do! model.Start()
    }

    /// <summary>
    /// Start
    /// </summary>
    /// <param name="model"></param>
    member _this.Start(model : StrategyBotExecutorModel) =
        model.Start()

    /// <summary>
    /// Stop
    /// </summary>
    /// <param name="model"></param>
    member _this.Stop(model : StrategyBotExecutorModel) =
        model.Stop()

    /// <summary>
    /// OpenMarket
    /// </summary>
    /// <param name="model"></param>
    member _this.OpenMarket(model : StrategyBotExecutorModel) = async {
        if model.HaveSelectedFootballMarkets
        then
            let selectedFootballMarket = model.SelectedFootballMarkets |> Seq.head

            let marketInfo = selectedFootballMarket.MarketInfo
            let bfexplorer = (ApplicationService.BfexplorerService :> IBfexplorerService).Bfexplorer
            
            let! result = async {
                match bfexplorer.GetOpenMarket(marketInfo) with
                | Some market -> return DataResult.Success market
                | None -> return! bfexplorer.OpenMarket(marketInfo, true)
            }
                       
            if result.IsSuccessResult
            then
                let market = result.SuccessResult

                Events.Post (ApplicationEvents.ReopenMarket market)
                bfexplorer |> watchFavouriteSelection market
    }

    /// <summary>
    /// Delete
    /// </summary>
    /// <param name="model"></param>
    member _this.Delete(model : StrategyBotExecutorModel) =
        model.DeleteSelected()

    /// <summary>
    /// RestartExecution
    /// </summary>
    /// <param name="model"></param>
    member _this.RestartExecution(model : StrategyBotExecutorModel) = async {
        do! reloadMarkets model
    }