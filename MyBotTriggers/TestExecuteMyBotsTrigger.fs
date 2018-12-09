// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module TestExecuteMyBotsTrigger

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// MyBotToExecute
/// </summary>
type MyBotToExecute =
    {
        BotName : string
        MinimumOdds : float
        MaximumOdds : float
        MyBotParameters : MyBotParameter list
    }

let MyBotsToExecute = [
        { 
            BotName = "My Trader Bot"
            MinimumOdds = 2.5
            MaximumOdds = 3.5
            MyBotParameters = [ { MyBotParameter.Name = "OpenBetPosition.Stake"; MyBotParameter.Value = 500.0 } ] 
        }
        { 
            BotName = "My Trader Bot"
            MinimumOdds = 3.5
            MaximumOdds = 5.0
            MyBotParameters = [ { MyBotParameter.Name = "CloseBetPosition.Profit"; MyBotParameter.Value = 2 } ]
        }
    ]

/// <summary>
/// TestExecuteMyBotsTrigger
/// </summary>
type TestExecuteMyBotsTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =
    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            let lastPriceTraded = selection.LastPriceTraded
            
            match MyBotsToExecute |> List.tryFind (fun m -> lastPriceTraded >= m.MinimumOdds && lastPriceTraded <= m.MaximumOdds) with
            | Some myBotToExecute ->            
                ExecuteMyActionBotOnSelectionWithParametersAndContinueToExecute (myBotToExecute.BotName, selection, myBotToExecute.MyBotParameters, false)
            | None ->
                EndExecutionWithMessage (sprintf "Not in allowed price range %s: %.2f" selection.Name lastPriceTraded)

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()