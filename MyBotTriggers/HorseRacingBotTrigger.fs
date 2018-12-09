// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module HorseRacingBotTrigger

#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Data.dll"
#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Bfexplorer.Domain.dll"
#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Bfexplorer.Trading.dll"

open System
open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetRaceDistance
/// </summary>
/// <param name="market"></param>
let GetRaceDistance(market : Market) =
    let raceDistanceString = market.MarketInfo.MarketName.Split(' ').[1]
    let distanceParts = raceDistanceString.Length / 2

    let mutable distance = 0.0
    let mutable index = 0

    while index <= distanceParts do
        distance <- distance + (
                float ((int)raceDistanceString.[index] - (int)'0') * 
                match raceDistanceString.Chars(index + 1) with
                | 'm' -> 1609.344
                | _ -> 201.1708
            )

        index <- index + 2

    distance

/// <summary>
/// GetFavourite
/// </summary>
/// <param name="market"></param>
let GetFavourite(market : Market) =
    market.Selections
    |> Seq.filter (fun selection -> selection.Status = SelectionStatus.Active)
    |> Seq.sortBy (fun selection -> selection.LastPriceTraded)
    |> Seq.head

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | DeactivateMonitoring
    | Reopen
    | StartMyActionBot

/// <summary>
/// HorseRacingBotTrigger
/// </summary>
type HorseRacingBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    let mutable status = TriggerStatus.Initialize

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | Initialize ->
                if market.MarketInfo.BetEventType.Id = 7
                then
                    let allowedDistance = defaultArg (botTriggerParameters.GetParameter<float>("AllowedDistance")) 1609.344

                    if GetRaceDistance(market) >= allowedDistance
                    then
                        status <- TriggerStatus.DeactivateMonitoring
                        WaitingForOperation
                    else
                        EndExecution
                else
                    EndExecutionWithMessage "You can run this bot on a horse racing market only!"
                
            | DeactivateMonitoring ->
                DeactiveMarketMonitoringStatus(market)
                status <- TriggerStatus.Reopen
                WaitingForOperation

            | Reopen ->
                status <- TriggerStatus.StartMyActionBot
                WaitingForOperation

            | StartMyActionBot ->
                ExecuteActionBotOnSelection (GetFavourite(market))

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()