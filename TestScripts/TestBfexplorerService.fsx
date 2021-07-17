#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Betfair.API.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.API.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.dll"

#load "MyBetfairCredentials.fsx"
open MyBetfairCredentials

open BeloSoft.Data
open BeloSoft.Bfexplorer.Service

let bfexplorerService = BfexplorerService()

bfexplorerService.ServiceStatus.IsPracticeMode <- true

async {
    let! resultLogin = bfexplorerService.Login(username, password)

    if resultLogin.IsSuccessResult
    then
        let! resultAccountFunds = bfexplorerService.GetAccountFunds()

        printfn "%s" <|
            match resultAccountFunds with
            | DataResult.Success accountFunds -> sprintf "%.2f" accountFunds.AvailableToBetBalance
            | DataResult.Failure errorMessage -> errorMessage

        do! bfexplorerService.Logout() |> Async.Ignore    
}
|> Async.RunSynchronously