(*
    Copyright © 2018 - 2019, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.MyFootballStrategy.App.Services

open System

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

[<AutoOpen>]
module internal TeamNameOperations =

    let gbTeamNames =
        [
            "Man City", "Manchester City"
            "Man Utd", "Manchester United"
            "Leicester", "Leicester City"
            "Wolves", "Wolverhampton"
            "Newcastle", "Newcastle United"
            "C Palace", "Crystal Palace"
            "Brighton", "Brighton & Hove Albion"
            "West Ham", "West Ham United"
            "Cardiff", "Cardiff City"
            "Sheff Utd", "Sheffield Utd"
        ]

    let getTeamName name (teamNames : (string * string) list) =
        match teamNames |> List.tryFind (fun (betfairName, _sofaScoreName) -> betfairName = name) with
        | Some (_betfairName, sofaScoreName) -> Some sofaScoreName
        | None -> None

    let tryGetTeamNameByCountry (country : string) (name : string) =
        match country with
        | "GB" -> gbTeamNames |> getTeamName name
        | _ -> None

    let getLongestPart (name : string) =
        let nameParts = name.Replace("-", " ").Split(' ')
        let index = 
            if nameParts.Length = 1
            then
                0
            else
                let lengths = nameParts |> Array.map (String.length)
                let maxLength = lengths |> Array.max

                lengths |> Array.findIndex ((=) maxLength)

        nameParts.[index]
  
    let getTeamNameByCountry country name =
        match tryGetTeamNameByCountry country name with
        | Some sofaScoreName -> sofaScoreName
        | None -> getLongestPart name

    let toTeamNames (marketInfo : MarketInfo) =
        let country = marketInfo.BetEvent.CountryCode
        let teamNames = marketInfo.EventName.Split([| " v " |], StringSplitOptions.None)

        getTeamNameByCountry country teamNames.[0], getTeamNameByCountry country teamNames.[1]
