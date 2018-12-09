// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JayBeeBot.Helpers
{
    /// <summary>
    /// FSharpHelper
    /// </summary>
    static public class FSharpHelper
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

        /// <summary>
        /// ToFSharpList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static FSharpList<T> ToFSharpList<T>(this IList<T> input)
        {
            return CreateFSharpList(input, 0);
        }

        /// <summary>
        /// CreateFSharpList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static FSharpList<T> CreateFSharpList<T>(IList<T> input, int index)
        {
            if (index >= input.Count)
            {
                return FSharpList<T>.Empty;
            }
            else
            {
                return FSharpList<T>.Cons(input[index], CreateFSharpList(input, index + 1));
            }
        }
    }
}