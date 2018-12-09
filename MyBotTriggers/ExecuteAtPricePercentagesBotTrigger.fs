// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module ExecuteAtPricePercentagesBotTrigger

open System
open System.Collections.Generic

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

let toPricePercentages(data : string) =
    try
        if String.IsNullOrEmpty data
        then
            Seq.empty
        else
            data.Split('|') |> Seq.map float
    with 
    | _ -> Seq.empty

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | ExecuteMyActionBot
        
/// <summary>
/// ExecuteAtPricePercentagesBotTrigger
/// </summary>
type ExecuteAtPricePercentagesBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =       

    let pricePercentages = Queue<float>(toPricePercentages (defaultArg (botTriggerParameters.GetParameter<string>("PricePercentages")) String.Empty))
    let isLayBet = defaultArg (botTriggerParameters.GetParameter<bool>("IsLayBet")) true
    let oddsParameterName = if (defaultArg (botTriggerParameters.GetParameter<bool>("UseTradingBotParameters")) false) then "OpenBetPosition.Odds" else "Odds"

    let mutable triggerStatus = TriggerStatus.Initialize
    let mutable initialProbability = 0.0

    let getMyPrice(pricePercentage : float) =        
        let probability = if isLayBet then initialProbability + pricePercentage else initialProbability - pricePercentage

        if probability <= 100.0 && probability >= 0.1
        then
            Some (selection.OddsContext.GetValidOdds(toProbabilityPrice probability))
        else
            None
                
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match triggerStatus with
            | TriggerStatus.Initialize ->
                
                if pricePercentages.Count = 0
                then
                    TriggerResult.EndExecutionWithMessage "Please enter PricePercentages parameter values, for instance: 10|25|45"
                else
                    initialProbability <- toPriceProbability (selection.GetBestPrice(if isLayBet then BetType.Lay else BetType.Back))
                    triggerStatus <- TriggerStatus.ExecuteMyActionBot

                    TriggerResult.WaitingForOperation

            | TriggerStatus.ExecuteMyActionBot ->

                let pricePercentage = pricePercentages.Dequeue()
                let continueToExecute = pricePercentages.Count > 0

                match getMyPrice(pricePercentage) with
                | Some price ->

                    let myBotParameters = [ { MyBotParameter.Name = oddsParameterName; MyBotParameter.Value = price } ]

                    TriggerResult.ExecuteActionBotOnSelectionWithParametersAndContinueToExecute (selection, myBotParameters, continueToExecute)

                | None -> 
                
                    if continueToExecute 
                    then 
                        TriggerResult.WaitingForOperation
                    else
                        TriggerResult.EndExecution

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()