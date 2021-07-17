// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

namespace MyFsharpBot

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.Trading

/// <summary>
/// ShowMyAccountFundsBot
/// </summary>
[<Sealed>]
type ShowMyAccountFundsBot(market : Market, parameters : BotParameters, bfexplorerService : IBfexplorerService) as this =
    inherit MarketBaseBot(market, parameters, bfexplorerService)

    let mutable accountFunds = nil<AccountFunds>

    let loadAvailableBalance() =
        this.BetStatus <- BotBetStatus.MarketDataUpdating

        Async.StartWithContinuations(
            computation = bfexplorerService.GetAccountFunds(),
            continuation = (fun result ->
                this.Status <-
                    if result.IsSuccessResult
                    then
                        accountFunds <- result.SuccessResult

                        BotStatus.WaitingForOperation
                    else
                        this.OutputMarketMessage("Failed to load the available balance!")

                        BotStatus.ExecutionEnded

                this.BetStatus <- BotBetStatus.MarketDataUpdated
            ),
            exceptionContinuation = (fun _ -> this.Status <- BotStatus.ExecutionEnded),
            cancellationContinuation = (fun _ -> this.Status <- BotStatus.ExecutionEnded)
        )

    /// <summary>
    /// Execute
    /// </summary>
    override this.Execute() =
        match this.Status with
        | BotStatus.InitializationInProgress ->

            loadAvailableBalance()
            
            this.Status <- BotStatus.WaitingForEntryCriteria

        | BotStatus.WaitingForOperation ->

            this.OutputMessage(sprintf "The available to bet balance: %.2f, the current exposure: %.2f" accountFunds.AvailableToBetBalance accountFunds.Exposure)

            this.Status <- BotStatus.ExecutionEnded

        | _ -> ()