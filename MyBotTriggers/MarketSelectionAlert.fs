// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MarketSelectionAlert

open System
open System.Text

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// AverageValue
/// </summary>
type AverageValue(initialTotalValue : float) =

    let mutable count = 0
    let mutable averageValue = 0.0
        
    member this.Average
        with get() = averageValue

    member this.SetAverageValue(totalValue : float) =
        count <- count + 1
        averageValue <- (totalValue - initialTotalValue) / float count

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | CheckTradedVolume

/// <summary>
/// MarketSelectionAlert
/// </summary>
type MarketSelectionAlert(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let tradedDifference = defaultArg (botTriggerParameters.GetParameter<float>("TradedDifference")) 30.0
    let textOutput = StringBuilder()

    let mutable status = TriggerStatus.Initialize
    let mutable timeToCheck = DateTime.MinValue

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | Initialize -> 
                
                this.Initialize()                

            | CheckTradedVolume -> 
                
                if this.CheckTradedVolume()
                then
                    let message = textOutput.ToString() //sprintf "%s\n%s" market.MarketFullName (textOutput.ToString())
                    textOutput.Clear() |> ignore

                    AlertMessage message
                else
                    WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// Initialize
    /// </summary>
    member private this.Initialize() =
        market.Selections
        |> Seq.iter (fun mySelection -> 
                if mySelection.Status = SelectionStatus.Active
                then
                    mySelection.SetData("averageTraded", AverageValue(mySelection.TotalMatched))
            )
              
        status <- TriggerStatus.CheckTradedVolume
        timeToCheck <- DateTime.Now.AddMinutes(1.0)
        WaitingForOperation

    /// <summary>
    /// CheckTradedVolume
    /// </summary>
    member private this.CheckTradedVolume() =
        let now = DateTime.Now

        if now >= timeToCheck
        then
            market.Selections
            |> Seq.iter (fun mySelection ->
                    if mySelection.Status = SelectionStatus.Active
                    then
                        mySelection.GetData<AverageValue>("averageTraded")
                        |> Option.iter (fun averageTraded ->
                                let previousAverage = averageTraded.Average

                                averageTraded.SetAverageValue(mySelection.TotalMatched)

                                if previousAverage > 0.0
                                then
                                    let difference = ((averageTraded.Average - previousAverage) / previousAverage) * 100.0

                                    if difference >= tradedDifference
                                    then
                                        textOutput.AppendLine(sprintf "%s %.0f%%" mySelection.Name difference) |> ignore
                            )
                )
            
            timeToCheck <- now.AddMinutes(1.0)
            textOutput.Length > 0
        else
            false