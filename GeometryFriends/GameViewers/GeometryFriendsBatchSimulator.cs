using GeometryFriends.AI;
using GeometryFriends.Levels;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System;
using System.Drawing;
using GeometryFriends.Levels.Shared;
using Project = GeometryFriends.Levels.XMLLevelParser.LevelInformation;


namespace GeometryFriends.GameViewers
{
    public partial class GeometryFriendsBatchSimulator : Form
    {
        int speed, simulations, simulated, completedBonus, collectibleBonus;
        int completedBonusDefault, collectibleBonusDefault;
        Dictionary<string, string> agentImplementationsPath;
        List<string> selectedAgentImplementations;
        string[] quickStartupOptionsTemplate;
        Thread simulator;
        //simulation results specific
        //[agentImplementation][world][level][Result]
        Dictionary<string, Dictionary<string, Dictionary<int, List<LevelResult>>>> batchResults;
        string currentAgentImplementation;
        string currentWorld;
        int currentLevel;
        bool listenToResultsRegistered;
        char csvSeparator = SinglePlayerLevel.CSV_SEPERATOR;
        string defaultBatchResultsFile;

        public GeometryFriendsBatchSimulator()
        {
            InitializeComponent();
            LoadAgentImplementations();
            LoadWorldsAndLevels();

            speed = 1;
            simulations = 1;
            selectedAgentImplementations = new List<string>();
            simulator = null;
            listenToResultsRegistered = false;

            labelProgressBarText.Text = "";
            defaultBatchResultsFile = textBoxResultsFilename.Text = string.Format("{0:yyyy-MM-dd}", DateTime.Now) + "_Competition";

            completedBonus = completedBonusDefault = int.Parse(textBoxCompletedBonus.Text);
            collectibleBonus = collectibleBonusDefault = int.Parse(textBoxCollectibleBonus.Text);            
        }

        private void LoadAgentImplementations()
        {
            agentImplementationsPath = AgentsManager.GetAgentImplementationsPathMapping();
            checkedListBoxImplementations.Items.AddRange(agentImplementationsPath.Keys.ToArray());
        }

        private void LoadWorldsAndLevels()
        {
            DirectoryInfo levelsFolder = new DirectoryInfo(XMLLevelParser.LEVELS_FOLDER);
            
            TreeNode newNode;            
            foreach (KeyValuePair<string, List<XMLLevelParser.LevelInformation>> item in XMLLevelParser.GetAllWorldsLevelsStructure())
            {
                newNode = new TreeNode(item.Key, GenerateLevelNodes(item.Value));
                treeViewWorldsLevels.Nodes.Add(newNode);
            }
        }

        private TreeNode[] GenerateLevelNodes(List<XMLLevelParser.LevelInformation> levelsInformation)
        {
            TreeNode[] levels = new TreeNode[levelsInformation.Count];
            string timeLimit;
            string level;
            string nodeText;
            XMLLevelParser.LevelInformation levelInfo;
            for (int i = 0; i < levelsInformation.Count; i++)
            {
                levelInfo = levelsInformation[i];
                if(double.IsInfinity(levelInfo.timeLimit)){
                    timeLimit = "+infinity";
                }
                else{
                    timeLimit = string.Empty + Convert.ToInt32(levelInfo.timeLimit);
                }
                level = string.Empty + (i + 1);
                nodeText = "Level " + level;
                for(int j = 0; j < 15 - level.Length * 2; j++){
                    nodeText += " ";                    
                }
                nodeText += "(Time Limit: " + timeLimit + " )";
                levels[i] = new TreeNode(nodeText);
            }
            return levels;
        }

        private void treeViewWorldsLevels_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            bool isChecked = node.Checked;

            var localPosition = treeViewWorldsLevels.PointToClient(Cursor.Position);
            var hitTestInfo = treeViewWorldsLevels.HitTest(localPosition);
            if (hitTestInfo.Location == TreeViewHitTestLocations.StateImage)
            {
                //clicked checkbox
                //world
                if (node.Level == 0)
                {
                    //check that all the underlying levels have the same checked state
                    foreach (TreeNode item in node.Nodes)
                    {
                        if (item.Checked != isChecked)
                        {
                            item.Checked = isChecked;
                        }
                    }
                }
                //level 
                else if (node.Level == 1)
                {
                    //check if parent is checked accordingly
                    if (isChecked && !node.Parent.Checked)
                    {
                        node.Parent.Checked = true;
                    }
                    else if (!isChecked && node.Parent.Checked)
                    {
                        foreach (TreeNode item in node.Parent.Nodes)
                        {
                            if (item.Checked)
                                return;
                        }
                        node.Parent.Checked = false;
                    }
                }
            }
        }

        private void textBoxSpeed_TextChanged(object sender, System.EventArgs e)
        {
            ValidateTextBoxPositiveValue(textBoxSpeed, ref speed);
        }

        private void textBoxNumberSimulations_TextChanged(object sender, System.EventArgs e)
        {
            ValidateTextBoxPositiveValue(textBoxNumberSimulations, ref simulations);
        }

        private void ValidateTextBoxPositiveValue(TextBox control, ref int valueStore)
        {
            if (string.IsNullOrEmpty(control.Text))
            {
                valueStore = 1;
                control.Text = "1";
            }
            else
            {
                int parsed;
                if (int.TryParse(control.Text, out parsed) && parsed > 0)
                {
                    valueStore = parsed;
                    control.Text = "" + parsed;
                }
                else
                {
                    valueStore = 1;
                    control.Text = "1";
                }
            }
        }

        private void buttonStart_Click(object sender, System.EventArgs e)
        {
            SetGroupBoxesEnabled(false);

            Program.HasRendering = checkBoxRendering.Checked;
            Program.HasFixedTimeStep = checkBoxFixedTimestep.Checked;
            Program.LogToFile = checkBoxLogToFile.Checked;
            Program.GameSpeed = speed;
            Program.SimulationsCount = simulations;

            //listen to simulation results
            if (!listenToResultsRegistered){
                listenToResultsRegistered = true;
                Level.LevelResults += RegisterSimulationResult;
            }

            //setup the level bonus values


            //quick startup format: -[trigger] level world -a agent_path
            quickStartupOptionsTemplate = new string[5];

            if (radioButtonTimeStampFile.Checked)
            {
                //timestamped results file
                quickStartupOptionsTemplate[0] = "-stn";
            }
            else{
                //single results file
                quickStartupOptionsTemplate[0] = "-st";
            }

            quickStartupOptionsTemplate[3] = "-a";

            //determine agent implementations to simulate
            selectedAgentImplementations.Clear();
            foreach (Object item in checkedListBoxImplementations.CheckedItems)
            {
                selectedAgentImplementations.Add((string) item);
            }

            //prepare progress track
            progressBar1.Maximum = GetSimulationsToPerformCount() * simulations;
            progressBar1.Value = 0;
            simulated = 0;
            UpdateProgressBarLabel();

            //prepare simulator thread
            simulator = new Thread(Simulate);
            simulator.SetApartmentState(ApartmentState.STA);
            simulator.IsBackground = true;
            simulator.Start();
        }

        private void SetGroupBoxesEnabled(bool enabled)
        {
            if (this != null)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke((MethodInvoker)delegate()
                    {
                        SetUIComponentsVisibility(enabled);
                    });
                }
                else
                {
                    SetUIComponentsVisibility(enabled);
                }
            }
        }

        private void SetUIComponentsVisibility(bool enabled)
        {
            groupBoxResults.Enabled = groupBoxAgents.Enabled = groupBoxLevels.Enabled = groupBoxSimulation.Enabled = enabled;
            progressBar1.Enabled = !enabled;
            buttonStart.Enabled = enabled;
            buttonStop.Enabled = !enabled;
        }


        private void RegisterSimulationResult(LevelResult result)
        {
            simulated++;
            batchResults[currentAgentImplementation][currentWorld][currentLevel].Add(result);
            UpdateProgressBar();
        }

        private void Simulate()
        {
            Engine currentSimulation;
            batchResults = new Dictionary<string, Dictionary<string, Dictionary<int, List<LevelResult>>>>();

            foreach (string agentImplementation in selectedAgentImplementations)
            {
                string agentImplementationPath = agentImplementationsPath[agentImplementation];
                currentAgentImplementation = agentImplementation;
                if (!batchResults.ContainsKey(currentAgentImplementation))
                    batchResults[currentAgentImplementation] = new Dictionary<string, Dictionary<int, List<LevelResult>>>();

                foreach (TreeNode world in treeViewWorldsLevels.Nodes)
                {
                    if (world.Checked)
                    {
                        currentWorld = world.Text;
                        if (!batchResults[agentImplementation].ContainsKey(currentWorld))
                            batchResults[currentAgentImplementation][currentWorld] = new Dictionary<int, List<LevelResult>>();

                        foreach (TreeNode level in world.Nodes)
                        {
                            //if selected, simulate level
                            if (level.Checked)
                            {
                                currentLevel = level.Index + 1;
                                if (!batchResults[agentImplementation][currentWorld].ContainsKey(currentLevel))
                                    batchResults[currentAgentImplementation][currentWorld][currentLevel] = new List<LevelResult>();

                                Program.quickStartup = (string[])quickStartupOptionsTemplate.Clone();

                                //setup the world and level
                                Program.quickStartup[1] = "" + world.Index;
                                Program.quickStartup[2] = "" + currentLevel;

                                //setup agent
                                Program.quickStartup[4] = agentImplementationPath;

                                //start game simulation
                                if (Program.HasRendering)
                                {
                                    currentSimulation = Program.StartGameEngineWithRendering();
                                }
                                else
                                {
                                    currentSimulation = Program.StartGameEngineWithoutRendering();
                                }
                            }
                        }
                    }
                }
            }
            WriteBatchResultsFile();
            SetGroupBoxesEnabled(true);
        }

        private void WriteBatchResultsFile()
        {
            if (checkBoxWriteResults.Checked)
            {
                FileStream fs;
                string outputFullPath;
                string timestampFile = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
                string timestamp = string.Format("{0:yyyy-MM-dd_HH:mm:ss}", DateTime.Now);

                Directory.CreateDirectory(SinglePlayerLevel.RESULTS_OUTPUT_DIR);

                string filename = textBoxResultsFilename.Text;
                if (string.IsNullOrEmpty(filename) || string.IsNullOrWhiteSpace(filename))
                {
                    filename = defaultBatchResultsFile;
                }

                outputFullPath = Path.Combine(SinglePlayerLevel.RESULTS_OUTPUT_DIR, filename + ".csv");

                FileMode mode = FileMode.Create;
                if (File.Exists(outputFullPath) && !checkBoxOvewriteResults.Checked)
                {
                    mode = FileMode.Append;
                }
                fs = TryOpenFileStream(outputFullPath, mode);
                
                if (fs != null)
                {   
                    StreamWriter sw = new StreamWriter(fs);

                    foreach (var agentImplementationResults in batchResults)
                    {
                        //agent implementation name
                        string agentImplementation = agentImplementationResults.Key;
                        sw.WriteLine(agentImplementation);

                        //iterate reasuls for each world
                        foreach (var worldResults in agentImplementationResults.Value)
                        {
                            string world = worldResults.Key;

                            sw.WriteLine("World: " + csvSeparator + world);
                            sw.WriteLine(GetBatchResultsHeader());
                            //iterate results for each level
                            foreach (var levelResults in worldResults.Value)
                            {
                                sw.WriteLine(GetLevelResultLine(levelResults.Key, levelResults.Value));
                            }
                        }
                    }
                    sw.Close();
                    fs.Close();
                }
            }
        }

        protected string GetBatchResultsHeader()
        {
            return "level" + csvSeparator +
                "runs" + csvSeparator +
                "collectibles average" + csvSeparator +
                "collectibles available" + csvSeparator +
                "time average" + csvSeparator +
                "time limit" + csvSeparator +
                "score average";
        }

        internal string GetLevelResultLine(int level, List<LevelResult> levelResults)
        {
            if (levelResults.Count == 0)
            {
                //simulation(s) might have been cancelled
                return string.Empty;
            }

            int totalRuns = simulations;
            double averageDiamonds, averageTime, averageScore;
            int maximumCollectibles = levelResults[0].CollectiblesAvailable;
            double maximumTime = levelResults[0].TimeLimit;

            averageScore = averageDiamonds = averageTime = 0;
            foreach (LevelResult item in levelResults)
            {
                averageTime += item.ElapsedGameTime;
                averageDiamonds += item.CollectiblesCaught;
                averageScore += item.GetScore(completedBonus, collectibleBonus);
            }

            averageTime /= simulations;
            averageDiamonds /= simulations;
            averageScore /= simulations;

            return "" + level + csvSeparator +
                totalRuns + csvSeparator +
                String.Format("{0:0.#}", averageDiamonds) + csvSeparator +
                maximumCollectibles + csvSeparator +
                String.Format("{0:0.#}", averageTime) + csvSeparator +
                maximumTime + csvSeparator +
                Convert.ToInt32(averageScore);
        }

        protected FileStream TryOpenFileStream(string filename, FileMode openMode)
        {
            try
            {
                return new FileStream(filename, openMode);
            }
            catch (Exception e)
            {
                Log.LogError("Could not create/open batch results file because: " + e.Message);
                Log.LogError(e.StackTrace);
                return null;
            }
        }

        private void UpdateProgressBar()
        {

            if (this != null)
            {
                if (this.InvokeRequired)
                {
                    this.BeginInvoke((MethodInvoker)delegate()
                    {
                        UpdateProgress();
                    });
                }
                else
                {
                    UpdateProgress();
                }
            } 
        }

        private void UpdateProgress()
        {
            //progressBar1.Value = progressBar1.Value + 1 * simulations;
            progressBar1.Value = simulated;
            UpdateProgressBarLabel();
        }

        private void UpdateProgressBarLabel()
        {
            labelProgressBarText.Text = "" + progressBar1.Value + " / " + progressBar1.Maximum;
        }

        private int GetSimulationsToPerformCount()
        {
            int count = 0;
            foreach (string agentImplementation in selectedAgentImplementations)
            {
                foreach (TreeNode world in treeViewWorldsLevels.Nodes)
                {
                    foreach (TreeNode level in world.Nodes)
                    {
                        //if selected, simulate level
                        if (level.Checked)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (simulator != null && simulator.IsAlive)
            {
                simulator.Abort();
                SetGroupBoxesEnabled(true);
            }
        }

        private void ValidateTextIntegerValue(TextBox control, ref int valueStore, int defaultValue)
        {
            if (string.IsNullOrEmpty(control.Text))
            {
                valueStore = defaultValue;
                control.Text = "" + defaultValue;
            }
            else
            {
                int parsed;
                if (int.TryParse(control.Text, out parsed))
                {
                    valueStore = parsed;
                    control.Text = "" + parsed;
                }
                else
                {
                    valueStore = defaultValue;
                    control.Text = "" + defaultValue;
                }
            }
        }

        private void textBoxCompletedBonus_TextChanged(object sender, EventArgs e)
        {
            ValidateTextIntegerValue(textBoxCompletedBonus, ref completedBonus, completedBonusDefault);
        }

        private void textBoxCollectibleBonus_TextChanged(object sender, EventArgs e)
        {
            ValidateTextIntegerValue(textBoxCollectibleBonus, ref collectibleBonus, collectibleBonusDefault);
        }
    }
}
