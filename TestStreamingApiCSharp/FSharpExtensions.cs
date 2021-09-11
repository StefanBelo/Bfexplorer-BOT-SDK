using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using System.Threading;
using System.Threading.Tasks;

namespace BeloSoft.Extensions
{
    public static class FSharpExtensions
    {
        public static Task<T> ExecuteAsyncTask<T>(this FSharpAsync<T> task)
        {
            return FSharpAsync.StartAsTask(task, FSharpOption<TaskCreationOptions>.None, FSharpOption<CancellationToken>.None);
        }
    }
}
