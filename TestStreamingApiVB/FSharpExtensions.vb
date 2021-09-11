Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.FSharp.Control
Imports Microsoft.FSharp.Core

Module FSharpExtensions

    <Extension>
    Public Function ExecuteAsyncTask(Of T)(task As FSharpAsync(Of T)) As Task(Of T)
        Return FSharpAsync.StartAsTask(task, FSharpOption(Of TaskCreationOptions).None, FSharpOption(Of CancellationToken).None)
    End Function
End Module