#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Data.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Domain.dll"
#r @"C:\Program Files (x86)\BeloSoft\Bfexplorer\BeloSoft.Bfexplorer.Service.Core.dll"

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

let bfexplorer : IBfexplorerConsole = nil

let market = bfexplorer.ActiveMarket
let selection = market.Selections.[0]

let offeredToLay = selection.ToLay.GetOfferedSize()
let offeredToBack = selection.ToBack.GetOfferedSize()

printf "WOM: %.2f" (offeredToLay / (offeredToLay + offeredToBack))