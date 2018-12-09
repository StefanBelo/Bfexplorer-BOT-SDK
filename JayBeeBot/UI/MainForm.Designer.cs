namespace JayBeeBot.UI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.applicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemLogin = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemPracticeMode = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewOutput = new System.Windows.Forms.DataGridView();
            this.outputTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.outputMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.propertyGridBotParameters = new System.Windows.Forms.PropertyGrid();
            this.labelOutput = new System.Windows.Forms.Label();
            this.dataGridViewRaces = new System.Windows.Forms.DataGridView();
            this.timeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.racecourseDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.raceInfoDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bindingSourceRaces = new System.Windows.Forms.BindingSource(this.components);
            this.labelRaces = new System.Windows.Forms.Label();
            this.labelMarket = new System.Windows.Forms.Label();
            this.dataGridViewSelections = new System.Windows.Forms.DataGridView();
            this.bindingSourceSelections = new System.Windows.Forms.BindingSource(this.components);
            this.dataGridViewBets = new System.Windows.Forms.DataGridView();
            this.bindingSourceBets = new System.Windows.Forms.BindingSource(this.components);
            this.labelMarketBets = new System.Windows.Forms.Label();
            this.buttonBotsStopAllBots = new System.Windows.Forms.Button();
            this.buttonExecuteBot = new System.Windows.Forms.Button();
            this.labelBotsToExecute = new System.Windows.Forms.Label();
            this.listBoxBotsToExecute = new System.Windows.Forms.ListBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastPriceTradedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.totalMatchedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.profitBalanceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.selectionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.betTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.priceDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.orderStatusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRaces)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceRaces)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSelections)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSelections)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceBets)).BeginInit();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applicationToolStripMenuItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(1329, 24);
            this.mainMenu.TabIndex = 0;
            this.mainMenu.Text = "menuStrip1";
            // 
            // applicationToolStripMenuItem
            // 
            this.applicationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemLogin,
            this.menuItemPracticeMode,
            this.menuItemExit});
            this.applicationToolStripMenuItem.Name = "applicationToolStripMenuItem";
            this.applicationToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.applicationToolStripMenuItem.Text = "Application";
            // 
            // menuItemLogin
            // 
            this.menuItemLogin.Name = "menuItemLogin";
            this.menuItemLogin.Size = new System.Drawing.Size(150, 22);
            this.menuItemLogin.Text = "Login";
            this.menuItemLogin.Click += new System.EventHandler(this.OnLoginClicked);
            // 
            // menuItemPracticeMode
            // 
            this.menuItemPracticeMode.Name = "menuItemPracticeMode";
            this.menuItemPracticeMode.Size = new System.Drawing.Size(150, 22);
            this.menuItemPracticeMode.Text = "Practice Mode";
            this.menuItemPracticeMode.Click += new System.EventHandler(this.OnPracticeModeClicked);
            // 
            // menuItemExit
            // 
            this.menuItemExit.Name = "menuItemExit";
            this.menuItemExit.Size = new System.Drawing.Size(150, 22);
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new System.EventHandler(this.OnExitClicked);
            // 
            // dataGridViewOutput
            // 
            this.dataGridViewOutput.AllowUserToAddRows = false;
            this.dataGridViewOutput.AllowUserToDeleteRows = false;
            this.dataGridViewOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewOutput.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOutput.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.outputTimeColumn,
            this.outputMessageColumn});
            this.dataGridViewOutput.Location = new System.Drawing.Point(12, 616);
            this.dataGridViewOutput.MultiSelect = false;
            this.dataGridViewOutput.Name = "dataGridViewOutput";
            this.dataGridViewOutput.ReadOnly = true;
            this.dataGridViewOutput.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewOutput.Size = new System.Drawing.Size(1305, 133);
            this.dataGridViewOutput.TabIndex = 1;
            // 
            // outputTimeColumn
            // 
            this.outputTimeColumn.DataPropertyName = "Time";
            this.outputTimeColumn.HeaderText = "Time";
            this.outputTimeColumn.Name = "outputTimeColumn";
            this.outputTimeColumn.ReadOnly = true;
            // 
            // outputMessageColumn
            // 
            this.outputMessageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.outputMessageColumn.DataPropertyName = "Message";
            this.outputMessageColumn.HeaderText = "Message";
            this.outputMessageColumn.Name = "outputMessageColumn";
            this.outputMessageColumn.ReadOnly = true;
            // 
            // propertyGridBotParameters
            // 
            this.propertyGridBotParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridBotParameters.Location = new System.Drawing.Point(950, 222);
            this.propertyGridBotParameters.Name = "propertyGridBotParameters";
            this.propertyGridBotParameters.Size = new System.Drawing.Size(367, 346);
            this.propertyGridBotParameters.TabIndex = 2;
            // 
            // labelOutput
            // 
            this.labelOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(12, 600);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(39, 13);
            this.labelOutput.TabIndex = 3;
            this.labelOutput.Text = "Output";
            // 
            // dataGridViewRaces
            // 
            this.dataGridViewRaces.AllowUserToAddRows = false;
            this.dataGridViewRaces.AllowUserToDeleteRows = false;
            this.dataGridViewRaces.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.dataGridViewRaces.AutoGenerateColumns = false;
            this.dataGridViewRaces.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewRaces.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewRaces.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.timeDataGridViewTextBoxColumn,
            this.racecourseDataGridViewTextBoxColumn,
            this.raceInfoDataGridViewTextBoxColumn});
            this.dataGridViewRaces.DataSource = this.bindingSourceRaces;
            this.dataGridViewRaces.Location = new System.Drawing.Point(12, 56);
            this.dataGridViewRaces.MultiSelect = false;
            this.dataGridViewRaces.Name = "dataGridViewRaces";
            this.dataGridViewRaces.ReadOnly = true;
            this.dataGridViewRaces.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewRaces.Size = new System.Drawing.Size(357, 541);
            this.dataGridViewRaces.TabIndex = 4;
            this.dataGridViewRaces.SelectionChanged += new System.EventHandler(this.OnRacesSelectionChanged);
            // 
            // timeDataGridViewTextBoxColumn
            // 
            this.timeDataGridViewTextBoxColumn.DataPropertyName = "Time";
            this.timeDataGridViewTextBoxColumn.HeaderText = "Time";
            this.timeDataGridViewTextBoxColumn.Name = "timeDataGridViewTextBoxColumn";
            this.timeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // racecourseDataGridViewTextBoxColumn
            // 
            this.racecourseDataGridViewTextBoxColumn.DataPropertyName = "Racecourse";
            this.racecourseDataGridViewTextBoxColumn.HeaderText = "Racecourse";
            this.racecourseDataGridViewTextBoxColumn.Name = "racecourseDataGridViewTextBoxColumn";
            this.racecourseDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // raceInfoDataGridViewTextBoxColumn
            // 
            this.raceInfoDataGridViewTextBoxColumn.DataPropertyName = "RaceInfo";
            this.raceInfoDataGridViewTextBoxColumn.HeaderText = "RaceInfo";
            this.raceInfoDataGridViewTextBoxColumn.Name = "raceInfoDataGridViewTextBoxColumn";
            this.raceInfoDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // bindingSourceRaces
            // 
            this.bindingSourceRaces.DataSource = typeof(JayBeeBot.Models.Race);
            // 
            // labelRaces
            // 
            this.labelRaces.AutoSize = true;
            this.labelRaces.Location = new System.Drawing.Point(12, 37);
            this.labelRaces.Name = "labelRaces";
            this.labelRaces.Size = new System.Drawing.Size(38, 13);
            this.labelRaces.TabIndex = 5;
            this.labelRaces.Text = "Races";
            // 
            // labelMarket
            // 
            this.labelMarket.AutoSize = true;
            this.labelMarket.Location = new System.Drawing.Point(384, 37);
            this.labelMarket.Name = "labelMarket";
            this.labelMarket.Size = new System.Drawing.Size(40, 13);
            this.labelMarket.TabIndex = 6;
            this.labelMarket.Text = "Market";
            // 
            // dataGridViewSelections
            // 
            this.dataGridViewSelections.AllowUserToAddRows = false;
            this.dataGridViewSelections.AllowUserToDeleteRows = false;
            this.dataGridViewSelections.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewSelections.AutoGenerateColumns = false;
            this.dataGridViewSelections.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewSelections.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewSelections.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.statusDataGridViewTextBoxColumn,
            this.lastPriceTradedDataGridViewTextBoxColumn,
            this.totalMatchedDataGridViewTextBoxColumn,
            this.profitBalanceDataGridViewTextBoxColumn});
            this.dataGridViewSelections.DataSource = this.bindingSourceSelections;
            this.dataGridViewSelections.Location = new System.Drawing.Point(387, 56);
            this.dataGridViewSelections.MultiSelect = false;
            this.dataGridViewSelections.Name = "dataGridViewSelections";
            this.dataGridViewSelections.ReadOnly = true;
            this.dataGridViewSelections.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewSelections.Size = new System.Drawing.Size(557, 329);
            this.dataGridViewSelections.TabIndex = 7;
            // 
            // bindingSourceSelections
            // 
            this.bindingSourceSelections.DataSource = typeof(BeloSoft.Bfexplorer.Domain.Selection);
            // 
            // dataGridViewBets
            // 
            this.dataGridViewBets.AllowUserToAddRows = false;
            this.dataGridViewBets.AllowUserToDeleteRows = false;
            this.dataGridViewBets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewBets.AutoGenerateColumns = false;
            this.dataGridViewBets.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewBets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewBets.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.idDataGridViewTextBoxColumn,
            this.selectionDataGridViewTextBoxColumn,
            this.betTypeDataGridViewTextBoxColumn,
            this.priceDataGridViewTextBoxColumn,
            this.sizeDataGridViewTextBoxColumn,
            this.orderStatusDataGridViewTextBoxColumn});
            this.dataGridViewBets.DataSource = this.bindingSourceBets;
            this.dataGridViewBets.Location = new System.Drawing.Point(387, 404);
            this.dataGridViewBets.MultiSelect = false;
            this.dataGridViewBets.Name = "dataGridViewBets";
            this.dataGridViewBets.ReadOnly = true;
            this.dataGridViewBets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewBets.Size = new System.Drawing.Size(557, 193);
            this.dataGridViewBets.TabIndex = 8;
            // 
            // bindingSourceBets
            // 
            this.bindingSourceBets.DataSource = typeof(BeloSoft.Bfexplorer.Domain.Bet);
            // 
            // labelMarketBets
            // 
            this.labelMarketBets.AutoSize = true;
            this.labelMarketBets.Location = new System.Drawing.Point(384, 388);
            this.labelMarketBets.Name = "labelMarketBets";
            this.labelMarketBets.Size = new System.Drawing.Size(64, 13);
            this.labelMarketBets.TabIndex = 9;
            this.labelMarketBets.Text = "Market Bets";
            // 
            // buttonBotsStopAllBots
            // 
            this.buttonBotsStopAllBots.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBotsStopAllBots.Location = new System.Drawing.Point(1242, 574);
            this.buttonBotsStopAllBots.Name = "buttonBotsStopAllBots";
            this.buttonBotsStopAllBots.Size = new System.Drawing.Size(75, 23);
            this.buttonBotsStopAllBots.TabIndex = 10;
            this.buttonBotsStopAllBots.Text = "Stop All Bots";
            this.buttonBotsStopAllBots.UseVisualStyleBackColor = true;
            this.buttonBotsStopAllBots.Click += new System.EventHandler(this.OnStopAllBotsClicked);
            // 
            // buttonExecuteBot
            // 
            this.buttonExecuteBot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExecuteBot.Location = new System.Drawing.Point(1150, 574);
            this.buttonExecuteBot.Name = "buttonExecuteBot";
            this.buttonExecuteBot.Size = new System.Drawing.Size(86, 23);
            this.buttonExecuteBot.TabIndex = 11;
            this.buttonExecuteBot.Text = "Execute Bot";
            this.buttonExecuteBot.UseVisualStyleBackColor = true;
            this.buttonExecuteBot.Click += new System.EventHandler(this.OnExecuteBotClicked);
            // 
            // labelBotsToExecute
            // 
            this.labelBotsToExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBotsToExecute.AutoSize = true;
            this.labelBotsToExecute.Location = new System.Drawing.Point(947, 37);
            this.labelBotsToExecute.Name = "labelBotsToExecute";
            this.labelBotsToExecute.Size = new System.Drawing.Size(82, 13);
            this.labelBotsToExecute.TabIndex = 12;
            this.labelBotsToExecute.Text = "Bots to Execute";
            // 
            // listBoxBotsToExecute
            // 
            this.listBoxBotsToExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxBotsToExecute.FormattingEnabled = true;
            this.listBoxBotsToExecute.Location = new System.Drawing.Point(950, 56);
            this.listBoxBotsToExecute.Name = "listBoxBotsToExecute";
            this.listBoxBotsToExecute.Size = new System.Drawing.Size(367, 160);
            this.listBoxBotsToExecute.TabIndex = 13;
            this.listBoxBotsToExecute.SelectedIndexChanged += new System.EventHandler(this.OnBotsToExecutedSelectedChanged);
            // 
            // timer
            // 
            this.timer.Interval = 200;
            this.timer.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // statusDataGridViewTextBoxColumn
            // 
            this.statusDataGridViewTextBoxColumn.DataPropertyName = "Status";
            this.statusDataGridViewTextBoxColumn.HeaderText = "Status";
            this.statusDataGridViewTextBoxColumn.Name = "statusDataGridViewTextBoxColumn";
            this.statusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // lastPriceTradedDataGridViewTextBoxColumn
            // 
            this.lastPriceTradedDataGridViewTextBoxColumn.DataPropertyName = "LastPriceTraded";
            this.lastPriceTradedDataGridViewTextBoxColumn.HeaderText = "LastPriceTraded";
            this.lastPriceTradedDataGridViewTextBoxColumn.Name = "lastPriceTradedDataGridViewTextBoxColumn";
            this.lastPriceTradedDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // totalMatchedDataGridViewTextBoxColumn
            // 
            this.totalMatchedDataGridViewTextBoxColumn.DataPropertyName = "TotalMatched";
            dataGridViewCellStyle1.Format = "N2";
            this.totalMatchedDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.totalMatchedDataGridViewTextBoxColumn.HeaderText = "TotalMatched";
            this.totalMatchedDataGridViewTextBoxColumn.Name = "totalMatchedDataGridViewTextBoxColumn";
            this.totalMatchedDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // profitBalanceDataGridViewTextBoxColumn
            // 
            this.profitBalanceDataGridViewTextBoxColumn.DataPropertyName = "ProfitBalance";
            dataGridViewCellStyle2.Format = "N2";
            this.profitBalanceDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.profitBalanceDataGridViewTextBoxColumn.HeaderText = "ProfitBalance";
            this.profitBalanceDataGridViewTextBoxColumn.Name = "profitBalanceDataGridViewTextBoxColumn";
            this.profitBalanceDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            this.idDataGridViewTextBoxColumn.HeaderText = "Id";
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            this.idDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // selectionDataGridViewTextBoxColumn
            // 
            this.selectionDataGridViewTextBoxColumn.DataPropertyName = "Selection";
            this.selectionDataGridViewTextBoxColumn.HeaderText = "Selection";
            this.selectionDataGridViewTextBoxColumn.Name = "selectionDataGridViewTextBoxColumn";
            this.selectionDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // betTypeDataGridViewTextBoxColumn
            // 
            this.betTypeDataGridViewTextBoxColumn.DataPropertyName = "BetType";
            this.betTypeDataGridViewTextBoxColumn.HeaderText = "BetType";
            this.betTypeDataGridViewTextBoxColumn.Name = "betTypeDataGridViewTextBoxColumn";
            this.betTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // priceDataGridViewTextBoxColumn
            // 
            this.priceDataGridViewTextBoxColumn.DataPropertyName = "Price";
            dataGridViewCellStyle3.Format = "N3";
            this.priceDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle3;
            this.priceDataGridViewTextBoxColumn.HeaderText = "Price";
            this.priceDataGridViewTextBoxColumn.Name = "priceDataGridViewTextBoxColumn";
            this.priceDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // sizeDataGridViewTextBoxColumn
            // 
            this.sizeDataGridViewTextBoxColumn.DataPropertyName = "Size";
            dataGridViewCellStyle4.Format = "N2";
            this.sizeDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle4;
            this.sizeDataGridViewTextBoxColumn.HeaderText = "Size";
            this.sizeDataGridViewTextBoxColumn.Name = "sizeDataGridViewTextBoxColumn";
            this.sizeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // orderStatusDataGridViewTextBoxColumn
            // 
            this.orderStatusDataGridViewTextBoxColumn.DataPropertyName = "OrderStatus";
            this.orderStatusDataGridViewTextBoxColumn.HeaderText = "OrderStatus";
            this.orderStatusDataGridViewTextBoxColumn.Name = "orderStatusDataGridViewTextBoxColumn";
            this.orderStatusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1329, 761);
            this.Controls.Add(this.listBoxBotsToExecute);
            this.Controls.Add(this.labelBotsToExecute);
            this.Controls.Add(this.buttonExecuteBot);
            this.Controls.Add(this.buttonBotsStopAllBots);
            this.Controls.Add(this.labelMarketBets);
            this.Controls.Add(this.dataGridViewBets);
            this.Controls.Add(this.dataGridViewSelections);
            this.Controls.Add(this.labelMarket);
            this.Controls.Add(this.labelRaces);
            this.Controls.Add(this.dataGridViewRaces);
            this.Controls.Add(this.labelOutput);
            this.Controls.Add(this.propertyGridBotParameters);
            this.Controls.Add(this.dataGridViewOutput);
            this.Controls.Add(this.mainMenu);
            this.MainMenuStrip = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "MainForm";
            this.Text = "JayBEE Bot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
            this.Load += new System.EventHandler(this.OnLoaded);
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewRaces)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceRaces)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSelections)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceSelections)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewBets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceBets)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem applicationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItemLogin;
        private System.Windows.Forms.ToolStripMenuItem menuItemPracticeMode;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
        private System.Windows.Forms.DataGridView dataGridViewOutput;
        private System.Windows.Forms.PropertyGrid propertyGridBotParameters;
        private System.Windows.Forms.DataGridViewTextBoxColumn outputTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn outputMessageColumn;
        private System.Windows.Forms.Label labelOutput;
        private System.Windows.Forms.DataGridView dataGridViewRaces;
        private System.Windows.Forms.Label labelRaces;
        private System.Windows.Forms.BindingSource bindingSourceRaces;
        private System.Windows.Forms.DataGridViewTextBoxColumn timeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn racecourseDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn raceInfoDataGridViewTextBoxColumn;
        private System.Windows.Forms.Label labelMarket;
        private System.Windows.Forms.DataGridView dataGridViewSelections;
        private System.Windows.Forms.DataGridView dataGridViewBets;
        private System.Windows.Forms.Label labelMarketBets;
        private System.Windows.Forms.Button buttonBotsStopAllBots;
        private System.Windows.Forms.Button buttonExecuteBot;
        private System.Windows.Forms.Label labelBotsToExecute;
        private System.Windows.Forms.ListBox listBoxBotsToExecute;
        private System.Windows.Forms.BindingSource bindingSourceSelections;
        private System.Windows.Forms.BindingSource bindingSourceBets;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastPriceTradedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn totalMatchedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn profitBalanceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn selectionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn betTypeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn priceDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn sizeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn orderStatusDataGridViewTextBoxColumn;
    }
}

