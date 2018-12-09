// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MiroGapStrategyBotTrigger

open System
open System.Diagnostics
open System.IO
open System.Collections.Generic

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// MyBotToExecuteData
/// </summary>
type MyBotToExecuteData =
    {
        Ticks : int
        BetType : BetType
        BotName : string
        MyBotParameters : MyBotParameter list
    }

/// <summary>
/// MyBotToExecute
/// </summary>
type MyBotToExecute =
    {
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
/// ToStakeType
/// </summary>
/// <param name="data"></param>
let inline ToStakeType(data : string) =
    Enum.Parse(typedefof<StakeType>, data) :?> StakeType

/// <summary>
/// ToObjectValue
/// </summary>
/// <param name="value"></param>
let ToObjectValue(value : string) =
    match value with
    | "Back" | "Lay" -> box (ToBetType(value))
    | "True" | "False" -> box (bool.Parse(value))
    | "Stake" | "Liability" | "TickProfit" | "NetTickProfit" -> box (ToStakeType(value))
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
            let ticks = int data.[0]
            let betType = ToBetType data.[1]
            let botName = data.[2]
            let myBotParameters = ToMyBotParameters data

            Some
                {
                    Ticks = ticks
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
    let myBotsToExecute = ResizeArray<MyBotToExecuteData>()

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
let myBotsToExecuteDictionary = Dictionary<string, ResizeArray<MyBotToExecuteData>>()

/// <summary>
/// GetMyBotsToExecuteParameters
/// </summary>
/// <param name="filePathName"></param>
let GetMyBotsToExecuteParameters(filePathName : string) =
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
            ResizeArray<MyBotToExecuteData>()

/// <summary>
/// GetCanEndExecution
/// </summary>
/// <param name="bot"></param>
let GetCanEndExecution(bot : Bot) =
    //bot.BetStatus = BotBetStatus.NoOperationInProgress && bot.Status <> BotStatus.BetPositionClosing
    true

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
    | :? FillOrKillBot as theBot -> Some theBot.Parameters.Odds
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
        for bot in selectionBots do
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
    | WaitingForGapToExecuteMyActionBots
    | ExecuteMyActionBots
        
/// <summary>
/// MiroStrategyBotTrigger
/// </summary>
type MiroGapStrategyBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =       

    let myBotsToExecuteParameters = GetMyBotsToExecuteParameters(defaultArg (botTriggerParameters.GetParameter<string>("CsvFile")) String.Empty)
    let tradeRange = defaultArg (botTriggerParameters.GetParameter<int>("TradeRange")) 3
    let oddsParameterName =
        if (defaultArg (botTriggerParameters.GetParameter<bool>("UseTradingBotParameters")) false)
        then
            "OpenBetPosition.Odds"
        else
            "Odds"

    let mutable status = Initialize
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
                    status <- WaitingForGapToExecuteMyActionBots
                    WaitingForOperation

            | WaitingForGapToExecuteMyActionBots ->

                let backFrom, layFrom, downOdds, upOdds = this.GetTradingRange()

                StopPassiveBotsOutOfTradingRange(market, selection.Id, botName, downOdds, upOdds)

                if selection.GetOfferedPriceDifference() >= 2
                then
                    let openPrices = GetRunningBotsOpenPrices(market, selection.Id, botName)

                    if this.GetHaveMyBotsToExecute(backFrom, layFrom, openPrices)
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

        let oddsContext = selection.OddsContext

        oddsContext.GetOddsDown(bestLayOdds, 1), oddsContext.GetOddsUp(bestBackOdds, 1), 
        oddsContext.GetOddsDown(bestBackOdds, tradeRange), oddsContext.GetOddsUp(bestLayOdds, tradeRange)

    /// <summary>
    /// GetHaveMyBotsToExecute
    /// </summary>
    /// <param name="backFrom"></param>
    /// <param name="layFrom"></param>
    /// <param name="openPrices"></param>
    member this.GetHaveMyBotsToExecute(backFrom, layFrom, openPrices : float list) =
        let oddsContext = selection.OddsContext

        let newBotsToExecute = Seq.toList (seq {
            for myBotsToExecuteData in myBotsToExecuteParameters do
                let odds =
                    if myBotsToExecuteData.BetType = BetType.Back
                    then
                        oddsContext.GetOddsUp(backFrom, myBotsToExecuteData.Ticks)
                    else
                        oddsContext.GetOddsDown(layFrom, myBotsToExecuteData.Ticks)

                if not(List.contains odds openPrices)
                then
                    yield 
                        {
                            MyBotToExecute.BotName = myBotsToExecuteData.BotName
                            MyBotToExecute.MyBotParameters = [{ MyBotParameter.Name = oddsParameterName; MyBotParameter.Value = odds }] @ myBotsToExecuteData.MyBotParameters
                        }
        })

        let status = not newBotsToExecute.IsEmpty

        if status
        then
            myBotsToExecute <- Queue newBotsToExecute

        status

    /// <summary>
    /// ExecuteMyActionBots
    /// </summary>
    member private this.ExecuteMyActionBots() =
        let myBotToExecute = myBotsToExecute.Dequeue()

        if myBotsToExecute.Count = 0
        then
            myBotsToExecute <- nil
            status <- WaitingForGapToExecuteMyActionBots

        ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute (myBotToExecute.BotName, selection, myBotToExecute.MyBotParameters, true)