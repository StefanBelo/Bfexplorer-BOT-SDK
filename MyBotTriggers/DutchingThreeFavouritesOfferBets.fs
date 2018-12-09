// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module DutchingThreeFavouritesOfferBets

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
/// MyDutchBetOrder
/// </summary>
[<NoEquality; NoComparison>]
type MyDutchBetOrder =
    {
        Selection : Selection
        Size : float
    }
    
/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | CalculateDutchBets
    | PlaceMyDutchBets
    | WaitForBetsMatching

/// <summary>
/// DutchingThreeFavouritesOfferBets
/// </summary>
type DutchingThreeFavouritesOfferBets(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.CalculateDutchBets
    let mutable myDutchBetOrders = nil<Queue<MyDutchBetOrder>>

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | CalculateDutchBets -> this.CalculateDutchBets()
            | PlaceMyDutchBets -> this.PlaceMyDutchBets()
            | WaitForBetsMatching -> this.WaitForBetsMatching()
                
        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// CalculateDutchBets
    /// </summary>
    member private this.CalculateDutchBets() =
        let dutchSelections = GetFavourites(market, 3) |> List.map (fun s -> SelectionBetPriceSize(s, BetType.Lay ))
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
            status <- TriggerStatus.PlaceMyDutchBets

            myDutchBetOrders <- Queue (dutchSelections |> List.map (fun dutchSelection -> 
                    {
                        MyDutchBetOrder.Selection = dutchSelection.Selection
                        MyDutchBetOrder.Size = dutchSelection.Size
                    }
                ))

            WaitingForOperation
        else
            EndExecutionWithMessage "Cannot place dutching bets!"

    /// <summary>
    /// PlaceMyDutchBets
    /// </summary>
    member private this.PlaceMyDutchBets() =
        let myDutchBetOrder = myDutchBetOrders.Dequeue()

        if myDutchBetOrders.Count = 0
        then
            status <- TriggerStatus.WaitForBetsMatching

        let myBotParameters = [ { MyBotParameter.Name = "Stake"; MyBotParameter.Value = myDutchBetOrder.Size } ]

        ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute ("Place my dutch bet", myDutchBetOrder.Selection, myBotParameters, true)

    /// <summary>
    /// WaitForBetsMatching
    /// </summary>
    member private this.WaitForBetsMatching() =
        if market.HaveUnmatchedBets
        then
            WaitingForOperation
        elif market.RunningBots.Count = 1
        then
            EndExecutionWithMessage "All bets matched."
        else
            WaitingForOperation