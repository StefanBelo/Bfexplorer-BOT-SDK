#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open System
open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let bfexplorer : IBfexplorerConsole = nil

async {
    let! resultOpenMyFavouriteBetEvent = bfexplorer.OpenMyFavouriteBetEvent("Football - Over/Under 2.5 Goals")

    if resultOpenMyFavouriteBetEvent.IsSuccessResult
    then
        let today = DateTime.Now.Date

        let executeOnMarketInfos =
            bfexplorer.BetEventBrowserMarketCatalogs
            |> List.filter (fun marketCatalog -> marketCatalog.MarketInfo.StartTime.Date = today && marketCatalog.TotalMatched >= 1000.0)
            |> List.map (fun marketCatalog -> marketCatalog.MarketInfo)

        if executeOnMarketInfos.Length > 0
        then
            let! resultStartMyBotStrategy = bfexplorer.StartMyBotStrategy("Trade 1 tick on Under 2.5 goals", TimeSpan.FromMinutes(-10.0), 1.0, executeOnMarketInfos)

            if not resultStartMyBotStrategy.IsSuccessResult
            then
                printf "%s" resultStartMyBotStrategy.FailureMessage
    else
        printf "%s" resultOpenMyFavouriteBetEvent.FailureMessage
}
|> Async.RunSynchronously