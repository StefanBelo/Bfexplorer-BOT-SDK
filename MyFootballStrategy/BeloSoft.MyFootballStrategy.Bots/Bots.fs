(*
    Copyright © 2019, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.MyFootballStrategy.Bots

open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading
open BeloSoft.Bfexplorer.Service

module Bots =    

    let internal MyBots = 
        [
        ]

    let internal MyBotsCreators : (Market * Selection * BotParameters * IBfexplorerService -> Bot) list = 
        [
        ]

/// <summary>
/// BfexplorerBotCreator
/// </summary>
[<Sealed>]
type BfexplorerBotCreator() =

    let isMyBot (botId : BotId) =
        botId.Id >= 5100 && botId.Id <= 5101

    interface IBotCreator with

        member _this.Bots
            with get() = Bots.MyBots

        member _this.GetIsMyBot(botId : BotId) =
            isMyBot botId

        member _this.GetBotCreator(botId : BotId) =
            if isMyBot botId
            then
                Some Bots.MyBotsCreators.[botId.Id - 5100]
            else
                None