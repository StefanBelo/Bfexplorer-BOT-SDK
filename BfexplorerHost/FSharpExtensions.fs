(*
    Copyright © 2021, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.Bfexplorer

module FSharpExtensions =

    let ExecuteAsyncTask<'T> (task : Async<'T>) =     
        Async.StartAsTask task

