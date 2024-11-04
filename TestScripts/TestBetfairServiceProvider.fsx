#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Betfair.API.dll"

#load "MyBetfairCredentials.fsx"
open MyBetfairCredentials

open System.Net

open BeloSoft.Data
open BeloSoft.Betfair.API
open BeloSoft.Betfair.API.Models

ServicePointManager.Expect100Continue <- false
ServicePointManager.UseNagleAlgorithm <- false

let accountOperations = 
    let betfairServiceProvider = BetfairServiceProvider()

    betfairServiceProvider.AccountOperations

async {
    let! resultLogin = accountOperations.Login(username, password)

    if resultLogin.IsSuccessResult
    then
        let! resultAccountFunds = accountOperations.GetAccountFunds()

        printfn "%s" <|
            match resultAccountFunds with
            | DataResult.Success accountFundsResponse -> sprintf "%.2f" accountFundsResponse.availableToBetBalance
            | DataResult.Failure errorMessage -> errorMessage

        do! accountOperations.Logout() |> Async.Ignore
}
|> Async.RunSynchronously