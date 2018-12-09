// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Domain;
using BeloSoft.Bfexplorer.Service;
using BeloSoft.Bfexplorer.Trading;
using Microsoft.FSharp.Core;
using System;

namespace MyCsharpBot
{
    /// <summary>
    /// MyMarketBotParameters
    /// </summary>
    [Serializable]
    public sealed class MyMarketBotParameters : BotParameters
    {
        public MyMarketBotParameters()
        {
            NumberOfIterations = 10;
        }

        public byte NumberOfIterations { get; set; }
    }

    /// <summary>
    /// MyMarketBot
    /// </summary>
    public sealed class MyMarketBot : MarketBaseBot
    {
        private MyMarketBotParameters myParameters;
        private byte iteration = 0;

        public MyMarketBot(Market market, MyMarketBotParameters parameters, IBfexplorerService bfexplorerService)
            : base(market, parameters, bfexplorerService, FSharpOption<ICriteriaEvaluator>.None)
        {
            myParameters = parameters;
        }

        public override void Execute()
        {
            if (iteration++ < myParameters.NumberOfIterations)
            {
                this.OutputMarketMessage(string.Format("Total matched: {0:C}", this.RunningOnMarket.TotalMatched));
            }
            else
            {
                this.Status = BotStatus.ExecutionEnded;
            }
        }
    }
}