// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Betfair.API;
using BeloSoft.Betfair.API.Models;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BetfairApiConsole
{
    /// <summary>
    /// FSharp
    /// </summary>
    static public class FSharp
    {
        /// <summary>
        /// ExecuteAsyncTask
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        static public Task<T> ExecuteAsyncTask<T>(this FSharpAsync<T> task)
        {
            return FSharpAsync.StartAsTask<T>(task, FSharpOption<TaskCreationOptions>.None, FSharpOption<CancellationToken>.None);
        }
    }

    class Program
    {
        /// <summary>
        /// ExecuteMyTest
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        static async Task ExecuteMyTest(string username, string password)
        {
            var betfairServiceProvider = new BetfairServiceProvider();

            var accountOperations = betfairServiceProvider.AccountOperations;
            var browsingOperations = betfairServiceProvider.BrowsingOperations;

            var loginResult = await accountOperations.Login(username, password).ExecuteAsyncTask();

            if (loginResult.IsSuccess)
            {
                var filter = new MarketFilter
                {
                    eventTypeIds = new int[] { 1 },
                    marketCountries = new string[] { "GB" },
                    inPlayOnly = false,
                    turnInPlayEnabled = true,
                    marketTypeCodes = new string[] { "MATCH_ODDS" }
                };

                var marketProjection = FSharpOption<MarketProjection[]>.Some(new MarketProjection[] {
                        MarketProjection.EVENT,
                        MarketProjection.MARKET_START_TIME,
                        MarketProjection.COMPETITION,
                        MarketProjection.RUNNER_DESCRIPTION,
                        MarketProjection.MARKET_DESCRIPTION });

                var marketCataloguesResult = 
                    await browsingOperations
                        .GetMarketCatalogues(filter, 10, marketProjection, FSharpOption<MarketSort>.Some(MarketSort.MAXIMUM_TRADED), FSharpOption<string>.None)
                        .ExecuteAsyncTask();

                if (marketCataloguesResult.IsSuccessResult)
                {
                    var marketCatalogues = marketCataloguesResult.SuccessResult;

                    foreach (var marketCatalogue in marketCatalogues)
                    {
                        var betEvent = marketCatalogue.@event;

                        Console.WriteLine($"{betEvent.openDate}: {betEvent.name}, eventId: {betEvent.id}, marketId: {marketCatalogue.marketId}");
                    }
                }

                await accountOperations.Logout().ExecuteAsyncTask();
            }
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                throw new Exception("Please enter your betfair user name and password!");
            }

            var username = args[0];
            var password = args[1];

            var task = ExecuteMyTest(username, password);

            Task.WaitAll(task);
        }
    }
}