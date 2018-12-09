// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module DutchingThreeFavourites

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
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | PlaceDutchBets
    | WaitForBetsPlacing
    | WaitForBetsMatching

/// <summary>
/// DutchingThreeFavourites
/// </summary>
type DutchingThreeFavourites(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    let mutable status = TriggerStatus.PlaceDutchBets

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | PlaceDutchBets -> this.PlaceDutchBets()
            | WaitForBetsPlacing -> WaitingForOperation
            | WaitForBetsMatching -> this.WaitForBetsMatching()
                
        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// PlaceDutchBets
    /// </summary>
    member private this.PlaceDutchBets() =
        let dutchSelections = GetFavourites(market, 3) |> List.map (fun s -> SelectionBetPriceSize(s, BetType.Back))
        let bookValue = DutchingCalculations.GetBookValue(dutchSelections)

        let canPlaceBets =
            if DutchingCalculations.GetCanPlaceDutchingBets(bookValue, BetType.Back)
            then
                let stake = defaultArg (botTriggerParameters.GetParameter<float>("Stake")) 100.0

                if DutchingCalculations.CalculateForAvailableSize(dutchSelections, stake) > 0.0
                then                
                    true
                else
                    false
            else
                false

        if canPlaceBets
        then
            status <- TriggerStatus.WaitForBetsPlacing

            let betOrders = dutchSelections |> List.map (fun dutchSelection -> 
                    {
                        SelectionBetOrder.Selection = dutchSelection.Selection
                        SelectionBetOrder.BetType = dutchSelection.BetType
                        SelectionBetOrder.Price = dutchSelection.Price
                        SelectionBetOrder.Size = dutchSelection.Size
                    }
                )

            PlaceBets (betOrders, PersistenceType.Persist, this.WaitForBetsPlacing)
        else
            EndExecutionWithMessage "Cannot place dutching bets!"

    member private this.WaitForBetsPlacing(result : bool) =
        status <- TriggerStatus.WaitForBetsMatching

    /// <summary>
    /// WaitForBetsMatching
    /// </summary>
    member private this.WaitForBetsMatching() =
        if market.HaveUnmatchedBets
        then
            WaitingForOperation
        else
            EndExecution