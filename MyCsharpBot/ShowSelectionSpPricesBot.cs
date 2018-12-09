// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Domain;
using BeloSoft.Bfexplorer.Service;
using BeloSoft.Bfexplorer.Trading;
using BeloSoft.Data;

namespace MyCsharpBot
{
    /// <summary>
    /// ShowSelectionSpPricesBot
    /// </summary>
	public sealed class ShowSelectionSpPricesBot : SelectionBot
	{
		private HistoryValue<double> myLastPriceTraded = new HistoryValue<double>();

		public ShowSelectionSpPricesBot(Market market, Selection selection, BotParameters parameters, IBfexplorerService bfexplorerService)
            : base(market, selection, parameters, bfexplorerService)
		{
            this.Status = this.RunningOnSelection.Data.ContainsKey("betfairSpPrices") ? BotStatus.WaitingForOperation : BotStatus.ExecutionEnded;
		}

		public override void Execute()
		{
            switch (this.Status)
            {
                case BotStatus.WaitingForOperation:
                    var selection = this.RunningOnSelection;

                    if (selection.IsUpdated && myLastPriceTraded.SetValue(selection.LastPriceTraded))
                    {
                        var betfairSpPrices = selection.Data["betfairSpPrices"] as BetfairSpPrices;

                        if (betfairSpPrices != null)
                        {
                            this.OutputMessage(string.Format("Last price traded: {0:N2}, SP near: {1:N2}, far: {2:N2}, actual: {3:N2}", myLastPriceTraded.Value, betfairSpPrices.NearPrice, betfairSpPrices.FarPrice, betfairSpPrices.ActualSP));
                        }
                    }
                    break;
                default:
                    break;                        
            }
		}
	}
}