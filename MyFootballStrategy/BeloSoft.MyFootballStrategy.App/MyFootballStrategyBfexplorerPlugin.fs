(*
    Copyright © 2019, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.MyFootballStrategy.App

open System
open System.Windows
open FSharp.Desktop.UI

open BeloSoft.Data
open BeloSoft.Bfexplorer.Service
open BeloSoft.MyFootballStrategy.UI.Controls

/// <summary>
/// MyFootballStrategyBfexplorerPlugin
/// </summary>
type MyFootballStrategyBfexplorerPlugin() =

    let uiControl = StrategyBotExecutorControl()
    let pluginMvc = PluginMvc<StrategyBotExecutorEvents, StrategyBotExecutorModel>(StrategyBotExecutorModel.Create(), StrategyBotExecutorView(uiControl), StrategyBotExecutorController())

    let mutable observable = nil<IDisposable>
       
    interface IBfexplorerPlugin with

        member _this.Name 
            with get() = "My Football Strategy"

        member _this.Description
            with get() = "My football strategy offering live score and match statistics."

        member _this.UiControl
            with get() = uiControl :> FrameworkElement

        member _this.Start() =
            observable <- pluginMvc.Start()