(*
    Copyright © 2021 - 2022, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.Bfexplorer

open System.Threading

open BeloSoft.Betfair.StreamingAPI.Models

open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Domain

/// <summary>
/// BfexplorerHost
/// </summary>
type BfexplorerHost () =

    let uiSynchronizationContext = SynchronizationContext ()

    do
        (* Grid Initialization *)
        DataContext.CreatePriceGridDataContext <- fun (priceGridData : PriceGridData) -> PriceGridDataContextStreaming (priceGridData) :> IPriceGridDataContext
        SelectionExtensions.InitializeSelection <- fun (selection : Selection) -> selection.PriceGridDataEnabled <- true

    interface IUiApplication with

        member _this.UiSynchronizationContext
            with get() = uiSynchronizationContext

        member _this.Execute (action : unit -> unit)  = 
            action ()

        member _this.ExecuteAndReturn<'T> (action : unit -> obj) = async {
            let result = action ()

            return unbox<'T> result
        }

        member _this.ExecuteAsync (action : unit -> Async<unit>) = 
            async {
                do! action ()
            }
            |> Async.StartImmediate
            
        member _this.ExecuteAsyncAndReturn<'T> (action : unit -> Async<obj>) = async {
            let! result = action ()

            return unbox<'T> result            
        }