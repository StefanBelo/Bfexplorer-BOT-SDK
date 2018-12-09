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

let bfexplorer = nil<IBfexplorerConsole>

let getMarketProfit (market : Market) =
    let winner = market.Selections |> Seq.find isWinnerSelection
    let aProfit = winner.SettledProfit

    if aProfit.HasValue then aProfit.Value else 0.0

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

let showMyBotStatistics market =
    let botStatistics = BotStatistics.Get market
    
    botStatistics.Records |> Seq.iter
        (fun record ->
            printfn "%s: %s, %s, %.2f" record.PlayerName record.Score (if record.IsOpenBet then "Open" else "Close") record.Liability
        )

// Show my data
let closedMarkets, openMarkets = bfexplorer.OpenMarkets |> List.partition isClosedMarket

(*
let activeMarkets = bfexplorer.GetMyStrategyActiveMarkets("My Tennis Strategy")
let settledMarkets = bfexplorer.GetMyStrategySettledMarkets("My Tennis Strategy")

let allClosedMarkets = closedMarkets @ settledMarkets
let allActiveMarkets = openMarkets @ activeMarkets

printfn "Profit: %.2f, Liability: %.2f" (allClosedMarkets |> getProfit) (allActiveMarkets |> getLiability)

allActiveMarkets |> List.iter showMyBotStatistics
*)

closedMarkets |> List.iter (fun m -> printfn "%A: %A" m m.SettledProfit)

printfn "Settled profit: %f" (closedMarkets |> List.sumBy (fun m -> m.SettledProfit.Value))

closedMarkets |> List.iter 
    (fun m ->
        m.Selections |> Seq.iter 
            (fun s -> 
                if s.SettledProfit.HasValue
                then
                    printfn "%s: %f" s.Name s.SettledProfit.Value
            )
    )