namespace GeometryFriends.GameViewers
{
    partial class GeometryFriendsBatchSimulator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GeometryFriendsBatchSimulator));
            this.checkedListBoxImplementations = new System.Windows.Forms.CheckedListBox();
            this.groupBoxAgents = new System.Windows.Forms.GroupBox();
            this.groupBoxLevels = new System.Windows.Forms.GroupBox();
            this.treeViewWorldsLevels = new System.Windows.Forms.TreeView();
            this.groupBoxSimulation = new System.Windows.Forms.GroupBox();
            this.checkBoxLogToFile = new System.Windows.Forms.CheckBox();
            this.textBoxNumberSimulations = new System.Windows.Forms.TextBox();
            this.labelNumberSimulations = new System.Windows.Forms.Label();
            this.radioButtonTimeStampFile = new System.Windows.Forms.RadioButton();
            this.labelResults = new System.Windows.Forms.Label();
            this.radioButtonSingleFile = new System.Windows.Forms.RadioButton();
            this.textBoxSpeed = new System.Windows.Forms.TextBox();
            this.labelSpeed = new System.Windows.Forms.Label();
            this.checkBoxFixedTimestep = new System.Windows.Forms.CheckBox();
            this.checkBoxRendering = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.groupBoxControl = new System.Windows.Forms.GroupBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.labelProgressBarText = new System.Windows.Forms.Label();
            this.groupBoxResults = new System.Windows.Forms.GroupBox();
            this.textBoxCollectibleBonus = new System.Windows.Forms.TextBox();
            this.labelCollectibleBonus = new System.Windows.Forms.Label();
            this.textBoxCompletedBonus = new System.Windows.Forms.TextBox();
            this.labelCompletedBonus = new System.Windows.Forms.Label();
            this.checkBoxOvewriteResults = new System.Windows.Forms.CheckBox();
            this.textBoxResultsFilename = new System.Windows.Forms.TextBox();
            this.labelResultsFileName = new System.Windows.Forms.Label();
            this.checkBoxWriteResults = new System.Windows.Forms.CheckBox();
            this.toolTipBase = new System.Windows.Forms.ToolTip(this.components);
            this.groupBoxAgents.SuspendLayout();
            this.groupBoxLevels.SuspendLayout();
            this.groupBoxSimulation.SuspendLayout();
            this.groupBoxControl.SuspendLayout();
            this.groupBoxResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkedListBoxImplementations
            // 
            this.checkedListBoxImplementations.CheckOnClick = true;
            this.checkedListBoxImplementations.FormattingEnabled = true;
            this.checkedListBoxImplementations.Location = new System.Drawing.Point(6, 19);
            this.checkedListBoxImplementations.Name = "checkedListBoxImplementations";
            this.checkedListBoxImplementations.Size = new System.Drawing.Size(288, 214);
            this.checkedListBoxImplementations.TabIndex = 0;
            // 
            // groupBoxAgents
            // 
            this.groupBoxAgents.Controls.Add(this.checkedListBoxImplementations);
            this.groupBoxAgents.Location = new System.Drawing.Point(12, 12);
            this.groupBoxAgents.Name = "groupBoxAgents";
            this.groupBoxAgents.Size = new System.Drawing.Size(301, 248);
            this.groupBoxAgents.TabIndex = 6;
            this.groupBoxAgents.TabStop = false;
            this.groupBoxAgents.Text = "Agent Implementations";
            // 
            // groupBoxLevels
            // 
            this.groupBoxLevels.Controls.Add(this.treeViewWorldsLevels);
            this.groupBoxLevels.Location = new System.Drawing.Point(319, 12);
            this.groupBoxLevels.Name = "groupBoxLevels";
            this.groupBoxLevels.Size = new System.Drawing.Size(390, 248);
            this.groupBoxLevels.TabIndex = 7;
            this.groupBoxLevels.TabStop = false;
            this.groupBoxLevels.Text = "Worlds and Levels";
            // 
            // treeViewWorldsLevels
            // 
            this.treeViewWorldsLevels.CheckBoxes = true;
            this.treeViewWorldsLevels.Location = new System.Drawing.Point(6, 19);
            this.treeViewWorldsLevels.Name = "treeViewWorldsLevels";
            this.treeViewWorldsLevels.Size = new System.Drawing.Size(378, 214);
            this.treeViewWorldsLevels.TabIndex = 0;
            this.treeViewWorldsLevels.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewWorldsLevels_NodeMouseClick);
            // 
            // groupBoxSimulation
            // 
            this.groupBoxSimulation.Controls.Add(this.checkBoxLogToFile);
            this.groupBoxSimulation.Controls.Add(this.textBoxNumberSimulations);
            this.groupBoxSimulation.Controls.Add(this.labelNumberSimulations);
            this.groupBoxSimulation.Controls.Add(this.radioButtonTimeStampFile);
            this.groupBoxSimulation.Controls.Add(this.labelResults);
            this.groupBoxSimulation.Controls.Add(this.radioButtonSingleFile);
            this.groupBoxSimulation.Controls.Add(this.textBoxSpeed);
            this.groupBoxSimulation.Controls.Add(this.labelSpeed);
            this.groupBoxSimulation.Controls.Add(this.checkBoxFixedTimestep);
            this.groupBoxSimulation.Controls.Add(this.checkBoxRendering);
            this.groupBoxSimulation.Location = new System.Drawing.Point(12, 266);
            this.groupBoxSimulation.Name = "groupBoxSimulation";
            this.groupBoxSimulation.Size = new System.Drawing.Size(272, 143);
            this.groupBoxSimulation.TabIndex = 8;
            this.groupBoxSimulation.TabStop = false;
            this.groupBoxSimulation.Text = "Simulation Options";
            // 
            // checkBoxLogToFile
            // 
            this.checkBoxLogToFile.AutoSize = true;
            this.checkBoxLogToFile.Location = new System.Drawing.Point(197, 30);
            this.checkBoxLogToFile.Name = "checkBoxLogToFile";
            this.checkBoxLogToFile.Size = new System.Drawing.Size(75, 17);
            this.checkBoxLogToFile.TabIndex = 10;
            this.checkBoxLogToFile.Text = "Log to File";
            this.toolTipBase.SetToolTip(this.checkBoxLogToFile, "Writes all information logged in the game/agent using the GeometryFriends.Log cla" +
        "ss to \r\nthe Log.txt file that can be found under the Logs directory.");
            this.checkBoxLogToFile.UseVisualStyleBackColor = true;
            // 
            // textBoxNumberSimulations
            // 
            this.textBoxNumberSimulations.Location = new System.Drawing.Point(136, 114);
            this.textBoxNumberSimulations.Name = "textBoxNumberSimulations";
            this.textBoxNumberSimulations.Size = new System.Drawing.Size(100, 20);
            this.textBoxNumberSimulations.TabIndex = 9;
            this.textBoxNumberSimulations.Text = "1";
            this.textBoxNumberSimulations.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTipBase.SetToolTip(this.textBoxNumberSimulations, "The number of times that each level selected is run for each\r\nagent implementatio" +
        "n selected.");
            this.textBoxNumberSimulations.TextChanged += new System.EventHandler(this.textBoxNumberSimulations_TextChanged);
            // 
            // labelNumberSimulations
            // 
            this.labelNumberSimulations.AutoSize = true;
            this.labelNumberSimulations.Location = new System.Drawing.Point(15, 117);
            this.labelNumberSimulations.Name = "labelNumberSimulations";
            this.labelNumberSimulations.Size = new System.Drawing.Size(115, 13);
            this.labelNumberSimulations.TabIndex = 8;
            this.labelNumberSimulations.Text = "Number of Simulations:";
            // 
            // radioButtonTimeStampFile
            // 
            this.radioButtonTimeStampFile.AutoSize = true;
            this.radioButtonTimeStampFile.Location = new System.Drawing.Point(145, 90);
            this.radioButtonTimeStampFile.Name = "radioButtonTimeStampFile";
            this.radioButtonTimeStampFile.Size = new System.Drawing.Size(95, 17);
            this.radioButtonTimeStampFile.TabIndex = 7;
            this.radioButtonTimeStampFile.Text = "Timestamp File";
            this.toolTipBase.SetToolTip(this.radioButtonTimeStampFile, "When selected this option makes the batch simulator write the results\r\nof each le" +
        "vel simulated to a newly created and timestamped results file.");
            this.radioButtonTimeStampFile.UseVisualStyleBackColor = true;
            // 
            // labelResults
            // 
            this.labelResults.AutoSize = true;
            this.labelResults.Location = new System.Drawing.Point(15, 92);
            this.labelResults.Name = "labelResults";
            this.labelResults.Size = new System.Drawing.Size(45, 13);
            this.labelResults.TabIndex = 6;
            this.labelResults.Text = "Results:";
            // 
            // radioButtonSingleFile
            // 
            this.radioButtonSingleFile.AutoSize = true;
            this.radioButtonSingleFile.Checked = true;
            this.radioButtonSingleFile.Location = new System.Drawing.Point(66, 90);
            this.radioButtonSingleFile.Name = "radioButtonSingleFile";
            this.radioButtonSingleFile.Size = new System.Drawing.Size(73, 17);
            this.radioButtonSingleFile.TabIndex = 5;
            this.radioButtonSingleFile.TabStop = true;
            this.radioButtonSingleFile.Text = "Single File";
            this.toolTipBase.SetToolTip(this.radioButtonSingleFile, resources.GetString("radioButtonSingleFile.ToolTip"));
            this.radioButtonSingleFile.UseVisualStyleBackColor = true;
            // 
            // textBoxSpeed
            // 
            this.textBoxSpeed.Location = new System.Drawing.Point(62, 58);
            this.textBoxSpeed.Name = "textBoxSpeed";
            this.textBoxSpeed.Size = new System.Drawing.Size(100, 20);
            this.textBoxSpeed.TabIndex = 3;
            this.textBoxSpeed.Text = "1";
            this.textBoxSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTipBase.SetToolTip(this.textBoxSpeed, "Specifies the speed at which the game runs. For example 1 is normal speed \r\nand 2" +
        " means the game runs twice the normal speed.");
            this.textBoxSpeed.TextChanged += new System.EventHandler(this.textBoxSpeed_TextChanged);
            // 
            // labelSpeed
            // 
            this.labelSpeed.AutoSize = true;
            this.labelSpeed.Location = new System.Drawing.Point(15, 61);
            this.labelSpeed.Name = "labelSpeed";
            this.labelSpeed.Size = new System.Drawing.Size(41, 13);
            this.labelSpeed.TabIndex = 2;
            this.labelSpeed.Text = "Speed:";
            // 
            // checkBoxFixedTimestep
            // 
            this.checkBoxFixedTimestep.AutoSize = true;
            this.checkBoxFixedTimestep.Checked = true;
            this.checkBoxFixedTimestep.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxFixedTimestep.Location = new System.Drawing.Point(99, 30);
            this.checkBoxFixedTimestep.Name = "checkBoxFixedTimestep";
            this.checkBoxFixedTimestep.Size = new System.Drawing.Size(97, 17);
            this.checkBoxFixedTimestep.TabIndex = 1;
            this.checkBoxFixedTimestep.Text = "Fixed Timestep";
            this.toolTipBase.SetToolTip(this.checkBoxFixedTimestep, "Makes the game update using a target timestep between game updates.");
            this.checkBoxFixedTimestep.UseVisualStyleBackColor = true;
            // 
            // checkBoxRendering
            // 
            this.checkBoxRendering.AutoSize = true;
            this.checkBoxRendering.Location = new System.Drawing.Point(18, 30);
            this.checkBoxRendering.Name = "checkBoxRendering";
            this.checkBoxRendering.Size = new System.Drawing.Size(75, 17);
            this.checkBoxRendering.TabIndex = 0;
            this.checkBoxRendering.Text = "Rendering";
            this.toolTipBase.SetToolTip(this.checkBoxRendering, "\"Specifies whether the simulations should render the game, i.e. with a visual rep" +
        "resentation.\"");
            this.checkBoxRendering.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(164, 416);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(545, 23);
            this.progressBar1.TabIndex = 9;
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(8, 422);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(48, 13);
            this.labelProgress.TabIndex = 10;
            this.labelProgress.Text = "Progress";
            // 
            // groupBoxControl
            // 
            this.groupBoxControl.Controls.Add(this.buttonStop);
            this.groupBoxControl.Controls.Add(this.buttonStart);
            this.groupBoxControl.Location = new System.Drawing.Point(550, 266);
            this.groupBoxControl.Name = "groupBoxControl";
            this.groupBoxControl.Size = new System.Drawing.Size(159, 143);
            this.groupBoxControl.TabIndex = 11;
            this.groupBoxControl.TabStop = false;
            this.groupBoxControl.Text = "Simulator Controller";
            // 
            // buttonStop
            // 
            this.buttonStop.Enabled = false;
            this.buttonStop.Location = new System.Drawing.Point(6, 80);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(147, 54);
            this.buttonStop.TabIndex = 1;
            this.buttonStop.Text = "Stop All";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(6, 15);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(147, 59);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // labelProgressBarText
            // 
            this.labelProgressBarText.Location = new System.Drawing.Point(58, 419);
            this.labelProgressBarText.Name = "labelProgressBarText";
            this.labelProgressBarText.Size = new System.Drawing.Size(99, 18);
            this.labelProgressBarText.TabIndex = 12;
            this.labelProgressBarText.Text = "label1";
            this.labelProgressBarText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBoxResults
            // 
            this.groupBoxResults.Controls.Add(this.textBoxCollectibleBonus);
            this.groupBoxResults.Controls.Add(this.labelCollectibleBonus);
            this.groupBoxResults.Controls.Add(this.textBoxCompletedBonus);
            this.groupBoxResults.Controls.Add(this.labelCompletedBonus);
            this.groupBoxResults.Controls.Add(this.checkBoxOvewriteResults);
            this.groupBoxResults.Controls.Add(this.textBoxResultsFilename);
            this.groupBoxResults.Controls.Add(this.labelResultsFileName);
            this.groupBoxResults.Controls.Add(this.checkBoxWriteResults);
            this.groupBoxResults.Location = new System.Drawing.Point(290, 266);
            this.groupBoxResults.Name = "groupBoxResults";
            this.groupBoxResults.Size = new System.Drawing.Size(254, 143);
            this.groupBoxResults.TabIndex = 13;
            this.groupBoxResults.TabStop = false;
            this.groupBoxResults.Text = "Results";
            // 
            // textBoxCollectibleBonus
            // 
            this.textBoxCollectibleBonus.Location = new System.Drawing.Point(105, 109);
            this.textBoxCollectibleBonus.Name = "textBoxCollectibleBonus";
            this.textBoxCollectibleBonus.Size = new System.Drawing.Size(100, 20);
            this.textBoxCollectibleBonus.TabIndex = 7;
            this.textBoxCollectibleBonus.Text = "100";
            this.textBoxCollectibleBonus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTipBase.SetToolTip(this.textBoxCollectibleBonus, "The bonus given by each collectible caught by an agent to be assigned when calcul" +
        "ating an agent\'s level score.");
            this.textBoxCollectibleBonus.TextChanged += new System.EventHandler(this.textBoxCollectibleBonus_TextChanged);
            // 
            // labelCollectibleBonus
            // 
            this.labelCollectibleBonus.AutoSize = true;
            this.labelCollectibleBonus.Location = new System.Drawing.Point(6, 112);
            this.labelCollectibleBonus.Name = "labelCollectibleBonus";
            this.labelCollectibleBonus.Size = new System.Drawing.Size(91, 13);
            this.labelCollectibleBonus.TabIndex = 6;
            this.labelCollectibleBonus.Text = "Collectible Bonus:";
            // 
            // textBoxCompletedBonus
            // 
            this.textBoxCompletedBonus.Location = new System.Drawing.Point(105, 84);
            this.textBoxCompletedBonus.Name = "textBoxCompletedBonus";
            this.textBoxCompletedBonus.Size = new System.Drawing.Size(100, 20);
            this.textBoxCompletedBonus.TabIndex = 5;
            this.textBoxCompletedBonus.Text = "200";
            this.textBoxCompletedBonus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTipBase.SetToolTip(this.textBoxCompletedBonus, "The level completed bonus to be assigned when calculating an agent\'s level score." +
        "");
            this.textBoxCompletedBonus.TextChanged += new System.EventHandler(this.textBoxCompletedBonus_TextChanged);
            // 
            // labelCompletedBonus
            // 
            this.labelCompletedBonus.AutoSize = true;
            this.labelCompletedBonus.Location = new System.Drawing.Point(6, 87);
            this.labelCompletedBonus.Name = "labelCompletedBonus";
            this.labelCompletedBonus.Size = new System.Drawing.Size(93, 13);
            this.labelCompletedBonus.TabIndex = 4;
            this.labelCompletedBonus.Text = "Completed Bonus:";
            // 
            // checkBoxOvewriteResults
            // 
            this.checkBoxOvewriteResults.AutoSize = true;
            this.checkBoxOvewriteResults.Location = new System.Drawing.Point(126, 25);
            this.checkBoxOvewriteResults.Name = "checkBoxOvewriteResults";
            this.checkBoxOvewriteResults.Size = new System.Drawing.Size(128, 17);
            this.checkBoxOvewriteResults.TabIndex = 3;
            this.checkBoxOvewriteResults.Text = "Overwrite Results File";
            this.toolTipBase.SetToolTip(this.checkBoxOvewriteResults, "Specifies if the batch simulator results file should be overwritten \r\n(in case th" +
        "ere already is a previous one).");
            this.checkBoxOvewriteResults.UseVisualStyleBackColor = true;
            // 
            // textBoxResultsFilename
            // 
            this.textBoxResultsFilename.Location = new System.Drawing.Point(64, 53);
            this.textBoxResultsFilename.Name = "textBoxResultsFilename";
            this.textBoxResultsFilename.Size = new System.Drawing.Size(184, 20);
            this.textBoxResultsFilename.TabIndex = 2;
            this.toolTipBase.SetToolTip(this.textBoxResultsFilename, "The name of the batch simulator results file.");
            // 
            // labelResultsFileName
            // 
            this.labelResultsFileName.AutoSize = true;
            this.labelResultsFileName.Location = new System.Drawing.Point(6, 56);
            this.labelResultsFileName.Name = "labelResultsFileName";
            this.labelResultsFileName.Size = new System.Drawing.Size(52, 13);
            this.labelResultsFileName.TabIndex = 1;
            this.labelResultsFileName.Text = "Filename:";
            // 
            // checkBoxWriteResults
            // 
            this.checkBoxWriteResults.AutoSize = true;
            this.checkBoxWriteResults.Checked = true;
            this.checkBoxWriteResults.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWriteResults.Location = new System.Drawing.Point(6, 25);
            this.checkBoxWriteResults.Name = "checkBoxWriteResults";
            this.checkBoxWriteResults.Size = new System.Drawing.Size(120, 17);
            this.checkBoxWriteResults.TabIndex = 0;
            this.checkBoxWriteResults.Text = "Write Batch Results";
            this.toolTipBase.SetToolTip(this.checkBoxWriteResults, "Writes the overall results of a batch simulation to a results file\r\nto be found u" +
        "nder the Results directory. This results file is \r\nshows performance of the agen" +
        "ts as in a competition.");
            this.checkBoxWriteResults.UseVisualStyleBackColor = true;
            // 
            // GeometryFriendsBatchSimulator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 451);
            this.Controls.Add(this.groupBoxResults);
            this.Controls.Add(this.labelProgressBarText);
            this.Controls.Add(this.groupBoxControl);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBoxSimulation);
            this.Controls.Add(this.groupBoxLevels);
            this.Controls.Add(this.groupBoxAgents);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "GeometryFriendsBatchSimulator";
            this.Text = "Geometry Friends Batch Simulator";
            this.groupBoxAgents.ResumeLayout(false);
            this.groupBoxLevels.ResumeLayout(false);
            this.groupBoxSimulation.ResumeLayout(false);
            this.groupBoxSimulation.PerformLayout();
            this.groupBoxControl.ResumeLayout(false);
            this.groupBoxResults.ResumeLayout(false);
            this.groupBoxResults.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBoxImplementations;
        private System.Windows.Forms.GroupBox groupBoxAgents;
        private System.Windows.Forms.GroupBox groupBoxLevels;
        private System.Windows.Forms.GroupBox groupBoxSimulation;
        private System.Windows.Forms.CheckBox checkBoxFixedTimestep;
        private System.Windows.Forms.CheckBox checkBoxRendering;
        private System.Windows.Forms.RadioButton radioButtonTimeStampFile;
        private System.Windows.Forms.Label labelResults;
        private System.Windows.Forms.RadioButton radioButtonSingleFile;
        private System.Windows.Forms.TextBox textBoxSpeed;
        private System.Windows.Forms.Label labelSpeed;
        private System.Windows.Forms.TextBox textBoxNumberSimulations;
        private System.Windows.Forms.Label labelNumberSimulations;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.GroupBox groupBoxControl;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.TreeView treeViewWorldsLevels;
        private System.Windows.Forms.CheckBox checkBoxLogToFile;
        private System.Windows.Forms.Label labelProgressBarText;
        private System.Windows.Forms.GroupBox groupBoxResults;
        private System.Windows.Forms.CheckBox checkBoxOvewriteResults;
        private System.Windows.Forms.TextBox textBoxResultsFilename;
        private System.Windows.Forms.Label labelResultsFileName;
        private System.Windows.Forms.CheckBox checkBoxWriteResults;
        private System.Windows.Forms.TextBox textBoxCollectibleBonus;
        private System.Windows.Forms.Label labelCollectibleBonus;
        private System.Windows.Forms.TextBox textBoxCompletedBonus;
        private System.Windows.Forms.Label labelCompletedBonus;
        private System.Windows.Forms.ToolTip toolTipBase;
    }
}