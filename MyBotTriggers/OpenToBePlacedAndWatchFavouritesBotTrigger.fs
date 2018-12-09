// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module OpenToBePlacedAndWatchFavouritesBotTrigger

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetActiveSelections
/// </summary>
/// <param name="selections"></param>
let GetActiveSelections(selections : Selection seq) =
    selections |> Seq.filter (fun s -> s.Status = SelectionStatus.Active)

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | OpenWinMarket
    | WaitToOpenWinMarket
    | OpenToBePlacedMarket
    | WaitToOpenToBePlacedMarket
    | FailedToOpenAssociatedMarket
    | WatchToBePlacedFavourites
    | Done

/// <summary>
/// OpenToBePlacedAndWatchFavouritesBotTrigger
/// </summary>
type OpenToBePlacedAndWatchFavouritesBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =    

    let mutable status = TriggerStatus.OpenWinMarket
    let mutable winMarket = nil<Market>
    let mutable toBePlacedMarket = nil<Market>

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            let result =
                match status with
                | OpenWinMarket ->
                    status <- WaitToOpenWinMarket
                    OpenAssociatedMarkets ([| "WIN" |], this.SetWinMarket)
                | OpenToBePlacedMarket ->
                    status <- WaitToOpenToBePlacedMarket
                    OpenAssociatedMarkets ([| "PLACE" |], this.SetToBePlacedMarket)
                | WaitToOpenWinMarket | WaitToOpenToBePlacedMarket ->
                    WaitingForOperation
                | FailedToOpenAssociatedMarket ->
                    EndExecutionWithMessage "Failed to open the associated market."
                | WatchToBePlacedFavourites ->
                    this.WatchToBePlacedFavourites()
                | Done ->
                    EndExecution

            result

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// SetWinMarket
    /// </summary>
    /// <param name="result"></param>
    member this.SetWinMarket(result : DataResult<Market[]>) =
        status <- 
            if result.IsSuccessResult
            then
                winMarket <- result.SuccessResult.[0]
                OpenToBePlacedMarket
            else
                FailedToOpenAssociatedMarket
         
    /// <summary>
    /// SetToBePlacedMarket
    /// </summary>
    /// <param name="result"></param>
    member this.SetToBePlacedMarket(result : DataResult<Market[]>) =
        status <- 
            if result.IsSuccessResult
            then
                toBePlacedMarket <- result.SuccessResult.[0]
                WatchToBePlacedFavourites
            else
                FailedToOpenAssociatedMarket

    /// <summary>
    /// WatchToBePlacedFavourites
    /// </summary>
    member this.WatchToBePlacedFavourites() =
        status <- Done

        let numberOfHorses = GetActiveSelections(winMarket.Selections) |> Seq.length

        let toWinFavourites = 
            GetActiveSelections(winMarket.Selections)
            |> Seq.sortBy (fun s -> s.LastPriceTraded)
            |> Seq.take (min 3 numberOfHorses)
            |> Seq.toList

        let toBePlacedSelections = toBePlacedMarket.Selections
                    
        WatchMarketSelections (toBePlacedMarket, toWinFavourites |> List.map (fun s -> GetSelectionByName(s.Name, toBePlacedSelections).Value))
