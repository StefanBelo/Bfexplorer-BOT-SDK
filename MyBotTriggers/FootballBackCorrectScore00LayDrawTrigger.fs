// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module FootballBackCorrectScore00LayDrawTrigger

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

[<Literal>]
let StakeParameterName = "Stake"

[<Literal>]
let BackCorrectScoreBotName = "Back Correct Score 0 - 0"

[<Literal>]
let LayDrawBotName = "Lay Draw"

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | BackCorrectScore
    | WaitForBetToBeMatched
    | LayDraw

/// <summary>
/// FootballBackCorrectScore00LayDrawTrigger
/// </summary>
type FootballBackCorrectScore00LayDrawTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    let mutable status = TriggerStatus.BackCorrectScore

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            let result =
                match status with
                | BackCorrectScore -> 
                    this.BackCorrectScore(selection, botTriggerParameters)
                | WaitForBetToBeMatched ->
                    this.WaitForBetToBeMatched(market)
                | LayDraw ->
                    this.LayDraw(selection)

            result

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// BackCorrectScore
    /// </summary>
    /// <param name="selection"></param>
    /// <param name="botTriggerParameters"></param>
    member private this.BackCorrectScore(selection : Selection, botTriggerParameters : BotTriggerParameters) =
        status <- WaitForBetToBeMatched

        let myBotParameters =
            match botTriggerParameters.GetParameter<float>(StakeParameterName) with
            | Some stake -> [ { MyBotParameter.Name = StakeParameterName; MyBotParameter.Value = stake } ] 
            | None -> List.Empty

        ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute (BackCorrectScoreBotName, selection, myBotParameters, true)

    /// <summary>
    /// WaitForBetToBeMatched
    /// </summary>
    /// <param name="market"></param>
    member private this.WaitForBetToBeMatched(market : Market) =
        let isBotRunning = market.RunningBots |> Seq.exists (fun bot -> bot.Name = BackCorrectScoreBotName)

        if not isBotRunning
        then
            status <- LayDraw

        WaitingForOperation

    /// <summary>
    /// LayDraw
    /// </summary>
    /// <param name="selection"></param>
    member private this.LayDraw(selection : Selection) =
        if selection.Profit.HasValue
        then
            let botParameters = ExecuteOnAssociatedMarketBotParameters()

            botParameters.Name <- "Lay Draw On Associated Market"
            botParameters.BotName <- LayDrawBotName
            botParameters.MarketName <- "MATCH_ODDS"

            let setBotParameter = botParameters :> ISetBotParameter
            
            setBotParameter.SetParameter(StakeParameterName, selection.Profit.Value * 0.85)

            ExecuteBfexplorerBotOnSelectionWithParametersAndContinueToExecute (typeof<ExecuteOnAssociatedMarketBot>, selection, botParameters, false)
        else
            EndExecutionWithMessage "No back bet placed on 0 - 0"
