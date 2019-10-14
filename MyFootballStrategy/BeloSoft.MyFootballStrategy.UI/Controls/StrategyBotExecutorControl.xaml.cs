/////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2015 - 2016, Stefan Belopotocan, http://bfexplorer.net
//
/////////////////////////////////////////////////////////////////////////////

using BeloSoft.Bfexplorer.Domain;
using DevExpress.Xpf.Editors;
using System;
using System.Windows.Controls;

namespace BeloSoft.MyFootballStrategy.UI.Controls
{
    /// <summary>
    /// StrategyBotExecutorControl
    /// </summary>
    public partial class StrategyBotExecutorControl : UserControl
    {
        // Data
        private readonly OddsContext oddsContext = new OddsContext();

        /// <summary>
        /// StrategyBotExecutorControl
        /// </summary>
        public StrategyBotExecutorControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnOddsEditValueChanging
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OnOddsEditValueChanging(object sender, EditValueChangingEventArgs eventArgs)
        {
            var tePrice = sender as SpinEdit;

            double newOdds = Convert.ToDouble(eventArgs.NewValue);
            double odds = oddsContext.GetValidOdds(newOdds);

            tePrice.Increment = (decimal)oddsContext.OddsRange.Increment;

            if (odds != newOdds)
            {
                eventArgs.IsCancel = true;
                tePrice.EditValue = (decimal)odds;
            }
        }
    }
}
