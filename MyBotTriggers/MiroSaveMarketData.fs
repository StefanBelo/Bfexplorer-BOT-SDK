// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MiroSaveMarketData

open System
open System.Text.RegularExpressions

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
open System.IO

/// <summary>
/// ToCorrectName
/// </summary>
/// <param name="text"></param>
let ToCorrectName(text : string) =
    Regex.Replace(text, "[^A-Za-z0-9_. ]+", "")

/// <summary>
/// MyData
/// </summary>
type MyData =
    {
        Time : DateTime
        BackPrice : float
        LayPrice : float
        ProfitBalance : float
    }

    member this.ToCsv() =
        sprintf "%A;%.2f;%.2f;%.2f" this.Time this.BackPrice this.LayPrice this.ProfitBalance

    static member Create(time, backPrice, layPrice, profitBalance) =
        {
            Time = time
            BackPrice = backPrice
            LayPrice = layPrice
            ProfitBalance = profitBalance
        }

    static member Set(selection : Selection) = 
        selection.SetData("myData", ResizeArray<MyData>())

    static member Get(selection : Selection) = 
        selection.Data.["myData"] :?> ResizeArray<MyData>

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | RecordData
        
/// <summary>
/// MiroSaveMarketData
/// </summary>
type MiroSaveMarketData(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let updateInterval = TimeSpan.FromSeconds(defaultArg (botTriggerParameters.GetParameter<float>("UpdateInterval")) 5.0)

    let mutable triggerStatus = TriggerStatus.Initialize
    let mutable timeToUpdate = DateTime.MinValue

    let saveSelectionData(selection : Selection, directory : string) =
        let filePathName = Path.Combine(directory, sprintf "%s.csv" (ToCorrectName(selection.Name)))
        use writer = File.CreateText(filePathName)
        
        MyData.Get(selection) |> Seq.iter (fun myData -> writer.WriteLine(myData.ToCsv()))

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match triggerStatus with
            | TriggerStatus.Initialize ->

                market.Selections
                |> Seq.iter (fun mySelection -> if mySelection.Status = SelectionStatus.Active then MyData.Set(mySelection))

                triggerStatus <- TriggerStatus.RecordData

            | TriggerStatus.RecordData ->
                
                let now = DateTime.Now

                if now >= timeToUpdate
                then
                    market.Selections
                    |> Seq.iter (fun mySelection -> 
                            if mySelection.Status = SelectionStatus.Active
                            then
                                let myDatas = MyData.Get(mySelection)

                                let backPrice = mySelection.GetBestPrice(BetType.Back)
                                let layPrice = mySelection.GetBestPrice(BetType.Lay)
                                let profitBalance = if mySelection.ProfitBalance.HasValue then mySelection.ProfitBalance.Value else 0.0
                                                                
                                myDatas.Add(MyData.Create(now, backPrice, layPrice, profitBalance))
                        )

                    timeToUpdate <- now.Add(updateInterval)
                
            WaitingForOperation

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            let csvDirectory = Path.Combine(defaultArg (botTriggerParameters.GetParameter<string>("CsvFolder")) String.Empty, ToCorrectName(market.MarketFullName))

            try
                if not(Directory.Exists(csvDirectory))
                then
                    Directory.CreateDirectory(csvDirectory) |> ignore

                market.Selections
                |> Seq.iter (fun mySelection -> 
                        if mySelection.Status = SelectionStatus.Active
                        then
                            saveSelectionData(mySelection, csvDirectory)
                    )
            with
            | ex -> myBfexplorer.BfexplorerService.OutputMessage(sprintf "Failed to save data: %s" ex.Message)


