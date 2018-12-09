// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Domain;
using BeloSoft.Bfexplorer.Service;
using BeloSoft.Bfexplorer.Trading;
using Microsoft.FSharp.Core;
using System;
using ApiModels = BeloSoft.Betfair.API.Models;

namespace MyCsharpBot
{
    /// <summary>
    /// BetfairSpPrices
    /// </summary>
    public class BetfairSpPrices
    {
        public double NearPrice { get; set; }
        public double FarPrice { get; set; }
        public double ActualSP { get; set; }
    }

    /// <summary>
    /// UpdateSpPricesBotParameters
    /// </summary>
    [Serializable]
    public sealed class UpdateSpPricesBotParameters : BotParameters
    {
        public UpdateSpPricesBotParameters()
        {
            UpdateInterval = TimeSpan.FromSeconds(5);
        }

        public TimeSpan UpdateInterval { get; set; }
    }

    /// <summary>
    /// SelectionUpdateFunc
    /// </summary>
    public abstract class SelectionUpdateFunc : FSharpFunc<Tuple<Selection, ApiModels.Runner>, Unit>
    {
    }

    /// <summary>
    /// UpdateSelectionSpPricesFunc
    /// </summary>
    public sealed class UpdateSelectionSpPricesFunc : SelectionUpdateFunc
    {
        public override Unit Invoke(Tuple<Selection, ApiModels.Runner> tupledArg)
        {
            Selection selection = tupledArg.Item1;
            ApiModels.Runner runner = tupledArg.Item2;

            BetfairSpPrices betfairSpPrices = selection.Data["betfairSpPrices"] as BetfairSpPrices;
            ApiModels.StartingPrices spPrices = runner.sp;

            betfairSpPrices.NearPrice = spPrices.nearPrice;
            betfairSpPrices.FarPrice = spPrices.farPrice;
            betfairSpPrices.ActualSP = spPrices.actualSP;

            return null;
        }
    }

    /// <summary>
    /// DataUpdateFunc
    /// </summary>
    public abstract class DataUpdateFunc : FSharpFunc<bool, Unit>
    {
    }

    /// <summary>
    /// UpdateMarketDataFunc
    /// </summary>
    public sealed class UpdateMarketDataFunc : DataUpdateFunc
    {
        public override Unit Invoke(bool result)
        {
            return null;
        }
    }

    /// <summary>
    /// UpdateSpPricesMarketBot
    /// </summary>
    public sealed class UpdateSpPricesMarketBot : MarketBot
    {
        // Const
        private static ApiModels.PriceProjection BetfairSpPriceProjection = ApiModels.PriceProjection.DefaultBetfairSpPrice();

        private static UpdateSelectionSpPricesFunc updateSelectionSpPricesFunc = new UpdateSelectionSpPricesFunc();
        private static UpdateMarketDataFunc updateMarketDataFunc = new UpdateMarketDataFunc();

        // Data
        private UpdateSpPricesBotParameters myParameters;
        private DateTime timeToUpdated = DateTime.MinValue;

        public UpdateSpPricesMarketBot(Market market, UpdateSpPricesBotParameters parameters, IBfexplorerService bfexplorerService)
            : base(market, parameters, bfexplorerService, FSharpOption<ICriteriaEvaluator>.None)
        {
            myParameters = parameters;

            foreach (var selection in market.Selections)
            {
                selection.Data["betfairSpPrices"] = new BetfairSpPrices();
            }
        }

        public override void Execute()
        {
            switch (this.Status)
            {
                case BotStatus.WaitingForEntryCriteria:
                    if (this.EvaluateCriteria())
                    {
                        this.Status = BotStatus.WaitingForOperation;
                    }
                    break;
                case BotStatus.WaitingForOperation:

                    if (DateTime.Now >= timeToUpdated)
                    {
                        this.UpdateMarketData(BetfairSpPriceProjection, updateSelectionSpPricesFunc, updateMarketDataFunc);
                        timeToUpdated = DateTime.Now.Add(myParameters.UpdateInterval);
                    }

                    if (this.RunningOnMarket.IsInPlay)
                    {
                        this.Status = BotStatus.ExecutionEnded;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}