// Bfexplorer cannot be held responsible for any losses or damages incurred during the use of this betfair bot.
// It is up to you to determine the level of risk you wish to trade under. 
// Do not gamble with money you cannot afford to lose.

using BeloSoft.Bfexplorer.Service;
using JayBeeBot.Helpers;
using Microsoft.FSharp.Core;
using System;
using System.Windows.Forms;

namespace JayBeeBot.UI
{
    /// <summary>
    /// LoginForm
    /// </summary>
    public partial class LoginForm : Form
    {
        private BfexplorerService bfexplorerService;

        /// <summary>
        /// LoginForm
        /// </summary>
        /// <param name="bfexplorerService"></param>
        public LoginForm(BfexplorerService bfexplorerService)
        {
            this.bfexplorerService = bfexplorerService;

            InitializeComponent();
        }

        /// <summary>
        /// OnLoginClicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            buttonLogin.Enabled = false;

            var result = await bfexplorerService.Login(textBoxUserName.Text, textBoxPassword.Text).ExecuteAsyncTask();

            if (result.IsSuccessResult)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show(result.FailureResult.Item3);

                buttonLogin.Enabled = true;
            }
        }
    }
}