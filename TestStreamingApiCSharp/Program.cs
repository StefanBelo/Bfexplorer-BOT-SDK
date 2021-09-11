using BeloSoft.Betfair.StreamingAPI;
using BeloSoft.Betfair.StreamingAPI.Models;
using BeloSoft.Bfexplorer;
using BeloSoft.Bfexplorer.Domain;
using BeloSoft.Bfexplorer.Service;
using BeloSoft.Extensions;
using Microsoft.FSharp.Collections;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TestStreamingApiCSharp
{
    /// <summary>
    /// Program
    /// </summary>
    public class Program
    {
        private static (string UserName, string Password)? GetUserNameAndPassword(string[] args)
        {
            if (args.Length == 2)
            {
                return (args[0], args[1]);
            }
            else
            {
                return null;
            }
        }

        private static void SetNotify(Market market)
        {
            ((INotifyPropertyChanged)market).PropertyChanged += (sender, arg) =>
                {
                    if (arg.PropertyName == "TotalMatched")
                    {
                        Console.WriteLine($"{market}: {market.TotalMatched:N2}");
                    }
                };
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static async Task Main(string[] args)
        {
            var entryParameters = GetUserNameAndPassword(args);

            if (!entryParameters.HasValue)
            {
                throw new Exception("Please enter your betfair user name and password!");
            }

            var bfexplorerService = new BfexplorerService(initializeBotManager: false)
                {
                    UiApplication = new BfexplorerHost()
                };

            var loginResult = await bfexplorerService.Login(entryParameters?.UserName, entryParameters?.Password).ExecuteAsyncTask();

            if (loginResult.IsSuccessResult)
            {
                var marketUpdateService = new MarketUpdateService(bfexplorerService, StreamingData.MarketDataFilterForPassiveMarkets);

                marketUpdateService.OnMarketsOpened += (sender, markets) =>
                    {
                        foreach (var market in markets)
                        {
                            Console.WriteLine($"{market}");
                            SetNotify(market);
                        }
                    };

                var startResult = await marketUpdateService.Start().ExecuteAsyncTask();

                if (startResult.IsSuccessResult)
                {
                    // Horse Racing
                    var filter = ListModule.OfArray(new [] { 
                        BetEventFilterParameter.NewBetEventTypeIds(new [] { 7 }),
                        BetEventFilterParameter.NewMarketTypeCodes(new [] { "WIN" }),
                        BetEventFilterParameter.NewCountries(new [] { "GB" })
                    });

                    var subscribeResult = await marketUpdateService.Subscribe(filter).ExecuteAsyncTask();

                    string message;

                    if (subscribeResult.IsSuccessResult)
                    {
                        message = marketUpdateService.ConnectionHasSuccessStatus ? "Successfully subscribed." : $"Failed to subscribe: {marketUpdateService.ErrorMessage}";
                    }
                    else
                    {
                        message = subscribeResult.FailureMessage;
                    }

                    Console.WriteLine($"{message}\nPress any key to exit.");
                    Console.ReadKey();

                    await marketUpdateService.Stop().ExecuteAsyncTask();
                }

                await bfexplorerService.Logout().ExecuteAsyncTask();
            }
        }
    }
}