// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module TimeoutActionBotTrigger

open System
open System.Collections.Generic
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetMyBotName
/// </summary>
/// <param name="botTriggerParameters"></param>
let GetMyBotName(botTriggerParameters : BotTriggerParameters) =
    let myBotNames = defaultArg (botTriggerParameters.GetParameter<string>("BotsToExecute")) String.Empty

    if String.IsNullOrEmpty myBotNames then [||] else myBotNames.Split('|')
    
/// <summary>
/// TimeoutActionBotTrigger
/// </summary>
type TimeoutActionBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    
    let botsToExecute = Queue<string>(GetMyBotName(botTriggerParameters))
    let timeout = TimeSpan.FromSeconds(defaultArg (botTriggerParameters.GetParameter<float>("Timeout")) 30.0)

    let mutable timeToExecute = DateTime.MinValue

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            if DateTime.Now >= timeToExecute
            then
                let myActionBot = if botsToExecute.Count > 0 then botsToExecute.Dequeue() else String.Empty
                let continueToExecute = botsToExecute.Count > 0

                if continueToExecute
                then
                    timeToExecute <- DateTime.Now.Add(timeout)

                if String.IsNullOrEmpty myActionBot
                then
                    EndExecutionWithMessage "Please set the BotsToExecute parameter."
                else
                    ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute (myActionBot, selection, list.Empty, continueToExecute)
            else
                WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()