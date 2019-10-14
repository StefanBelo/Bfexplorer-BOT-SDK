(*
    Copyright © 2018 - 2019, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.MyFootballStrategy.App.Services

open System
open System.Collections.Generic

open Diacritics.Extensions

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

open BeloSoft.SofaScoreProvider
open BeloSoft.SofaScoreProvider.Models
  
[<AutoOpen>]
module private SofaScoreDataOperations =

    let isMyFootballMatch (homeTeamName : string, awayTeamName : string) (footballMatch : FootballMatch) =
        let homeTeam = footballMatch.HomeTeam.Name.RemoveDiacritics()
        let awayTeam = footballMatch.AwayTeam.Name.RemoveDiacritics()

        //System.Diagnostics.Debug.WriteLine(sprintf "%s - %s | %s - %s" homeTeamName awayTeamName footballMatch.HomeTeam.Name footballMatch.AwayTeam.Name)

        (homeTeam.Contains homeTeamName) || (awayTeam.Contains awayTeamName)

    [<Literal>]
    let FootballMatchDataKey = "FootballMatch"

/// <summary>
/// SofaScoreDataProvider
/// </summary>
type SofaScoreDataProvider() =

    let listOfLeagueMatchesByDate = Dictionary<DateTime, LeagueMatches list>()
    let mutable initializationInProgress = false

    static let instance = lazy(SofaScoreDataProvider())

    let getListOfLeagueMatches date = async {
        if listOfLeagueMatchesByDate.ContainsKey(date)
        then
            return DataResult.Success listOfLeagueMatchesByDate.[date]
        else
            let! result = SofaScoreProvider.GetFootballMatches(date)

            if result.IsSuccessResult
            then
                initializationInProgress <- false
                listOfLeagueMatchesByDate.[date] <- result.SuccessResult

            return result
    }

    let toFootballMatch (listOfLeagueMatches : LeagueMatches list) (market : Market) =
        match market.GetData<FootballMatch>(FootballMatchDataKey) with
        | Some footballMatch -> Some footballMatch
        | None ->

            let teamNames = toTeamNames market.MarketInfo

            let myFootballMatch =
                listOfLeagueMatches 
                |> List.map (fun leagueMatches -> leagueMatches.Matches)
                |> List.concat
                |> List.tryFind (isMyFootballMatch teamNames)

            myFootballMatch |> Option.iter (fun footballMatch -> market.SetData(FootballMatchDataKey, footballMatch))
            myFootballMatch

    let execute (market : Market) (job : FootballMatch -> Async<DataResult<'T>>) = async {
        let! result = getListOfLeagueMatches market.MarketInfo.StartTime.Date

        if result.IsSuccessResult
        then
            match market |> toFootballMatch result.SuccessResult with
            | Some footballMatch -> return! job footballMatch
            | None -> return DataResult.Failure "No match details!"
        else
            return DataResult.Failure result.FailureMessage
    }

    /// <summary>
    /// Instance
    /// </summary>
    static member Instance
        with get() = instance.Value

    /// <summary>
    /// Initialize
    /// </summary>
    member _this.Initialize() = async {
        let today = DateTime.Today

        if listOfLeagueMatchesByDate.ContainsKey(today)
        then
            return DataResult.Success true
        elif initializationInProgress
        then
            return DataResult.Success false
        else
            initializationInProgress <- true

            let! result = getListOfLeagueMatches today

            return
                if result.IsSuccessResult
                then
                    DataResult.Success true
                else
                    DataResult.Failure result.FailureMessage
    }

    /// <summary>
    /// Reset
    /// </summary>
    member _this.Reset() =
        listOfLeagueMatchesByDate.Clear()    
        
    /// <summary>
    /// GetMatchDetails
    /// </summary>
    /// <param name="market"></param>
    member _this.GetMatchDetails(market : Market) =
        execute market (fun footballMatch -> SofaScoreProvider.GetFootballMatchDetails(footballMatch.Id))

    /// <summary>
    /// GetMatchStatistics
    /// </summary>
    /// <param name="market"></param>
    member _this.GetMatchStatistics(market : Market) =
        execute market (fun footballMatch -> SofaScoreProvider.GetFootballMatchStatistics(footballMatch.Id))

    /// <summary>
    /// GetPreviousMatches
    /// </summary>
    /// <param name="market"></param>
    member _this.GetPreviousMatches(market : Market) =
        execute market (fun footballMatch -> SofaScoreProvider.GetFootballPreviousMatches(footballMatch.Id))

    /// <summary>
    /// GetMatchTeamsLiveForm
    /// </summary>
    /// <param name="market"></param>
    member _this.GetMatchTeamsLiveForm(market : Market) =
        execute market (fun footballMatch -> SofaScoreProvider.GetFootballMatchTeamsLiveForm(footballMatch.Id))