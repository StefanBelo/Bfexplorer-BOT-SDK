(*
    Copyright © 2019, Stefan Belopotocan, http://bfexplorer.net
*)

namespace BeloSoft.MyFootballStrategy.Bots.Models

open System

open BeloSoft.Data
open BeloSoft.Bfexplorer.Domain

open BeloSoft.SofaScoreProvider

open BeloSoft.Bfexplorer.FootballScoreProvider.Models

/// <summary>
/// RatingData
/// </summary>
[<NoEquality; NoComparison>]
type RatingData =
    {
        ShotsOnTargetValues : Models.MatchStatisticsValues
        ShotsOffTargetValues : Models.MatchStatisticsValues
        BallPossessionValues : Models.MatchStatisticsValues
    }

    static member CalculateShotsOnOffTarget (startData : RatingData, ratingData : RatingData) =
        let mutable homeRating = (ratingData.ShotsOnTargetValues.Home - startData.ShotsOnTargetValues.Home) * 10us
        let mutable awayRating = (ratingData.ShotsOnTargetValues.Away - startData.ShotsOnTargetValues.Away) * 10us

        homeRating <- homeRating + (ratingData.ShotsOffTargetValues.Home - startData.ShotsOffTargetValues.Home) * 10us
        awayRating <- awayRating + (ratingData.ShotsOffTargetValues.Away - startData.ShotsOffTargetValues.Away) * 10us

        homeRating, awayRating

    static member CalculatePossession (ratingData : RatingData) =
        let homeRating = uint16 (float ratingData.BallPossessionValues.Home / 5.0)
        let awayRating = uint16 (float ratingData.BallPossessionValues.Away / 5.0)

        homeRating, awayRating

[<AutoOpen>]
module private MatchStatisticsOperations =

    let MatchStatisticsValuesEmpty = 
        { 
            Models.MatchStatisticsValues.Home = 0us
            Models.MatchStatisticsValues.Away = 0us
        }

    (*
    let RatingDataEmpty =
        {
            ShotsOnTargetValues = MatchStatisticsValuesEmpty
            ShotsOffTargetValues = MatchStatisticsValuesEmpty
            BallPossessionValues = MatchStatisticsValuesEmpty
        }
    *)

    let tryGetGroupMatchStatisticsItems name (allMatchStatisticsItems : Models.GroupMatchStatisticsItems list) =
        allMatchStatisticsItems |> List.tryFind (fun groupMatchStatisticsItems -> groupMatchStatisticsItems.Name = name)

    let tryGetMatchStatisticsItem name (matchStatisticsItems : Models.GroupMatchStatisticsItems) =
        matchStatisticsItems.Items |> List.tryFind (fun matchStatisticsItem -> matchStatisticsItem.Name = name)

    let tryGetShotsStatistics (allMatchStatisticsItems : Models.GroupMatchStatisticsItems list) = maybe {
        let! matchStatisticsItem = allMatchStatisticsItems |> tryGetGroupMatchStatisticsItems "Shots"
        let! shotsOffTarget = matchStatisticsItem |> tryGetMatchStatisticsItem "Shots off target"

        let shotsOnTargetValues = 
            match matchStatisticsItem |> tryGetMatchStatisticsItem "Shots on target" with
            | Some shotsOnTarget -> shotsOnTarget.Values
            | None -> MatchStatisticsValuesEmpty

        return shotsOnTargetValues, shotsOffTarget.Values
    }

    let tryGetTvDataStatistics (allMatchStatisticsItems : Models.GroupMatchStatisticsItems list) = maybe {
        let! matchStatisticsItem = allMatchStatisticsItems |> tryGetGroupMatchStatisticsItems "TVData"
        let! cornerKicks = matchStatisticsItem |> tryGetMatchStatisticsItem "Corner kicks"

        let redCardsValues =
            match matchStatisticsItem |> tryGetMatchStatisticsItem "Red cards" with
            | Some redCards -> redCards.Values
            | None -> MatchStatisticsValuesEmpty

        return cornerKicks.Values, redCardsValues
    }

    let tryGetPossessionStatistics (allMatchStatisticsItems : Models.GroupMatchStatisticsItems list) = maybe {
        let! matchStatisticsItem = allMatchStatisticsItems |> tryGetGroupMatchStatisticsItems "Possession"
        let! ballPossession = matchStatisticsItem |> tryGetMatchStatisticsItem "Ball possession"

        return ballPossession.Values
    }
                
/// <summary>
/// MatchRatings
/// </summary>
type MatchRatings() =

    let mutable ratingDatas = list<RatingData>.Empty
    let mutable homeRating = 0us
    let mutable awayRating = 0us

    let calculateMatchRating() =
        homeRating <- 0us
        awayRating <- 0us

        ratingDatas
        |> List.iter (fun ratingData ->
                let homeRatingValue, awayRatingValue = RatingData.CalculatePossession ratingData

                homeRating <- homeRating + homeRatingValue
                awayRating <- awayRating + awayRatingValue
            )

        if ratingDatas.Length > 1
        then
            let homeRatingShotsOnOffTarget, awayRatingShotsOnOffTarget = RatingData.CalculateShotsOnOffTarget (ratingDatas |> List.head, ratingDatas |> List.last)

            homeRating <- homeRating + homeRatingShotsOnOffTarget
            awayRating <- awayRating + awayRatingShotsOnOffTarget

    member _this.HomeRating
        with get() = homeRating

    member _this.AwayRating
        with get() = awayRating

    member _this.AddRatingData(shotsOnTargetValues : Models.MatchStatisticsValues, shotsOffTargetValues : Models.MatchStatisticsValues, ballPossessionValues : Models.MatchStatisticsValues) =
        let ratingData =
            {
                ShotsOnTargetValues = shotsOnTargetValues
                ShotsOffTargetValues = shotsOffTargetValues
                BallPossessionValues = ballPossessionValues
            }
               
        let previousRatingDatas =
            if ratingDatas.Length = 10
            then
                ratingDatas.Tail
            else
                ratingDatas

        ratingDatas <- previousRatingDatas @ [ ratingData ]

        calculateMatchRating()

    override _this.ToString() =
        sprintf "%d - %d" homeRating awayRating

/// <summary>
/// MatchMatchStatisticsDataType
/// </summary>
type MatchMatchStatisticsDataType =
    | SofaScore
    | Performgroup
    | Unknown
    | NoMatchStatistics

/// <summary>
/// FootballMarket
/// </summary>
type FootballMarket(market : Market) as this =
    inherit ObservableObject()

    let footballMatch = toFootballMatch market

    let homeSelection = market.Selections.[0]
    let awaySelection = market.Selections.[1]
    let drawSelection = market.Selections.[2]

    let mutable matchMatchStatisticsDataType = MatchMatchStatisticsDataType.Unknown

    let mutable shotsOnTargetValues = nil<Models.MatchStatisticsValues>
    let mutable shotsOffTargetValues = nil<Models.MatchStatisticsValues>
    let mutable cornerKicksValues = nil<Models.MatchStatisticsValues>
    let mutable redCardsValues = nil<Models.MatchStatisticsValues>
    let mutable ballPossessionValues = nil<Models.MatchStatisticsValues>

    let matchRatings = MatchRatings()

    let toString (matchStatisticsValues : Models.MatchStatisticsValues) =
        if isNotNullObj matchStatisticsValues
        then
            matchStatisticsValues.ToString()
        else
            String.Empty

    let notifySelectionPropertyChanged name (selection : Selection) =
        if selection.IsUpdated
        then
            this.NotifyPropertyChanged name

    let notifyMatchStatisticsValuesPropertyChanged (matchStatisticsValues : Models.MatchStatisticsValues byref, newMatchStatisticsValues : Models.MatchStatisticsValues) name =
        let updated =
            if isNullObj matchStatisticsValues
            then
                true
            else
                matchStatisticsValues <> newMatchStatisticsValues

        if updated
        then
            matchStatisticsValues <- newMatchStatisticsValues
            this.NotifyPropertyChanged name
    
    member _this.Market
        with get() = market

    member _this.MarketInfo
        with get() = market.MarketInfo

    member _this.HomeSelection
        with get() = homeSelection

    member _this.AwaySelection
        with get() = awaySelection

    member _this.DrawSelection
        with get() = drawSelection

    member _this.FootballMatch
        with get() = footballMatch

    member _this.ShotsOnTarget
        with get() = toString shotsOnTargetValues

    member _this.ShotsOffTarget
        with get() = toString shotsOffTargetValues

    member _this.Corners
        with get() = toString cornerKicksValues

    member _this.Red
        with get() = toString redCardsValues

    member _this.Possession
        with get() = toString ballPossessionValues

    member _this.Rating
        with get() = matchRatings.ToString()

    member _this.HomeRating
        with get() = matchRatings.HomeRating

    member _this.AwayRating
        with get() = matchRatings.AwayRating

    member _this.MatchMatchStatisticsDataType
        with get() = matchMatchStatisticsDataType
        and set(value) = 
            matchMatchStatisticsDataType <- value
            this.NotifyPropertyChanged "HaveMatchStatistics"

    member _this.HaveMatchStatistics
        with get() = 
            match matchMatchStatisticsDataType with
            | MatchMatchStatisticsDataType.NoMatchStatistics -> false
            | _ -> true
        
    member _this.CanUpdatePrices
        with get() = canMonitorMarket market

    member _this.CanUpdateMatchData
        with get() = not (isClosedMarket market) && market.IsInPlay && footballMatch.IsInProgess

    member this.CanUpdateMatchStatistics
        with get() = this.HaveMatchStatistics && this.CanUpdateMatchData

    member this.OnPricesUpdated() =
        if market.IsUpdated
        then
            this.NotifyPropertyChanged "Market"

        homeSelection |> notifySelectionPropertyChanged "HomeSelection"
        awaySelection |> notifySelectionPropertyChanged "AwaySelection"
        drawSelection |> notifySelectionPropertyChanged "DrawSelection"

    member this.OnMatchDataUpdated() =
        if footballMatch.IsUpdated
        then
            this.NotifyPropertyChanged "FootballMatch"

    member _this.SetMatchStatistics(allMatchStatisticsItems : Models.GroupMatchStatisticsItems list) =
        tryGetShotsStatistics allMatchStatisticsItems
        |> Option.iter (fun (newShotsOnTargetValues, newShotsOffTargetValues) ->
                notifyMatchStatisticsValuesPropertyChanged (&shotsOnTargetValues, newShotsOnTargetValues) "ShotsOnTarget"
                notifyMatchStatisticsValuesPropertyChanged (&shotsOffTargetValues, newShotsOffTargetValues) "ShotsOffTarget"
            )

        tryGetTvDataStatistics allMatchStatisticsItems
        |> Option.iter (fun (newCornerKicksValues, newRedCardsValues) ->
                notifyMatchStatisticsValuesPropertyChanged (&cornerKicksValues, newCornerKicksValues) "Corners"
                notifyMatchStatisticsValuesPropertyChanged (&redCardsValues, newRedCardsValues) "Red"
            )

        tryGetPossessionStatistics allMatchStatisticsItems
        |> Option.iter (fun newBallPossessionValues ->
                notifyMatchStatisticsValuesPropertyChanged (&ballPossessionValues, newBallPossessionValues) "Possession"
            )

        if isNotNullObj shotsOnTargetValues && isNotNullObj shotsOffTargetValues && isNotNullObj ballPossessionValues
        then
            matchRatings.AddRatingData(shotsOnTargetValues, shotsOffTargetValues, ballPossessionValues)

            this.NotifyPropertyChanged "Rating"
            this.NotifyPropertyChanged "HomeRating"
            this.NotifyPropertyChanged "AwayRating"