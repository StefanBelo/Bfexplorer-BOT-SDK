// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Domain;
using BeloSoft.Bfexplorer.Trading;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace MyBotTriggersCSharp
{
    /// <summary>
    /// TestBotTrigger
    /// </summary>
    public sealed class TestBotTrigger : IBotTrigger
    {
        private Market market;
        private Selection selection;
        private BotTriggerParameters botTriggerParameters;

        public TestBotTrigger(Market market, Selection selection, string botName, BotTriggerParameters botTriggerParameters, IMyBfexplorer myBfexplorer)
        {
            this.market = market;
            this.selection = selection;
            this.botTriggerParameters = botTriggerParameters;
        }

        /// <summary>
        /// Execute
        /// </summary>
        /// <returns></returns>
        public TriggerResult Execute()
        {
            if (Operators.DefaultArg(botTriggerParameters.GetParameter<bool>("execute"), false))
            {
                FSharpList<MyBotParameter> myBotParameters = FSharpList<MyBotParameter>.Cons(new MyBotParameter("OpenBetPosition.Stake", 2.0), FSharpList<MyBotParameter>.Empty);

                return TriggerResult.NewExecuteActionBotOnSelectionWithParameters(selection, myBotParameters);
            }
            
            return TriggerResult.EndExecution;
        }

        /// <summary>
        /// EndExecution
        /// </summary>
        public void EndExecution()
        {
        }
    }
}