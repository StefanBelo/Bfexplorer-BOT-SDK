// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MiroBotTrigger

open System
open System.IO
open System.Collections.Generic

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// MyBotToExecute
/// </summary>
type MyBotToExecute =
    {
        Price : float
        BetType : BetType
        BotName : string
        MyBotParameters : MyBotParameter list
    }

/// <summary>
/// ToBetType
/// </summary>
/// <param name="data"></param>
let inline ToBetType(data : string) =
    Enum.Parse(typedefof<BetType>, data) :?> BetType

/// <summary>
/// ToObjectValue
/// </summary>
/// <param name="value"></param>
let ToObjectValue(value : string) =
    match value with
    | "Back" | "Lay" -> box (ToBetType(value))
    | "True" | "False" -> box (bool.Parse(value))
    | _ -> box (Double.Parse value)

/// <summary>
/// ToMyBotParameter
/// </summary>
/// <param name="name"></param>
/// <param name="value"></param>
let ToMyBotParameter(name : string, value : string) =
    try
        Some
            {
                MyBotParameter.Name = name
                MyBotParameter.Value = ToObjectValue value
            }
    with
    | _ -> None

/// <summary>
/// ToMyBotParameters
/// </summary>
/// <param name="data"></param>
let ToMyBotParameters(data : string[]) =
    let endIndex = data.Length - 2

    if endIndex >= 3
    then
        let myBotParameters = ResizeArray<MyBotParameter>()
        let mutable index = 3
    
        while index <= endIndex do
            ToMyBotParameter(data.[index], data.[index + 1]) |> Option.iter myBotParameters.Add

            index <- index + 2

        myBotParameters |> Seq.toList
    else
        list.Empty

/// <summary>
/// ToMyBotToExecute
/// </summary>
/// <param name="line"></param>
let ToMyBotToExecute(line : string) =
    let data = line.Split(';')

    if data.Length >= 3
    then
        try
            let price = double data.[0]
            let betType = ToBetType data.[1]
            let botName = data.[2]
            let myBotParameters = ToMyBotParameters data

            Some
                {
                    Price = price
                    BetType = betType
                    BotName = botName
                    MyBotParameters = myBotParameters
                }
        with
        | _ -> None
    else
        None

/// <summary>
/// LoadCsvData
/// </summary>
/// <param name="filePathName"></param>
let LoadCsvData(filePathName : string) =
    let myBotsToExecute = ResizeArray<MyBotToExecute>()

    try
        use reader = new StreamReader(filePathName)

        while not reader.EndOfStream do
            let line = reader.ReadLine()

            if not(String.IsNullOrEmpty line)
            then
                ToMyBotToExecute(line) |> Option.iter myBotsToExecute.Add

        Some myBotsToExecute
    with
    | _ -> 
        None

// Cached data
let myBotsToExecuteDictionary = Dictionary<string, ResizeArray<MyBotToExecute>>()

/// <summary>
/// GetMyBotsToExecute
/// </summary>
/// <param name="filePathName"></param>
let GetMyBotsToExecute(filePathName : string) =
    let status, myBotsToExecute = myBotsToExecuteDictionary.TryGetValue filePathName

    if status
    then
        myBotsToExecute
    else
        let someBotsToExecute = LoadCsvData(filePathName)

        if someBotsToExecute.IsSome
        then
            let myBotsToExecute = someBotsToExecute.Value

            myBotsToExecuteDictionary.Add(filePathName, myBotsToExecute)
            myBotsToExecute
        else
            ResizeArray<MyBotToExecute>()
        
/// <summary>
/// MiroBotTrigger
/// </summary>
type MiroBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    let myBotsToExecute = GetMyBotsToExecute(defaultArg (botTriggerParameters.GetParameter<string>("CsvFile")) String.Empty)
        
    interface IBotTrigger with

        member this.Execute() =
            if myBotsToExecute.Count = 0
            then
                EndExecutionWithMessage "No CSV data loaded."
            else
                let someBotToExecute =
                    myBotsToExecute 
                    |> Seq.tryFind (fun myBotToExecute -> myBotToExecute.BetType = botTriggerParameters.BetType && myBotToExecute.Price = botTriggerParameters.Price)

                if someBotToExecute.IsSome
                then
                    let myBotToExecute = someBotToExecute.Value

                    ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute (myBotToExecute.BotName, selection, myBotToExecute.MyBotParameters, false)
                else
                    ExecuteActionBot

        member this.EndExecution() =
            ()