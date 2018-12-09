// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

module MyStakingPlanTrigger

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// MyStakingPlanTrigger
/// </summary>
type MyStakingPlanTrigger(market : Market, selection : Selection, botName : string, botTriggerParameters : BotTriggerParameters, myBfexplorer : IMyBfexplorer) =

    let myStrategyResults = MyStrategyOperations.GetMyStrategyResults(botName)

    interface IBotTrigger with

        /// <summary>
        /// Execute
        /// </summary>
        member this.Execute() =
            match myStrategyResults with
            | Some myResults ->
                let stake = defaultArg (botTriggerParameters.GetParameter<float>("Stake")) 3.0

                let myStake = 
                    match myResults.BettingStreak with
                    | LosingStreak numberOfBets ->

                        let numberOfRecoveryBets = defaultArg (botTriggerParameters.GetParameter<int>("NumberOfRecoveryBets")) 3

                        if numberOfBets <= numberOfRecoveryBets
                        then
                            let isReallyLoss = myResults.TotalProfit < 0.0

                            if isReallyLoss
                            then
                                (stake * 2.0) + abs(myResults.TotalProfit)
                            else
                                stake * 2.0
                        else
                            myResults.ResetResults()
                            stake
                    | _ ->
                        stake

                let myBotParameters = [ { MyBotParameter.Name = "Stake"; MyBotParameter.Value = myStake } ]

                ExecuteActionBotOnSelectionWithParameters (selection, myBotParameters)
            | None ->
                EndExecutionWithMessage "Please use the parameter: UseMyStrategyResults."

        /// <summary>
        /// EndExecution
        /// </summary>
        member this.EndExecution() =
            ()
