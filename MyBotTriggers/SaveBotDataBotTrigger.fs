// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module SaveBotDataBotTrigger

open System
open System
open System.ComponentModel
open System.Collections
open System.Collections.ObjectModel
open System.Collections.Specialized
open System.IO
open System.Text.RegularExpressions

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetFileName
/// </summary>
/// <param name="market"></param>
let GetFileName(market : Market) =
    Regex.Replace(market.MarketFullName, "[^A-Za-z0-9_. ]+", "")
        
/// <summary>
/// SaveBotDataBotTrigger
/// </summary>
type SaveBotDataBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    let filePathName = Path.Combine(defaultArg (botTriggerParameters.GetParameter<string>("CsvFolder")) String.Empty, sprintf "%s.csv" (GetFileName(market)))

    let SaveBotsData(bots : IList) =
        try
            use writer = File.AppendText(filePathName)

            for botObj in bots do
                match botObj with
                | :? SelectionBot as bot -> 

                    let betPosition = bot.BotBetsCache.BetPosition
                    let backPosition = betPosition.BackPosition
                    let layPosition = betPosition.LayPosition

                    writer.WriteLine(sprintf "%s;%s;%.2f;%.2f;%.2f;%.2f;%.2f" bot.Name bot.RunningOnSelection.Name
                            backPosition.Price backPosition.Size 
                            layPosition.Price layPosition.Size
                            betPosition.ProfitIfWin
                        )

                | _ -> ()
        with
            | ex -> myBfexplorer.BfexplorerService.OutputMessage(sprintf "Failed to save data: %s" ex.Message)
        
    let SaveBotDataHandler = NotifyCollectionChangedEventHandler(fun _ args ->
            if args.Action = NotifyCollectionChangedAction.Remove
            then
                SaveBotsData(args.OldItems)
        )

    do
        (market.RunningBots :> INotifyCollectionChanged).CollectionChanged.AddHandler SaveBotDataHandler
        
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
            (market.RunningBots :> INotifyCollectionChanged).CollectionChanged.RemoveHandler SaveBotDataHandler