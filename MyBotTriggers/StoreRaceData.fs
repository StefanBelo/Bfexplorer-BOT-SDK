// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module StoreRaceData

#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Data.dll"
#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Bfexplorer.Domain.dll"
#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Bfexplorer.Trading.dll"
#r @"D:\Projects\Bfexplorer\Development\BetfairFramework\BeloSoft.DataAnalysis\bin\Debug\BeloSoft.DataAnalysis.dll"

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
open BeloSoft.DataAnalysis.Models

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | DeactivateMonitoring
    | Reopen
    | StoreData

/// <summary>
/// StoreRaceData
/// </summary>
type StoreRaceData(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let mutable status = TriggerStatus.DeactivateMonitoring
    let mutable isInPlay = market.IsInPlay

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | DeactivateMonitoring ->

                DeactiveMarketMonitoringStatus(market)
                status <- TriggerStatus.Reopen

            | Reopen ->

                market.Selections
                |> Seq.filter (fun mySelection -> mySelection.Status = SelectionStatus.Active)
                |> Seq.iter (fun mySelection -> 
                        mySelection.SetData("PriceRanges", PriceRanges(mySelection.LastPriceTraded))
                    )

                status <- TriggerStatus.StoreData

            | StoreData ->

                let setInPlay = this.SetInPlay()

                market.Selections
                |> Seq.filter (fun mySelection -> mySelection.Status = SelectionStatus.Active)
                |> Seq.iter (fun mySelection -> 
                        mySelection.GetData<PriceRanges>("PriceRanges")
                        |> Option.iter (fun priceRanges ->
                                if setInPlay
                                then
                                    priceRanges.SetClosePriceRange(mySelection.LastPriceTraded)
                                else
                                    priceRanges.SetPrice(mySelection.LastPriceTraded)
                            )
                    )

            WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    member private this.SetInPlay() =
        if isInPlay <> market.IsInPlay
        then
            isInPlay <- market.IsInPlay            
            true
        else
            false