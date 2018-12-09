// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MiroStrategyBotTrigger

open System
open System.Diagnostics
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
    | ex -> 
        Debug.WriteLine(ex.ToString())
        None

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
            let price = Double.Parse data.[0]
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
        | ex -> 
            Debug.WriteLine(ex.ToString())
            None
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
    | ex -> 
        Debug.WriteLine(ex.ToString())
        None

// Cached data
let myBotsToExecuteDictionary = Dictionary<string, ResizeArray<MyBotToExecute>>()

/// <summary>
/// GetMyBotsToExecuteParameters
/// </summary>
/// <param name="filePathName"></param>
let GetMyBotsToExecuteParameters(filePathName : string) =
    lock myBotsToExecuteDictionary (fun () ->
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
    )

/// <summary>
/// GetCanEndExecution
/// </summary>
/// <param name="bot"></param>
let GetCanEndExecution(bot : Bot) =
    bot.BetStatus = BotBetStatus.NoOperationInProgress && bot.Status = BotStatus.WaitingForEntryCriteria || bot.Status = BotStatus.WaitingForOperation || bot.Status = BotStatus.BetPositionOpening

/// <summary>
/// StopPassiveBots
/// </summary>
/// <param name="market"></param>
/// <param name="selectionId"></param>
/// <param name="botName"></param>
let StopPassiveBots(market : Market, selectionId : int64, botName : string) =
    market.RunningBots
    |> Seq.filter (fun bot -> if isNull bot.RunningOnSelection then false else bot.RunningOnSelection.Id = selectionId && bot.Name <> botName)
    |> Seq.iter (fun bot ->
            if GetCanEndExecution(bot)
            then
                try
                    bot.EndExecution()
                with
                | _ -> ()

                bot.Status <- BotStatus.ExecutionEnded
        )

/// <summary>
/// GetBotOpenPrice
/// </summary>
/// <param name="bot"></param>
let GetBotOpenPrice(bot : Bot) =
    match bot with
    | :? PlaceBetBot as theBot -> Some theBot.Parameters.Odds
    | :? PlaceBetClosePositionBot as theBot -> Some theBot.Parameters.OpenBetPosition.Odds
    | _ -> None

/// <summary>
/// StopPassiveBotsOutOfTradingRange
/// </summary>
/// <param name="market"></param>
/// <param name="selectionId"></param>
/// <param name="botName"></param>
/// <param name="downOdds"></param>
/// <param name="upOdds"></param>
let StopPassiveBotsOutOfTradingRange(market : Market, selectionId : int64, botName : string, downOdds : float, upOdds : float) =    
    market.RunningBots
    |> Seq.filter (fun bot -> if isNull bot.RunningOnSelection then false else bot.RunningOnSelection.Id = selectionId && bot.Name <> botName)
    |> Seq.iter (fun bot ->
            GetBotOpenPrice(bot)
            |> Option.iter (fun price ->
                if GetCanEndExecution(bot) && (price < downOdds || price > upOdds)
                then
                    try
                        bot.EndExecution()
                    with
                    | _ -> ()
                )
        )

/// <summary>
/// GetRunningBotsOpenPrices
/// </summary>
/// <param name="market"></param>
/// <param name="selectionId"></param>
/// <param name="botName"></param>
let GetRunningBotsOpenPrices(market : Market, selectionId : int64, botName : string) =
    let selectionBots =
        market.RunningBots
        |> Seq.filter (fun bot -> if isNull bot.RunningOnSelection then false else bot.RunningOnSelection.Id = selectionId && bot.Name <> botName)

    [
        for bot in selectionBots  do
            let somePrice = GetBotOpenPrice(bot)

            if somePrice.IsSome
            then
                yield somePrice.Value
    ]
    
/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | Initialize
    | WaitingForTimeToCheck
    | ExecuteMyActionBots
        
/// <summary>
/// MiroStrategyBotTrigger
/// </summary>
type MiroStrategyBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =       

    let myBotsToExecuteParameters = GetMyBotsToExecuteParameters(defaultArg (botTriggerParameters.GetParameter<string>("CsvFile")) String.Empty)
    let checkTimeSpan = TimeSpan.FromSeconds(defaultArg (botTriggerParameters.GetParameter<float>("CheckTimeSpan")) 20.0)
    let backRange = defaultArg (botTriggerParameters.GetParameter<int>("BackRange")) 2
    let layRange = defaultArg (botTriggerParameters.GetParameter<int>("LayRange")) 6
    let matchedDiff  = defaultArg (botTriggerParameters.GetParameter<float>("matchedDiff")) 5.0

    let mutable status = Initialize
    let mutable timeToCheck = DateTime.MinValue
    let mutable myBotsToExecute = nil<Queue<MyBotToExecute>>
        
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match status with
            | Initialize ->

                if myBotsToExecuteParameters.Count = 0
                then
                    EndExecutionWithMessage "No CSV data loaded."
                else
                    status <- WaitingForTimeToCheck
                    WaitingForOperation

            | WaitingForTimeToCheck ->

                let now = DateTime.Now

                if now >= timeToCheck
                then
                    timeToCheck <- now.Add(checkTimeSpan)

                    let backDownOdds, backUpOdds, layDownOdds, layUpOdds = this.GetTradingRange()

                    StopPassiveBotsOutOfTradingRange(market, selection.Id, botName, backDownOdds, layUpOdds)

                    let openPrices = GetRunningBotsOpenPrices(market, selection.Id, botName)

                    let bilance = this.GetBilance()

                    let canExecuteBackBot = matchedDiff > bilance
                    let canExecuteLayBot = -matchedDiff < bilance
                                           
                    if this.GetHaveMyBotsToExecute(backDownOdds, backUpOdds, layDownOdds, layUpOdds, openPrices, canExecuteBackBot, canExecuteLayBot)
                    then
                        status <- ExecuteMyActionBots

                WaitingForOperation

            | ExecuteMyActionBots ->

                this.ExecuteMyActionBots()

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()

    /// <summary>
    /// GetTradingRange
    /// </summary>
    member this.GetTradingRange() =
        let bestBackOdds = selection.GetBestPrice(BetType.Back)
        let bestLayOdds = selection.GetBestPrice(BetType.Lay)
        let isGapInOfferer = selection.OddsContext.GetOddsDifference(bestBackOdds, bestLayOdds) > 2
        let oddsContext = selection.OddsContext

        let backUpOdds = if isGapInOfferer then oddsContext.GetOddsUp(bestBackOdds, 1) else bestBackOdds
        let layDownOdds = if isGapInOfferer then oddsContext.GetOddsDown(bestLayOdds, 1) else bestLayOdds

        oddsContext.GetOddsDown(backUpOdds, backRange), backUpOdds, layDownOdds, oddsContext.GetOddsUp(layDownOdds, layRange)

    /// <summary>
    /// GetBilance
    /// </summary>
    member this.GetBilance() =
        let betPosition = selection.BetPosition

        if betPosition.IsValid then betPosition.BackPosition.Size - betPosition.LayPosition.Size else 0.0

    /// <summary>
    /// GetHaveMyBotsToExecute
    /// </summary>
    /// <param name="backDownOdds"></param>
    /// <param name="backUpOdds"></param>
    /// <param name="layDownOdds"></param>
    /// <param name="layUpOdds"></param>
    /// <param name="openPrices"></param>
    /// <param name="canExecuteBackBot"></param>
    /// <param name="canExecuteLayBot"></param>
    member this.GetHaveMyBotsToExecute(backDownOdds, backUpOdds, layDownOdds, layUpOdds, openPrices : float list, canExecuteBackBot, canExecuteLayBot) =
        let newBotsToExecute = Seq.toList (seq {
                yield! 
                    myBotsToExecuteParameters 
                    |> Seq.filter (fun myBotToExecute -> 
                            if (openPrices |> List.contains myBotToExecute.Price)
                            then
                                false
                            else
                                myBotToExecute.Price >= backDownOdds && myBotToExecute.Price <= backUpOdds && (if myBotToExecute.BetType = BetType.Back then canExecuteBackBot else canExecuteLayBot)
                        )

                yield! 
                    myBotsToExecuteParameters 
                    |> Seq.filter (fun myBotToExecute -> 
                            if (openPrices |> List.contains myBotToExecute.Price)
                            then
                                false
                            else
                                myBotToExecute.Price >= layDownOdds && myBotToExecute.Price <= layUpOdds && (if myBotToExecute.BetType = BetType.Back then canExecuteBackBot else canExecuteLayBot)
                        )
            })

        let status = not newBotsToExecute.IsEmpty

        if status
        then
            myBotsToExecute <- Queue newBotsToExecute

        status

    /// <summary>
    /// ExecuteMyActionBots
    /// </summary>
    member this.ExecuteMyActionBots() =
        let myBotToExecute = myBotsToExecute.Dequeue()

        if myBotsToExecute.Count = 0
        then
            myBotsToExecute <- nil
            status <- WaitingForTimeToCheck

        ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute (myBotToExecute.BotName, selection, myBotToExecute.MyBotParameters, true)