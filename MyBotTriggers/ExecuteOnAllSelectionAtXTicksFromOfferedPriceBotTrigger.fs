// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module ExecuteOnAllSelectionAtXTicksFromOfferedPriceBotTrigger

open System
open System.Collections.Generic

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | ExecuteMyActionBots
    
/// <summary>
/// ExecuteOnAllSelectionAtXTicksFromOfferedPriceBotTrigger
/// </summary>
type ExecuteOnAllSelectionAtXTicksFromOfferedPriceBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = Initialize
    let mutable priceDifference = 0y
    let mutable myBotsToExecuteOnSelections = nil<Queue<Selection>>

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
        if botTriggerParameters.Price = 0.0
        then
            EndExecutionWithMessage "Please use the parameter: UseLadderParameters"
        else
            priceDifference <- sbyte (selection.OddsContext.GetOddsDifference(selection.GetPriceSize(botTriggerParameters.BetType).Price, botTriggerParameters.Price))
            myBotsToExecuteOnSelections <- Queue (market.Selections |> Seq.filter (fun s -> s.Status = SelectionStatus.Active))
            status <- ExecuteMyActionBots

            //myBfexplorer.BfexplorerService.OutputMessage(sprintf "priceDifference: %d" priceDifference)
                    
            WaitingForOperation

    /// <summary>
    /// ExecuteMyActionBots
    /// </summary>
    member private this.ExecuteMyActionBots() =
        let mySelection = myBotsToExecuteOnSelections.Dequeue()

        let oddsParameterName =
            if (defaultArg (botTriggerParameters.GetParameter<bool>("UseTradingBotParameters")) false)
            then
                "OpenBetPosition.Odds"
            else
                "Odds"

        let odds = 
            match mySelection.GetPrice(botTriggerParameters.BetType, false, priceDifference) with
            | Some price -> price
            | None -> if botTriggerParameters.BetType = BetType.Back then OddsData.MaximalOdds else OddsData.MinimalOdds
        
        let myBotParameters = [ { MyBotParameter.Name = oddsParameterName; MyBotParameter.Value = odds } ]

        ExecuteActionBotOnSelectionWithParametersAndContinueToExecute (mySelection, myBotParameters, myBotsToExecuteOnSelections.Count > 0)