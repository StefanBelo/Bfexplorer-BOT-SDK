// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MyGapStrategyBotTrigger

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// GetCanEndExecution
/// </summary>
/// <param name="bot"></param>
let GetCanEndExecution(bot : Bot) =
    bot.Status <> BotStatus.BetPositionClosing

/// <summary>
/// GetBotOpenPrice
/// </summary>
/// <param name="bot"></param>
let GetBotOpenPrice(bot : Bot) =
    match bot with
    | :? PlaceBetBot as theBot -> Some theBot.Parameters.Odds
    | :? FillOrKillBot as theBot -> Some theBot.Parameters.Odds
    | :? ScratchTradingBot as theBot -> Some theBot.Parameters.Odds
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
/// ReportBackOffer
/// </summary>
/// <param name="selection"></param>
let ReportBackOffer(selection : Selection) =
    let prices = selection.ToBack.Prices

    seq {
        for i = 2 downto 0 do
            let price = prices.[i]

            if price.IsValid
            then
                yield (sprintf "%.0f" price.Size)
    }
    |> String.concat ", "

/// <summary>
/// ReportLayOffer
/// </summary>
/// <param name="selection"></param>
let ReportLayOffer(selection : Selection) =
    let prices = selection.ToLay.Prices

    seq {
        for price in prices do
            if price.IsValid
            then
                yield (sprintf "%.0f" price.Size)
    }
    |> String.concat ", "

/// <summary>
/// TriggerStatus
/// </summary>
type TriggerStatus =
    | WaitingForGapToExecuteMyActionBots
    | ExecuteMyActionBots
        
/// <summary>
/// MyGapStrategyBotTrigger
/// </summary>
type MyGapStrategyBotTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =       

    let tradeRange = defaultArg (botTriggerParameters.GetParameter<int>("TradeRange")) 3
    let betTypeParameterName, oddsParameterName =
        if (defaultArg (botTriggerParameters.GetParameter<bool>("UseTradingBotParameters")) false)
        then
            "OpenBetPosition.BetType", "OpenBetPosition.Odds"
        else
            "BetType", "Odds"
        
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            let backFrom, layFrom, downOdds, upOdds = this.GetTradingRange()

            StopPassiveBotsOutOfTradingRange(market, selection.Id, botName, downOdds, upOdds)

            if selection.GetOfferedPriceDifference() >= 2
            then
                this.ExecuteMyActionBot(backFrom, layFrom, GetRunningBotsOpenPrices(market, selection.Id, botName))
                //this.ShowData(backFrom, layFrom)
            else                
                WaitingForOperation

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
    /// ExecuteMyActionBot
    /// </summary>
    /// <param name="backFrom"></param>
    /// <param name="layFrom"></param>
    /// <param name="openPrices"></param>
    member this.ExecuteMyActionBot(backFrom, layFrom, openPrices : float list) =
        let betType =
            if selection.GetWeightOfMoney() >= 0.5
            then
                BetType.Back
            else
                BetType.Lay

        let odds =
            if betType = BetType.Back
            then
                backFrom
            else
                layFrom

        if List.contains odds openPrices
        then
            WaitingForOperation
        else
            let myBotParameters = [
                { MyBotParameter.Name = betTypeParameterName; MyBotParameter.Value = betType }
                { MyBotParameter.Name = oddsParameterName; MyBotParameter.Value = odds }
            ]

            ExecuteActionBotOnSelectionWithParametersAndContinueToExecute (selection, myBotParameters, true)

    /// <summary>
    /// ShowData
    /// </summary>
    /// <param name="backFrom"></param>
    /// <param name="layFrom"></param>
    member this.ShowData(backFrom, layFrom) =
        let wom = selection.GetWeightOfMoney()
        let betType =
            if wom >= 0.5
            then
                BetType.Back
            else
                BetType.Lay

        let odds =
            if betType = BetType.Back
            then
                backFrom
            else
                layFrom
        
        myBfexplorer.BfexplorerService.OutputMessage(sprintf "%s: %A > [%s] %.2f | %.2f | %.2f [%s]" selection.Name betType (ReportBackOffer selection) layFrom wom backFrom (ReportLayOffer selection))
        WaitingForOperation