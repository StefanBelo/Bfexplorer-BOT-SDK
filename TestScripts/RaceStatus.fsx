#I @"C:\Program Files\BeloSoft\Bfexplorer\"

#r "BeloSoft.Data.dll"
#r "BeloSoft.Betfair.API.dll"

open System
open System.Net

open BeloSoft.Data
open BeloSoft.Betfair.API
open BeloSoft.Betfair.API.Models

ServicePointManager.Expect100Continue <- false
ServicePointManager.UseNagleAlgorithm <- false

let betfairServiceProvider = BetfairServiceProvider()

let accountOperations = betfairServiceProvider.AccountOperations

let getUserDetails username password = 
    async {
        let! loginResult = accountOperations.Login(username, password)

        if loginResult.IsSuccessResult
        then
            let! accountDetailsResult = accountOperations.GetAccountDetails()

            if accountDetailsResult.IsSuccessResult
            then
                let accountDetails = accountDetailsResult.SuccessResult

                Console.WriteLine(sprintf "%s, %s, %s %s" username password accountDetails.firstName accountDetails.lastName)
            else
                Console.WriteLine(sprintf "Failed to get account details: %s" accountDetailsResult.FailureMessage)

            do! accountOperations.Logout() |> Async.Ignore
        else
            Console.WriteLine(sprintf "Failed to login: %s" loginResult.FailureMessage)
    }
    |> Async.RunSynchronously

getUserDetails "user" "yourPassword"