// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module SaveMarketGraphs

open System
open System.Diagnostics
open System.IO
open System.Net
open System.Text.RegularExpressions

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// ToCorrectName
/// </summary>
/// <param name="text"></param>
let ToCorrectName(text : string) =
    Regex.Replace(text, "[^A-Za-z0-9_. ]+", "")

/// <summary>
/// CreateDirectory
/// </summary>
/// <param name="directory"></param>
let CreateDirectory(directory : string) =
    if not(Directory.Exists(directory))
    then
        Directory.CreateDirectory(directory) |> ignore

/// <summary>
/// CreateMarketDirectory
/// </summary>
/// <param name="rootDirectory"></param>
/// <param name="market"></param>
let CreateMarketDirectory(rootDirectory : string, market : Market) =
    let marketInfo = market.MarketInfo

    let todayDirectory = Path.Combine(rootDirectory, DateTime.Now.ToString("yyyyMMdd"))
    CreateDirectory(todayDirectory)

    let eventTypeDirectory = Path.Combine(todayDirectory, marketInfo.EventTypeName)
    CreateDirectory(eventTypeDirectory)

    let eventDirectory = Path.Combine(eventTypeDirectory, ToCorrectName(marketInfo.EventName))
    CreateDirectory(eventDirectory)

    let marketName =
        match marketInfo.BetEventType.Id with
        | 7 (* Horse Racing *)  | 4339 (* Greyhound Racing *) -> sprintf "%s %s" (marketInfo.StartTime.ToString("HHmm")) marketInfo.MarketName
        | _ -> marketInfo.MarketName
        
    let marketDirectory = Path.Combine(eventDirectory, ToCorrectName(marketName))
    CreateDirectory(marketDirectory)

    marketDirectory
                
/// <summary>
/// SaveMarketGraphs
/// </summary>
type SaveMarketGraphs(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let webClient = WebClient()

    let selectionGraphUrl = 
        let marketIds = market.Id.Split('.')
        sprintf @"http://%s.site.sports.betfair.com/betting/LoadRunnerInfoChartAction.do?marketId=%s&selectionId=" (if marketIds.[0] = "1" then "uk" else "au") marketIds.[1]

    let saveSelectionGraph(selection : Selection, directory : string) =
        let graphUrl = sprintf "%s%d" selectionGraphUrl selection.Id
        let filePathName = Path.Combine(directory, sprintf "%s.jpeg" (ToCorrectName(selection.Name)))

        webClient.DownloadFile(graphUrl, filePathName)

    let outputMessage message =
        myBfexplorer.BfexplorerService.OutputMessage(message)

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            async {
                let rootDirectory = defaultArg (botTriggerParameters.GetParameter<string>("CsvFolder")) String.Empty

                //return
                try
                    let directory = CreateMarketDirectory(rootDirectory, market)

                    market.Selections
                    |> Seq.iter (fun mySelection -> if mySelection.Status = SelectionStatus.Active then saveSelectionGraph(mySelection, directory))

                    //sprintf "Graphs has been saved to: %s" directory
                with
                | ex -> Debug.WriteLine(sprintf "Failed to save data: %s" ex.Message)
            }
            //|> Async.StartWithContinuations(task, outputMessage, (fun exn -> outputMessage exn.Message), (fun exn -> outputMessage exn.Message))
            |> Async.Start

            TriggerResult.EndExecution

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()