// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module ExecuteOnAllSelectionMyStrategyBotTrigger

open System
open System.Collections.Generic

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// MyStrategyData
/// </summary>
[<NoEquality; NoComparison>]
type MyStrategyData =
    {
        Selection : Selection
        BetType : BetType
    }

let GetMyStrategyData(selection : Selection, odds : float) =
    let lastPriceTraded = selection.LastPriceTraded

    {
        MyStrategyData.Selection = selection
        MyStrategyData.BetType = if lastPriceTraded >= odds then BetType.Back else BetType.Lay
    }

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | ExecuteMyActionBots
    
/// <summary>
/// ExecuteOnAllSelectionMyStrategyBotTrigger
/// </summary>
type ExecuteOnAllSelectionMyStrategyBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.Initialize
    let mutable myBotsToExecuteOnSelections = nil<Queue<MyStrategyData>>

    let odds = defaultArg (botTriggerParameters.GetParameter<float>("Odds")) 3.1
    let betTypeParameterName =
        if (defaultArg (botTriggerParameters.GetParameter<bool>("UseTradingBotParameters")) false)
        then
            "OpenBetPosition.BetType"
        else
            "BetType"

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | Initialize -> this.Initialize()
            | ExecuteMyActionBots -> this.ExecuteMyActionBots()

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// Initialize
    /// </summary>
    member private this.Initialize() =
        status <- TriggerStatus.ExecuteMyActionBots

        myBotsToExecuteOnSelections <- Queue(
                market.Selections
                |> Seq.filter (fun mySelection -> mySelection.Status = SelectionStatus.Active)
                |> Seq.map (fun mySelection -> GetMyStrategyData(mySelection, odds))
            )
        
        WaitingForOperation

    /// <summary>
    /// ExecuteMyActionBots
    /// </summary>
    member private this.ExecuteMyActionBots() =
        let myStrategyData = myBotsToExecuteOnSelections.Dequeue()
        let myBotParameters = [ { MyBotParameter.Name = betTypeParameterName; MyBotParameter.Value = myStrategyData.BetType } ]

        myStrategyData.Selection.PriceGridDataEnabled <- true

        ExecuteActionBotOnSelectionWithParametersAndContinueToExecute (myStrategyData.Selection, myBotParameters, myBotsToExecuteOnSelections.Count > 0)