(*
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.TennisScoreProvider.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\MyTennisTrading.Bot.dll"
*)


#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Data.dll"
#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Bfexplorer.Domain.dll"
#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Bfexplorer.Service.Core.dll"

#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\BeloSoft.Bfexplorer.TennisScoreProvider.dll"
#r @"D:\Projects\Bfexplorer\Development\Applications\BeloSoft.Bfexplorer.BotApplication\bin\Debug\MyTennisTrading.Bot.dll"


open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain
open BeloSoft.Bfexplorer.Service
open BeloSoft.Bfexplorer.MyTennisTrading

//let bfexplorer = nil<IBfexplorerConsole>

let getMarketProfit (market : Market) =
    if market.SettledProfit.HasValue
    then
        market.SettledProfit.Value
    else
        0.0

let getMarketLiability (market : Market) =
    market.Selections 
    |> Seq.map 
        (fun selection ->
            let profit = 
                if selection.Profit.HasValue
                then
                    selection.Profit.Value
                else
                    0.0

            if profit < 0.0
            then
                abs profit
            else
                0.0
        )
    |> Seq.max

let getProfit markets =
    markets |> List.sumBy getMarketProfit

let getLiability markets =
    markets |> List.sumBy getMarketLiability

let showMyBotStatistics (market : Market) =
    match market.GetData<BotStatistics>("botStatistics") with
    | Some botStatistics ->
    
        botStatistics.Records |> Seq.iter
            (fun record ->
                printfn "%s: %s, %s, %.2f" record.PlayerName record.Score (if record.IsOpenBet then "Open" else "Close") record.Liability
            )

    | None -> ()

let showMyBotStatisticsForClosedMarket (market : Market) =
    match market.GetData<BotStatistics>("botStatistics") with
    | Some botStatistics ->

        printfn "%s: %d, %.2f" market.MarketInfo.BetEvent.Name botStatistics.TradingSessions botStatistics.Liability

    | None -> ()
    
// Show my data
let closedMarkets, openMarkets = bfexplorer.OpenMarkets |> List.partition isClosedMarket

let activeMarkets = bfexplorer.GetMyStrategyActiveMarkets("My Tennis Strategy")
let settledMarkets = bfexplorer.GetMyStrategySettledMarkets("My Tennis Strategy")

let allClosedMarkets = closedMarkets @ settledMarkets
let allActiveMarkets = openMarkets @ activeMarkets

printfn "Profit: %.2f, Liability: %.2f" (allClosedMarkets |> getProfit) (allActiveMarkets |> getLiability)
printfn "Active markets: %d" allActiveMarkets.Length; allActiveMarkets |> List.iter showMyBotStatistics
printfn "Closed markets: %d" allClosedMarkets.Length; allClosedMarkets |> List.iter showMyBotStatisticsForClosedMarket