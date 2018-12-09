// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

namespace MyFsharpBot

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Trading

module Bots =

    let internal MyBots = [
            { BotDescriptor.BotId = { BotId.Id = 100; BotId.Name = "My Fsharp Test Bot" }; BotDescriptor.Parameters = MyMarketBotParameters() }
            { BotDescriptor.BotId = { BotId.Id = 101; BotId.Name = "Update SP prices" }; BotDescriptor.Parameters = UpdateSpPricesBotParameters() }
            { BotDescriptor.BotId = { BotId.Id = 102; BotId.Name = "Show Selection SP prices" }; BotDescriptor.Parameters = BotParameters() }
            { BotDescriptor.BotId = { BotId.Id = 103; BotId.Name = "Bfexplorer Spreadsheet Demo" }; BotDescriptor.Parameters = SpreadsheetDemoBotParameters() }
        ]

    let internal MyBotsCreators : (Market * Selection * BotParameters * IBfexplorerService -> Bot) list = [
            fun (market : Market, selection : Selection, botParameters : BotParameters, bfexplorerService : IBfexplorerService) 
                -> MyMarketBot(market, botParameters :?> MyMarketBotParameters, bfexplorerService) :> Bot

            fun (market : Market, selection : Selection, botParameters : BotParameters, bfexplorerService : IBfexplorerService) 
                -> UpdateSpPricesMarketBot(market, botParameters :?> UpdateSpPricesBotParameters, bfexplorerService) :> Bot

            fun (market : Market, selection : Selection, botParameters : BotParameters, bfexplorerService : IBfexplorerService)
                -> ShowSelectionSpPricesBot(market, selection, botParameters, bfexplorerService) :> Bot

            fun (market : Market, selection : Selection, botParameters : BotParameters, bfexplorerService : IBfexplorerService) 
                -> SpreadsheetDemoMarketBot(market, botParameters :?> SpreadsheetDemoBotParameters, bfexplorerService) :> Bot
        ]

[<Sealed>]
type BfexplorerBotCreator() =

    interface IBotCreator with

        member this.Bots
            with get() = Bots.MyBots

        member this.GetIsMyBot(botId : BotId) =
            // Check valid range of your bot/s id/s. Bfexplorer BotId starts from 0, use your unique number range for bot identification.
            botId.Id >= 100 && botId.Id <= 103

        member this.GetBotCreator(botId : BotId) =
            if (this :> IBotCreator).GetIsMyBot(botId)
            then
                Some Bots.MyBotsCreators.[botId.Id - 100]
            else
                None