(*
    Copyright © 2021, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.Bfexplorer

open System.Threading

open BeloSoft.Betfair.StreamingAPI.Models

open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Domain

/// <summary>
/// BfexplorerHost
/// </summary>
type BfexplorerHost() =

    let uiSynchronizationContext = SynchronizationContext()

    do
        (* Grid Initialization *)
        DataContext.CreatePriceGridDataContext <- fun (priceGridData : PriceGridData) -> PriceGridDataContextStreaming(priceGridData) :> IPriceGridDataContext
        SelectionExtensions.InitializeSelection <- fun (selection : Selection) -> selection.PriceGridDataEnabled <- true

    interface IUiApplication with

        /// <summary>
        /// UiSynchronizationContext
        /// </summary>
        member _this.UiSynchronizationContext
            with get() = uiSynchronizationContext

        /// <summary>
        /// ExecuteOnUiContext
        /// </summary>
        /// <param name="context"></param>
        /// <param name="action"></param>
        member _this.ExecuteOnUiContext (context : SynchronizationContext) (action : unit -> unit) = async {
            do! Async.SwitchToContext uiSynchronizationContext

            action()

            do! Async.SwitchToContext context
        }

        /// <summary>
        /// ExecuteOnUiContextAndReturn
        /// </summary>
        /// <param name="context"></param>
        /// <param name="action"></param>
        member _this.ExecuteOnUiContextAndReturn (context : SynchronizationContext) (action : unit -> 'T) = async {
            do! Async.SwitchToContext uiSynchronizationContext

            let result = action()

            do! Async.SwitchToContext context

            return result
        }

        /// <summary>
        /// ExecuteAsyncJobOnUiContext
        /// </summary>
        /// <param name="context"></param>
        /// <param name="action"></param>
        member _this.ExecuteAsyncJobOnUiContext (context : SynchronizationContext) (action : unit -> Async<unit>) = async {
            do! Async.SwitchToContext uiSynchronizationContext    

            do! action()

            do! Async.SwitchToContext context
        }

        /// <summary>
        /// ExecuteAsyncJobOnUiContextAndReturn
        /// </summary>
        /// <param name="context"></param>
        /// <param name="action"></param>
        member _this.ExecuteAsyncJobOnUiContextAndReturn (context : SynchronizationContext) (action : unit -> Async<'T>) = async {
            do! Async.SwitchToContext uiSynchronizationContext    

            let! result = action()

            do! Async.SwitchToContext context

            return result
        }