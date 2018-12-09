// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module HorseRacingTradeFirstFavouriteBotTrigger

open System.Collections.Generic

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | PlaceLayBets
    | WaitForBetsPlacing
    | WaitForBetsMatching
    | CloseBetPosition
    | FailedToPlaceBets

/// <summary>
/// HorseRacingTradeFirstFavouriteBotTrigger
/// </summary>
type HorseRacingTradeFirstFavouriteBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.PlaceLayBets
    let mutable closeOnSelections = nil<Queue<Selection>>

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | PlaceLayBets ->
                this.PlaceLayBets()
            | WaitForBetsPlacing -> 
                WaitingForOperation
            | WaitForBetsMatching -> 
                this.WaitForBetsMatching()
            | CloseBetPosition -> 
                this.CloseBetPosition()
            | FailedToPlaceBets -> 
                EndExecutionWithMessage "Failed to place bets!"

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// PlaceLayBets
    /// </summary>
    member private this.PlaceLayBets() =
        let price = defaultArg (botTriggerParameters.GetParameter<float>("LayOdds")) 1.1
        let size = defaultArg (botTriggerParameters.GetParameter<float>("LayStake")) 10.0

        let betOrders = 
            market.Selections
            |> Seq.filter (fun mySelection -> mySelection.Status = SelectionStatus.Active)
            |> Seq.map (fun mySelection -> 
                    {
                        SelectionBetOrder.Selection = mySelection
                        SelectionBetOrder.BetType = BetType.Lay
                        SelectionBetOrder.Price = price
                        SelectionBetOrder.Size = size
                    }
                )
            |> Seq.toList

        status <- TriggerStatus.WaitForBetsPlacing

        PlaceBets (betOrders, PersistenceType.Persist, fun (result : bool) -> status <- if result then TriggerStatus.WaitForBetsMatching else TriggerStatus.FailedToPlaceBets)
        
    /// <summary>
    /// WaitForBetsMatching
    /// </summary>
    member private this.WaitForBetsMatching() =
        if market.HaveMatchedBets
        then
            CancelBets (None, this.BetsCancelled)
        else
            WaitingForOperation

    /// <summary>
    /// BetsCancelled
    /// </summary>
    /// <param name="result"></param>
    member private this.BetsCancelled(result : bool) =
        if result
        then 
            closeOnSelections <- Queue (market.Selections |> Seq.filter (fun mySelection -> mySelection.BetPosition.IsValid))
            status <- TriggerStatus.CloseBetPosition

    /// <summary>
    /// CloseBetPosition
    /// </summary>
    member private this.CloseBetPosition() =
        let mySelection = closeOnSelections.Dequeue()

        ExecuteActionBotOnSelectionWithParametersAndContinueToExecute (mySelection, list.Empty, closeOnSelections.Count > 0)