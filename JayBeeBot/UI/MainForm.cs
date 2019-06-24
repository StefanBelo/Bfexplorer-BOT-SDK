// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Domain;
using BeloSoft.Bfexplorer.Service;
using JayBeeBot.Helpers;
using JayBeeBot.Models;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace JayBeeBot.UI
{
    /// <summary>
    /// MainForm
    /// </summary>
    public partial class MainForm : Form
    {
        private BfexplorerService bfexplorerService = new BfexplorerService();

        private BindingList<Race> races = new BindingList<Race>();
        private Race selectedRace;

        private BindingList<Selection> selections = new BindingList<Selection>();
        private MyBindingList<Bet> bets;

        private List<BotDescriptor> myBots = new List<BotDescriptor>();
        private BotDescriptor selectedBotToExecute;

        /// <summary>
        /// MainForm
        /// </summary>
        public MainForm()
        {
            // Test your bots in practice mode only, so no real bets are placed on betfair!
            // http://bfexplorer.net/Articles/ByTag?tag=Betfair%20BOT
            bfexplorerService.ServiceStatus.IsPracticeMode = true;

            InitializeComponent();

            menuItemPracticeMode.Checked = bfexplorerService.ServiceStatus.IsPracticeMode;
        }

        /// <summary>
        /// OnLoaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, EventArgs e)
        {
            bfexplorerService.UiApplication = new JayBeeBotHost(SynchronizationContext.Current);

            bets = new MyBindingList<Bet>(this);
            
            foreach (var botDescriptor in bfexplorerService.BotManager.MyBots)
            {
                int botId = botDescriptor.BotId.Id;
                
                if (botId <= 9 || botId == 22)
                {
                    botDescriptor.Parameters.Name = botDescriptor.BotId.Name;
                    myBots.Add(botDescriptor);
                }
            }
           
            bindingSourceRaces.DataSource = races;
            bindingSourceSelections.DataSource = selections;
            bindingSourceBets.DataSource = bets;

            listBoxBotsToExecute.DataSource = myBots;

            var outputMessages = new MyBindingList<OutputMessage>(this);
            outputMessages.Attach(bfexplorerService.ServiceStatus.OutputMessages);

            dataGridViewOutput.DataSource = new BindingSource { DataSource = outputMessages };
        }

        /// <summary>
        /// OnLoginClicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoginClicked(object sender, EventArgs e)
        {
            using (var dialog = new LoginForm(bfexplorerService))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    menuItemLogin.Enabled = false;

                    LoadRaces();

                    timer.Enabled = true;
                }
            }
        }

        /// <summary>
        /// OnPracticeModeClicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPracticeModeClicked(object sender, EventArgs e)
        {
            var serviceStatus = bfexplorerService.ServiceStatus;

            serviceStatus.IsPracticeMode = !serviceStatus.IsPracticeMode;
            menuItemPracticeMode.Checked = serviceStatus.IsPracticeMode;
        }

        /// <summary>
        /// LoadRaces
        /// </summary>
        private async void LoadRaces()
        {
            var serviceStatus = bfexplorerService.ServiceStatus;

            serviceStatus.OutputMessage("Starting to load races...", FSharpOption<string>.None);

            var filter = new List<BetEventFilterParameter>
            {
                BetEventFilterParameter.NewCountries(new string[] { "GB" } ),
                BetEventFilterParameter.NewBetEventTypeIds(new int[] { 7 }),
                BetEventFilterParameter.NewMarketTypeCodes(new string[] { "WIN" })
            };
            
            var marketCataloguesResult = await bfexplorerService.GetMarketCatalogues(filter.ToFSharpList(), FSharpOption<int>.Some(1000)).ExecuteAsyncTask();

            if (marketCataloguesResult.IsSuccessResult)
            {
                var marketIds = (from marketCatalogue in marketCataloguesResult.SuccessResult select marketCatalogue.MarketInfo.Id).ToArray();

                var marketsResult = await bfexplorerService.GetMarkets(marketIds, FSharpOption<bool>.None).ExecuteAsyncTask();

                if (marketsResult.IsSuccessResult)
                {
                    foreach (var market in marketsResult.SuccessResult)
                    {
                        races.Add(new Race(market));
                    }

                    serviceStatus.OutputMessage($"{races.Count} races has been loaded.", FSharpOption<string>.None);
                }
            }
            else
            {
                serviceStatus.OutputMessage("Failed to load today races.", FSharpOption<string>.None);
            }
        }

        /// <summary>
        /// OnExitClicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExitClicked(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// OnFormClosing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (bfexplorerService.ServiceStatus.IsAuthorized)
            {
                await bfexplorerService.Logout().ExecuteAsyncTask();
            }
        }

        /// <summary>
        /// OnRacesSelectionChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRacesSelectionChanged(object sender, EventArgs e)
        {
            var selectedRows = dataGridViewRaces.SelectedRows;
            Debug.Assert(selectedRows.Count > 0);

            PopulateRaceData(selectedRows[0].DataBoundItem as Race);
        }

        /// <summary>
        /// PopulateRaceData
        /// </summary>
        /// <param name="race"></param>
        private void PopulateRaceData(Race race)
        {
            if (selections.Count > 0)
            {
                selections.Clear();
            }

            foreach (var selection in race.Market.Selections)
            {
                if (selection.Status != SelectionStatus.Removed)
                {
                    selections.Add(selection);
                }
            }

            if (selectedRace != null)
            {
                bets.Dettach(selectedRace.Market.Bets);
            }

            bets.Attach(race.Market.Bets);

            labelMarket.Text = race.Market.MarketFullName;

            selectedRace = race;
        }

        /// <summary>
        /// OnTimerTick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTimerTick(object sender, EventArgs e)
        {
            if (selectedRace != null && selectedRace.Market.MarketStatus != MarketStatus.Closed)
            {
                await bfexplorerService.UpdateMarket(selectedRace.Market).ExecuteAsyncTask();
            }
        }

        /// <summary>
        /// OnBotsToExecutedSelectedChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBotsToExecutedSelectedChanged(object sender, EventArgs e)
        {
            selectedBotToExecute = listBoxBotsToExecute.SelectedItem as BotDescriptor;

            propertyGridBotParameters.SelectedObject = selectedBotToExecute.Parameters;
        }

        /// <summary>
        /// OnExecuteBotClicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExecuteBotClicked(object sender, EventArgs e)
        {
            if (selectedRace != null && selectedBotToExecute != null)
            {
                var selectedRows = dataGridViewSelections.SelectedRows;
                Debug.Assert(selectedRows.Count > 0);

                var selection = selectedRows[0].DataBoundItem as Selection;
                var myBotToExecute = new MyBotModel(selectedBotToExecute.BotId, selectedBotToExecute.Parameters);

                bfexplorerService.StartBot(selectedRace.Market, selection, myBotToExecute);
            }
        }

        /// <summary>
        /// OnStopAllBotsClicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStopAllBotsClicked(object sender, EventArgs e)
        {
            if (selectedRace != null)
            {
                var market = selectedRace.Market;

                if (market.RunningBots.Count > 0)
                {
                    bfexplorerService.StopBots(selectedRace.Market, FSharpOption<FSharpList<Bot>>.None);
                }
            }
        }
    }
}