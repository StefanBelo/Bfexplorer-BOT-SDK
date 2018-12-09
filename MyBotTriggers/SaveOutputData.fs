// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module SaveOutputData

open System
open System
open System.ComponentModel
open System.Collections
open System.Collections.ObjectModel
open System.Collections.Specialized
open System.IO

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
        
/// <summary>
/// SaveOutputData
/// </summary>
type SaveOutputData(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    let filePathName = Path.Combine(defaultArg (botTriggerParameters.GetParameter<string>("CsvFolder")) String.Empty, "Output.csv")

    let SaveData(messages : IList) =
        try
            use writer = File.AppendText(filePathName)

            messages 
            |> Seq.cast<OutputMessage>
            |> Seq.iter (fun message -> writer.WriteLine(sprintf "%A;%s" message.Time message.Message))
        with
            | ex -> myBfexplorer.BfexplorerService.OutputMessage(sprintf "Failed to save data: %s" ex.Message)
        
    let SaveDataHandler = NotifyCollectionChangedEventHandler(fun _ args ->
            if args.Action = NotifyCollectionChangedAction.Add
            then
                SaveData(args.NewItems)
        )

    do
        (myBfexplorer.BfexplorerService.ServiceStatus.OutputMessages :> INotifyCollectionChanged).CollectionChanged.AddHandler SaveDataHandler
        
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            (myBfexplorer.BfexplorerService.ServiceStatus.OutputMessages :> INotifyCollectionChanged).CollectionChanged.RemoveHandler SaveDataHandler