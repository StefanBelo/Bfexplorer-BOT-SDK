// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Domain;
using BeloSoft.Bfexplorer.Service;
using BeloSoft.Data;
using DevExpress.XtraSpreadsheet;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System;
using System.Threading;

namespace JayBeeBot.Helpers
{
    /// <summary>
    /// JayBeeBotHost
    /// </summary>
    internal class JayBeeBotHost : IBfexplorer
    {
        private SynchronizationContext uiSynchronizationContext;

        public JayBeeBotHost(SynchronizationContext uiSynchronizationContext)
        {
            this.uiSynchronizationContext = uiSynchronizationContext;
        }

        public ISpreadsheetControl BfexplorerSpreadsheet
        {
            get { return null; }
        }

        public SynchronizationContext UiSynchronizationContext
        {
            get { return uiSynchronizationContext; }
        }

        public FSharpAsync<Unit> ExecuteOnUiContext(SynchronizationContext context, FSharpFunc<Unit, Unit> action)
        {
            FSharpAsync.SwitchToContext(uiSynchronizationContext);

            //action();

            return FSharpAsync.SwitchToContext(context);
        }

        public FSharpAsync<Unit> ExecuteAsyncJobOnUiContext(SynchronizationContext context, FSharpFunc<Unit, FSharpAsync<Unit>> action)
        {
            FSharpAsync.SwitchToContext(uiSynchronizationContext);

            //action();

            return FSharpAsync.SwitchToContext(context);
        }

        public void CloseMarket(Market market, [OptionalArgument] FSharpOption<bool> openNext)
        {
            throw new NotImplementedException();
        }

        public FSharpOption<OpenBetEvent> GetOpenBetEvent(Market market)
        {
            throw new NotImplementedException();
        }

        public FSharpOption<Market> GetOpenMarket(MarketInfo marketInfo)
        {
            throw new NotImplementedException();
        }

        public FSharpAsync<DataResult<Market>> OpenMarket(MarketInfo marketInfo, [OptionalArgument] FSharpOption<bool> loadSelectionMetaData)
        {
            throw new NotImplementedException();
        }

        public void WatchMarketSelections(Market market, FSharpList<Selection> selections)
        {
            throw new NotImplementedException();
        }

        public FSharpAsync<DataResult<FSharpList<Market>>> OpenAssociatedMarkets(MarketInfo marketInfo, string[] marketTypeNames, FSharpOption<bool> loadSelectionMetaData)
        {
            throw new NotImplementedException();
        }

        public void ShowAndActivateMarketSelection(Market market, Selection selection)
        {
            throw new NotImplementedException();
        }

        public Market ReopenMarket(Market market)
        {
            throw new NotImplementedException();
        }
    }
}