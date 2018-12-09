// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Domain;
using System;

namespace JayBeeBot.Models
{
    /// <summary>
    /// Race
    /// </summary>
    public class Race
    {
        private Market market;

        /// <summary>
        /// Race
        /// </summary>
        /// <param name="market"></param>
        public Race(Market market)
        {
            this.market = market;
        }

        public Market Market
        {
            get { return market; }
        }

        public DateTime Time
        {
            get { return market.MarketInfo.StartTime; }
        }

        public string Racecourse
        {
            get { return market.MarketInfo.BetEvent.Details; }
        }

        public string RaceInfo
        {
            get { return market.MarketInfo.MarketName; }
        }
    }
}