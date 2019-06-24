// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Service;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System.Threading;

namespace JayBeeBot.Helpers
{
    /// <summary>
    /// JayBeeBotHost
    /// </summary>
    internal class JayBeeBotHost : IUiApplication
    {
        private readonly SynchronizationContext uiSynchronizationContext;

        public JayBeeBotHost(SynchronizationContext uiSynchronizationContext)
        {
            this.uiSynchronizationContext = uiSynchronizationContext;
        }

        public SynchronizationContext UiSynchronizationContext
        {
            get { return uiSynchronizationContext; }
        }

        public FSharpAsync<Unit> ExecuteOnUiContext(SynchronizationContext context, FSharpFunc<Unit, Unit> action)
        {
            FSharpAsync.SwitchToContext(uiSynchronizationContext);

            var func = FSharpFunc<Unit, Unit>.ToConverter(action);

            func.DynamicInvoke();

            return FSharpAsync.SwitchToContext(context);
        }

        public FSharpAsync<Unit> ExecuteAsyncJobOnUiContext(SynchronizationContext context, FSharpFunc<Unit, FSharpAsync<Unit>> action)
        {
            FSharpAsync.SwitchToContext(uiSynchronizationContext);

            var func = FSharpFunc<Unit, FSharpAsync<Unit>>.ToConverter(action);

            func.DynamicInvoke();

            return FSharpAsync.SwitchToContext(context);
        }
    }
}