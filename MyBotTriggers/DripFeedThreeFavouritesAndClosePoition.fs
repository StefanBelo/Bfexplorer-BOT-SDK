// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module DripFeedThreeFavouritesAndClosePoition

open System.Collections.Generic

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetFavourites
/// </summary>
/// <param name="market"></param>
/// <param name="numberOfFavourites"></param>
let GetFavourites(market : Market, numberOfFavourites : int) =
    let favourites =
        market.Selections
        |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Active)
        |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
        |> Seq.toList

    let count = min favourites.Length numberOfFavourites

    favourites |> List.take count

/// <summary>
/// MyBotToExecute
/// </summary>
type MyBotToExecute =
    {
        Selection : Selection
        Size : float
    }
    
/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | CalculateDutchBets
    | ExecuteDutchBetsDripFeeding
    | ExecuteClosePosition
    | WaitForBetsMatching
    | WaitForTargetProfitToClosePosition

/// <summary>
/// DripFeedThreeFavouritesAndClosePoition
/// </summary>
type DripFeedThreeFavouritesAndClosePoition(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.CalculateDutchBets
    let mutable myBotsToExecute = nil<Queue<MyBotToExecute>>

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | CalculateDutchBets -> this.CalculateDutchBets()
            | ExecuteDutchBetsDripFeeding -> this.ExecuteDutchBetsDripFeeding()
            | ExecuteClosePosition -> this.ExecuteClosePosition()
            | WaitForBetsMatching -> this.WaitForBetsMatching()
            | WaitForTargetProfitToClosePosition -> this.WaitForTargetProfitToClosePosition()
                
        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// CalculateDutchBets
    /// </summary>
    member private this.CalculateDutchBets() =
        let dutchSelections = 
            GetFavourites(market, 3) 
            |> List.map (fun s -> SelectionBetPriceSize(s, BetType.Back))

        let bookValue = DutchingCalculations.GetBookValue(dutchSelections)

        let canPlaceBets =
            if DutchingCalculations.GetCanPlaceDutchingBets(bookValue, BetType.Back)
            then
                let stake = defaultArg (botTriggerParameters.GetParameter<float>("Stake")) 100.0

                DutchingCalculations.CalculateForAvailableSize(dutchSelections, stake) > 0.0
            else
                false

        if canPlaceBets
        then
            status <- TriggerStatus.ExecuteDutchBetsDripFeeding

            myBotsToExecute <- Queue (
                    dutchSelections |> List.map (fun dutchSelection -> 
                        { 
                            MyBotToExecute.Selection = dutchSelection.Selection
                            MyBotToExecute.Size = dutchSelection.Size 
                        }
                    )
                )

            WaitingForOperation
        else
            EndExecutionWithMessage "Cannot place dutching bets!"

    /// <summary>
    /// ExecuteDutchBetsDripFeeding
    /// </summary>
    member private this.ExecuteDutchBetsDripFeeding() =
        let myBotToExecute = myBotsToExecute.Dequeue()

        if myBotsToExecute.Count = 0
        then
            myBotsToExecute <- nil
            status <- ExecuteClosePosition

        let myBotParameters = [ { MyBotParameter.Name = "TargetValue"; MyBotParameter.Value = myBotToExecute.Size } ]

        ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute ("Drip feed backing 5 Euro", myBotToExecute.Selection, myBotParameters, true)

    /// <summary>
    /// ExecuteClosePosition
    /// </summary>
    member private this.ExecuteClosePosition() =
        status <- WaitForBetsMatching

        ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute ("Close my dutch bets", selection, list.Empty, true)

    /// <summary>
    /// WaitForBetsMatching
    /// </summary>
    member private this.WaitForBetsMatching() =
        if market.RunningBots.Count = 1
        then
            status <- WaitForTargetProfitToClosePosition

        WaitingForOperation

    /// <summary>
    /// WaitForTargetProfitToClosePosition
    /// </summary>
    member private this.WaitForTargetProfitToClosePosition() =
        if market.RunningBots.Count = 1
        then
            EndExecution
        else
            WaitingForOperation