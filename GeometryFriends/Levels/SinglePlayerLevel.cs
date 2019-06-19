using GeometryFriends.AI;
using GeometryFriends.AI.Perceptions.Information;
using GeometryFriends.Input;
using GeometryFriends.Levels.Shared;
using GeometryFriends.ScreenSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.IO;

namespace GeometryFriends.Levels
{
    internal class SinglePlayerLevel : MultiplayerLevel
    {
        public const string RESULTS_OUTPUT_DIR = "Results";
        public const string RESULTS_FILENAME = "Results.csv";
        public const string TIMESTAMP_RESULTS_FILENAME = "{0}_Results.csv";
        public const char CSV_SEPERATOR = ';';

        AgentsManager agentsManager;
        bool togglePositions, toggleGraph = true;
        DrawingSystem.LineBrush allLineBrush, ballLineBrush, squareLineBrush;
        
        private AgentDebugPane agentPane;
        public AgentDebugPane AgentPane { 
            get { 
                return agentPane; 
            } 
        }

        internal enum AgentsToRun
        {
            Circle,
            Rectangle,
            Both
        }
        private AgentsToRun agents;
        public AgentsToRun Agents {
            get { return agents; }
        }

        //Sensors Information to Draw The Graph
        private CountInformation numbersInfo;
        private RectangleRepresentation rectangleInfo;
        private CircleRepresentation circleInfo;
        private ObstacleRepresentation[] obstaclesInfo;
        private ObstacleRepresentation[] rectanglePlatformsInfo;
        private ObstacleRepresentation[] circlePlatformsInfo;
        private CollectibleRepresentation[] collectiblesInfo;
        private System.Drawing.Rectangle area;
        
        public SinglePlayerLevel(int levelNumber, AgentDebugPane agentPane, AgentsToRun agents)
            : base(levelNumber)
        {
            this.agentPane = agentPane;
            this.agents = agents;

            Level.IsMulti(false);
        }

        public SinglePlayerLevel(int levelNumber, int worldNumber, AgentDebugPane agentPane, AgentsToRun agents)
            : base(levelNumber, worldNumber)
        {
            this.agentPane = agentPane;
            this.agents = agents;

            Level.IsMulti(false);
        }

         public override void LoadLevelContent()
         {
             base.LoadLevelContent();

             //Manager que cria os agentes e faz a ligação entre os agentes e o jogo
             agentsManager = new AgentsManager(this, this.agentPane);

             //properly execute the desired agents
             switch (agents)
             {
                 case AgentsToRun.Circle:
                     agentsManager.RunCircle = true;
                     agentsManager.RunRectangle = false;
                     break;
                 case AgentsToRun.Rectangle:
                     agentsManager.RunCircle = false;
                     agentsManager.RunRectangle = true;
                     break;
                 case AgentsToRun.Both:
                     agentsManager.RunCircle = true;
                     agentsManager.RunRectangle = true;
                     break;
                 default:
                     throw new Exception("Unhandled agents to run setting in single level.");
             }

             //Para garantir que so carrega os agentes dps de criar toda a informação dos sensores
             if (agentsManager.CreateSensorsInfo() == 1)
                 agentsManager.LoadAgents();

             this.numbersInfo = agentsManager.GetNumbersInfo();
             this.rectangleInfo = agentsManager.GetRectangleInfo();
             this.circleInfo = agentsManager.GetCircleInfo();
             this.obstaclesInfo = agentsManager.GetObstaclesInfo();
             this.rectanglePlatformsInfo = agentsManager.GetRectanglePlatformsInfo();
             this.circlePlatformsInfo = agentsManager.GetCirclePlatformsInfo();
             this.collectiblesInfo = agentsManager.GetCollectiblesInfo();
             this.area = agentsManager.GetArea();

             //DebugSensorsInfo();

             allLineBrush = new DrawingSystem.LineBrush(2, Color.White);
             squareLineBrush = new DrawingSystem.LineBrush(2, Color.LimeGreen);
             ballLineBrush = new DrawingSystem.LineBrush(2, Color.Yellow);
         }

         public override void UnloadContent()
         {
             base.UnloadContent();
             agentsManager.UnloadAgents();
         }

         protected void DebugSensorsInfo()
         {
             Log.LogInformation("Single Player - " + numbersInfo.ToString());

             Log.LogInformation("Single Player - " + rectangleInfo.ToString());

             Log.LogInformation("Single Player - " + circleInfo.ToString());

             foreach (ObstacleRepresentation i in obstaclesInfo)
             {
                 Log.LogInformation("Single Player - " + i.ToString("Obstacle"));
             }

             foreach (ObstacleRepresentation i in rectanglePlatformsInfo)
             {
                 Log.LogInformation("Single Player - " + i.ToString("Rectangle Platform"));
             }

             foreach (ObstacleRepresentation i in circlePlatformsInfo)
             {
                 Log.LogInformation("Single Player - " + i.ToString("Circle Platform"));
             }

             foreach (CollectibleRepresentation i in collectiblesInfo)
             {
                 Log.LogInformation("Single Player - " + i.ToString());
             }
         }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            //TODO: LoadLevelContent and LoadContent are meshed together making it hard to separate for simulations where we just want the level configured. Make this separation.
            if (!ContentLoaded)
                LoadContent();

            if (agentsManager.AgentsGaveUp)
            {
                EndGameMessage = "Agents gave up.";
                EndGame();
            }

            agentsManager.Update(gameTime.ElapsedGameTime);
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void DrawLevel(DrawingInstructionsBatch instructionsBatch)
        {
            base.DrawLevel(instructionsBatch);    
        
            //draw agents specific debug information
            if(IsDebugViewEnabled){                
                agentsManager.DrawAgentsDebugInformation(instructionsBatch, DebugFont);                
            }
        }

        protected override void HandleCircleControls(InputState input)
        {
            //if (input.LastKeyboardState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.D0) && input.CurrentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D0))
            //    agentsManager.npcSquare.toggleDebug();
            
            if (input.LastKeyboardState.IsKeyUp(Keys.Tab) && input.CurrentKeyboardState.IsKeyDown(Keys.Tab))
            {
                togglePositions = !togglePositions;
                this.agentPane.GraphInfo = (togglePositions ? "Graph: Position" : "Graph: Vel. req.");
            }

            if (input.LastKeyboardState.IsKeyUp(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1))
                togglePositions = !toggleGraph;

            if (agents == AgentsToRun.Circle || agents == AgentsToRun.Both)
            {
                agentsManager.CircleAgentListener();                
            }
            else
            {
                base.HandleCircleControls(input);
            }
        }

        protected override void HandleRectangleControls(InputState input)
        {
            if (agents == AgentsToRun.Rectangle || agents == AgentsToRun.Both)
            {
                agentsManager.RectangleAgentListener();
            }
            else
            {
                base.HandleRectangleControls(input);
            }
        }

        protected override void EndGame()
        {
            FileStream fs;
            String newLine, levelPass;
            string outputFullPath;
            bool writeHeader = true;
            bool levelPassedBool = false;
            string timestampFile = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            string timestamp = string.Format("{0:yyyy-MM-dd_HH:mm:ss}", DateTime.Now);

            if (numberOfTrianglesCollected == GetCollectibles().Count)
            {
                levelPass = "1";
                levelPassedBool = true;
            }
            else
            {
                levelPass = "0";
                levelPassedBool = false;
            }
            LevelResult result = new LevelResult(timestamp, levelPassedBool, elapsedGameTime, XMLLevelParser.GetTimeLimit(), numberOfTrianglesCollected, GetCollectibles().Count);

            Directory.CreateDirectory(RESULTS_OUTPUT_DIR);

            if (Engine.newResultsFileFlag)
            {
                outputFullPath = Path.Combine(RESULTS_OUTPUT_DIR, TIMESTAMP_RESULTS_FILENAME);
                outputFullPath = String.Format(outputFullPath, timestampFile);
                fs = TryOpenFileStream(outputFullPath, FileMode.Create);
            } else {
                outputFullPath = Path.Combine(RESULTS_OUTPUT_DIR, RESULTS_FILENAME);
                if(File.Exists(outputFullPath)){
                    writeHeader = false;
                }
                fs = TryOpenFileStream(outputFullPath, FileMode.Append);
            }

            if (fs != null)
            {
                StreamWriter sw = new StreamWriter(fs);

                if (writeHeader)
                {
                    //write header
                    newLine = "Timestamp" + CSV_SEPERATOR +
                        "Agent Implementation" + CSV_SEPERATOR +
                        "World Name " + CSV_SEPERATOR +
                        "Level Number" + CSV_SEPERATOR + 
                        "Collectibles Collected" + CSV_SEPERATOR + 
                        "Collectibles to Catch" + CSV_SEPERATOR +
                        "Passed Level?" + CSV_SEPERATOR + 
                        "Elapsed Game Time (s)" + CSV_SEPERATOR +                         
                        "Time Limit (s)" + CSV_SEPERATOR +
                        "Elapsed Real Time (s)" + CSV_SEPERATOR +
                        "Score" + CSV_SEPERATOR +
                        "Endgame Message";
                    sw.WriteLine(newLine);
                }

                string elapsedRealTime = (((float)GetElapsedRealTime()) / 1000).ToString();
                newLine = timestamp + CSV_SEPERATOR +
                    agentsManager.CurrentAgentsImplementation + CSV_SEPERATOR +
                    XMLLevelParser.GetWorldName() + CSV_SEPERATOR +  
                    GetLevelNumber() + CSV_SEPERATOR + 
                    numberOfTrianglesCollected + CSV_SEPERATOR + 
                    GetCollectibles().Count + CSV_SEPERATOR +
                    levelPass + CSV_SEPERATOR + 
                    elapsedGameTime  + CSV_SEPERATOR +
                    XMLLevelParser.GetTimeLimit().ToString() + CSV_SEPERATOR +
                    elapsedRealTime + CSV_SEPERATOR +
                    result.GetScore(LevelCompletedBonus, CollectibleCaughtBonus)  + CSV_SEPERATOR + 
                    EndGameMessage;
                
                sw.WriteLine(newLine);
                sw.Close();
                fs.Close();
            }

            //communicate level results to any consumers
            SendLevelResults(result);

            //notify agents
            agentsManager.SendEndGame(numberOfTrianglesCollected, (int)elapsedGameTime);
            ScreenManager.RemoveScreen(this);
        }

        protected FileStream TryOpenFileStream(string filename, FileMode openMode)
        {
            try
            {
                return new FileStream(filename, openMode);
            }
            catch (IOException e)
            {
                Log.LogError("Could not create/open results file because: " + e.Message);
                Log.LogError(e.StackTrace);
                return null;
            }
        }
    }
}

