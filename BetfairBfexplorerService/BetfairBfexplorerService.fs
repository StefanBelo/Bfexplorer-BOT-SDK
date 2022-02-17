(*
    Copyright © 2021, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.Bfexplorer.Service

open BeloSoft.Data
open BeloSoft.Bfexplorer
open BeloSoft.Bfexplorer.Domain

/// <summary>
/// BetfairBfexplorerService
/// </summary>
type BetfairBfexplorerService() =
    
    let bfexplorerService = BfexplorerService(UiApplication = BfexplorerHost(), initializeBotManager = false)
        
    /// <summary>
    /// Login
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    member _this.Login(userName : string, password : string) = 
        async {
            let! result = bfexplorerService.Login(userName, password, ApplicationFeature.Professional)
            
            return
                if result.IsSuccessResult
                then
                    Result.Success
                else
                    let (_, _, errorMessage) = result.FailureResult

                    Result.Failure errorMessage
        }
        |> Async.RunSynchronously

    /// <summary>
    /// Logout
    /// </summary>
    member _this.Logout() =
        bfexplorerService.Logout() |> Async.RunSynchronously

    /// <summary>
    /// GetMarketCatalogues
    /// </summary>
    /// <param name="filter"></param>
    member _this.GetMarketCatalogues(filter : BetEventFilter) =
        bfexplorerService.GetMarketCatalogues(filter) |> Async.RunSynchronously

    /// <summary>
    /// GetMarket
    /// </summary>
    /// <param name="marketInfo"></param>   
    member _this.GetMarket(marketInfo : MarketInfo) =
        bfexplorerService.GetMarket(marketInfo, loadSelectionMetaData = true) |> Async.RunSynchronously

    /// <summary>
    /// UpdateMarket
    /// </summary>
    /// <param name="market"></param>
    member _this.UpdateMarket(market : Market) =
        bfexplorerService.UpdateMarket(market) |> Async.RunSynchronously


