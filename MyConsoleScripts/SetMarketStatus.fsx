#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let bfexplorer : IBfexplorerConsole = nil

let market = bfexplorer.ActiveMarket

DeactiveMarketMonitoringStatus(market)