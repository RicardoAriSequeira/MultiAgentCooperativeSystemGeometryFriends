//  Author(s):
//  João Catarino <joaopereiracatarino@gmail.com

using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.AI.ActionSimulation;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Perceptions;
using GeometryFriends.AI.Perceptions.Information;
using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace GeometryFriends.AI
{
    class AgentsManager
    {
        public static string AGENTS_DLL_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Agents");
        protected static Dictionary<string, Assembly> allAgentAssemblies;
        protected static bool agentsLoaded = false;
        protected static string CONTENT_AGENTS_FOLDER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Agents");

        private  AgentDebugPane agentPane;

        public AbstractRectangleAgent npcRectangle;
        public AbstractCircleAgent npcCircle;

        protected bool circleSensorsUpdated;
        protected bool rectangleSensorsUpdated;
        protected bool circleActionsUpdated;
        protected bool rectangleActionsUpdated;

        protected Moves circleCurrentAction, rectangleCurrentAction;

        protected Level level;

        protected static string path;

        protected TimeSpan elapsedGameTime;
        protected SensorsManager perceptions;
        protected int numObstacles, numRectanglePlatforms, numCirclePlatforms;
        protected int numCollectiblesLeft;
        protected List<RectanglePlatform> obstacles;
        protected List<RectanglePlatform> rectanglePlatforms;
        protected List<RectanglePlatform> circlePlatforms;
        protected TriangleCollectible[] collectiblesArray;
        //protected List<Elevator> elevators;

        //Structs that have all the information about one map level
        protected CountInformation numbersInfo;
        protected RectangleRepresentation rectangleInfo;
        protected CircleRepresentation circleInfo;
        protected ObstacleRepresentation[] obstaclesInfo;
        protected ObstacleRepresentation[] rectanglePlatformsInfo;
        protected ObstacleRepresentation[] circlePlatformsInfo;
        protected CollectibleRepresentation[] collectiblesInfo;

        //defaults for agent actions
        public const float DEFAULT_CIRCLE_AGENT_JUMP_INTESITY = 4.0f;

        //structure to contain the information required for physics lookahead in the game
        protected ActionSimulator simulator;

        //structures to enable visual debugging of the agents
        public List<DebugInformation> AgentsDebugInformation { get; set; }
        private object debugInfoLock = new Object();
        private object logLock = new Object();

        //control running individual agents
        public bool RunCircle { get; set; }
        public bool RunRectangle { get; set; }

        public bool AgentsGaveUp { get; set; }

        public string CurrentAgentsImplementation { get; set; }

        public AgentsManager(Level lvl, AgentDebugPane aP)
        {
            
            this.level = lvl;
            this.agentPane = aP;
            this.agentPane.setRectangle(this.level.rectangle);

            RunCircle = RunRectangle = true;

            perceptions = new SensorsManager((SinglePlayerLevel)lvl);

            CurrentAgentsImplementation = string.Empty;

            //Load Agents' DLL
            LoadAgentsDlls();

            //CreateSensorsInfo();

            AgentsDebugInformation = new List<DebugInformation>();
            AgentsGaveUp = false;
        }

        public static string GetAgentsDLLDirectory()
        {
            return AGENTS_DLL_PATH;
        }

        public static void LoadAgentsDlls()
        {
            if (agentsLoaded)
                return;

            path = AGENTS_DLL_PATH;

            //load all existing agents
            allAgentAssemblies = new Dictionary<string, Assembly>();

            string[] allDllFiles = new string[0];
            
            //load implementations available
            if (Directory.Exists(path))
                allDllFiles = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            else
            {
                //the agents implementations directory does not exist, create one
                Directory.CreateDirectory(path);
            }

            if (allDllFiles.Length == 0)
            {
                //if there are no agent implementations available, populate with all the agent implementation examples that we have available from content
#if DEBUG
                string agentsContentFolder = Path.Combine(CONTENT_AGENTS_FOLDER, "Debug");
#else
                string agentsContentFolder = Path.Combine(CONTENT_AGENTS_FOLDER, "Release");
#endif
                if (Directory.Exists(agentsContentFolder))
                {
                    string[] files = Directory.GetFiles(agentsContentFolder);
                    string fileName, destFile;

                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        fileName = Path.GetFileName(s);
                        destFile = Path.Combine(path, fileName);
                        File.Copy(s, destFile, true);
                    }
                }

                //get all the dlls for loading
                allDllFiles = Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
            }

            Assembly newDll;
            string agentsImplementation;
            List<string> couldNotLoad = new List<string>();
            foreach (string s in allDllFiles)
            {
                //load the dll
                try
                {
                    newDll = Assembly.LoadFile(s);
                }
                catch (NotSupportedException)
                {
                    couldNotLoad.Add(Path.GetFileName(s));                    
                    continue;
                }
                //get the dll name and store a human readable version
                agentsImplementation = Path.GetFileNameWithoutExtension(s);
                //insert spaces 
                agentsImplementation = GetHumanReadableDllID(agentsImplementation);
                //store assembly
                allAgentAssemblies.Add(agentsImplementation, newDll);
            }

            if (couldNotLoad.Count > 0)
            {
                string implementations = "[ " + String.Join(", ", couldNotLoad) + " ]";

                Log.LogWarning("Agent implementation " + implementations + " found in '{game_dir}/Agents/' could not be loaded.\n\n" +
                        "This is usually because you have downloaded the game from the internet and extracted the game files with a program that blocks the dlls from being loaded." +
                        " You must unblock the dll files so that they can be loaded. In Windows right-click on each dll file -> properties -> Unblock (at the end of the General properties pane).",
                        true);
            }

            if (allAgentAssemblies.Count == 0)
                Log.LogWarning("No agent implementations available.");
        }

        public static string GetHumanReadableDllID(string dllFileNameWithoutExtension)
        {
            return string.Concat(dllFileNameWithoutExtension.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }

        public static List<string> GetAgentImplementations()
        {
            if (!agentsLoaded)
                LoadAgentsDlls();
            return allAgentAssemblies.Keys.ToList();
        }

        public static Dictionary<string, string> GetAgentImplementationsPathMapping()
        {
            if (!agentsLoaded)
                LoadAgentsDlls();

            Dictionary<string, string> agentImplementationsPathMapping = new Dictionary<string, string>();

            foreach (KeyValuePair<string, Assembly> item in allAgentAssemblies)
            {
                agentImplementationsPathMapping.Add(item.Key, item.Value.Location);
            }

            return agentImplementationsPathMapping;
        }

        public static bool HasAgentsImplementation()
        {
            if (!agentsLoaded)
                LoadAgentsDlls();
            if (allAgentAssemblies.Count > 0)
                return true;
            else
                return false;
        }

        public int CreateSensorsInfo()
        {
            int temp;

            obstacles = perceptions.Obstacles;
            numObstacles = obstacles.Count();
            obstaclesInfo = new ObstacleRepresentation[numObstacles];
            
            rectanglePlatforms = perceptions.RectanglePlatforms;
            numRectanglePlatforms = rectanglePlatforms.Count();
            rectanglePlatformsInfo = new ObstacleRepresentation[numRectanglePlatforms];
            
            circlePlatforms = perceptions.CirclePlatforms;
            numCirclePlatforms = circlePlatforms.Count();
            circlePlatformsInfo = new ObstacleRepresentation[numCirclePlatforms];
            
            collectiblesArray = perceptions.Collectibles.ToArray();
            //numCollectiblesCaught = 0;
            numCollectiblesLeft = perceptions.Collectibles.Count();
            collectiblesInfo = new CollectibleRepresentation[numCollectiblesLeft];

            // numbersInfo Description

            numbersInfo = new CountInformation(numObstacles, numRectanglePlatforms, numCirclePlatforms, numCollectiblesLeft);

            // rectangleInfo Description

            rectangleInfo = new RectangleRepresentation(perceptions.RectanglePosition.X, perceptions.RectanglePosition.Y, perceptions.RectangleVelocity.X, perceptions.RectangleVelocity.Y, perceptions.RectangleHeight);

            // circleInfo Description

            circleInfo = new CircleRepresentation(perceptions.CirclePosition.X, perceptions.CirclePosition.Y, perceptions.CircleVelocity.X, perceptions.CircleVelocity.Y, perceptions.CircleRadius);

            // Obstacles and Platforms Info Description

            temp = 0;
            foreach (RectanglePlatform o in obstacles)
            {
                obstaclesInfo[temp] = o.GetRepresentation();
                temp++;
            }

            // rectanglePlatformsInfo Description

            temp = 0;
            foreach(RectanglePlatform o in rectanglePlatforms)
            {
                rectanglePlatformsInfo[temp] = o.GetRepresentation();                
                temp++;
            }

            // circlePlatformsInfo Description

            temp = 0;
            foreach(RectanglePlatform o in circlePlatforms)
            {
                circlePlatformsInfo[temp] = o.GetRepresentation();
                temp++;
            }
            

            //Collectibles' To Catch Coordinates (X,Y)

            temp = 0;
            foreach(TriangleCollectible c in collectiblesArray)
            {
                collectiblesInfo[temp] = c.GetRepresentation();
                temp++;
            }

            //also setup the simulator
            simulator = CreateUpdatedSimulator();

            //DebugSensorsInfo();

            //nao devia estar aqui(é chamado no singleplayerlevel, so p descobrir bug)
            //LoadAgents();

            return 1;
        }

        protected ActionSimulator CreateUpdatedSimulator()
        {
            PhysicsSimulator clonedPhysicsSimulator = level.GetPhysicsSimulator().Clone();
            CircleCharacter circle = null;
            RectangleCharacter rectangle = null;
            foreach (Geom item in clonedPhysicsSimulator.GeomList)
            {
                if (item.Tag.GetType() == typeof(String)){
                    string tag = (String) item.Tag;
                    if (tag == CircleCharacter.CIRCLE_CHARACTER_ID)
                    {
                        circle = level.circle.Clone(clonedPhysicsSimulator, item.Body, item);
                    }
                    else if(tag == RectangleCharacter.RECTANGLE_CHARACTER_ID){
                        rectangle = level.rectangle.Clone(clonedPhysicsSimulator, item.Body, item);
                    }
                }
            }

            if (circle == null)
                Log.LogWarning("AgentsManager: No geometry found in the simulator that was representative of the circle character.");
            if (rectangle == null)
                Log.LogWarning("AgentsManager: No geometry found in the simulator that was representative of the rectangle character.");

            return new ActionSimulator(clonedPhysicsSimulator, circle, rectangle);
        }

        public CountInformation GetNumbersInfo()
        {
            return numbersInfo;
        }

        public RectangleRepresentation GetRectangleInfo()
        {
            return rectangleInfo;
        }

        public CircleRepresentation GetCircleInfo()
        {
            return circleInfo;
        }

        public ObstacleRepresentation[] GetObstaclesInfo()
        {
            return obstaclesInfo;
        }

        public ObstacleRepresentation[] GetRectanglePlatformsInfo()
        {
            return rectanglePlatformsInfo;
        }

        public ObstacleRepresentation[] GetCirclePlatformsInfo()
        {
            return circlePlatformsInfo;
        }

        public CollectibleRepresentation[] GetCollectiblesInfo()
        {
            return collectiblesInfo;
        }

        public System.Drawing.Rectangle GetArea()
        {
            return level.Area;
        }


        public int UpdateSensorsInfo()
        {
            int temp;

            //update sensors
            perceptions.Sense();

            collectiblesArray = perceptions.Collectibles.ToArray();
            //numCollectiblesCaught = 0;
            numCollectiblesLeft = perceptions.Collectibles.Count;

            rectangleInfo.X = perceptions.RectanglePosition.X;
            rectangleInfo.Y = perceptions.RectanglePosition.Y;
            rectangleInfo.VelocityX = perceptions.RectangleVelocity.X;
            rectangleInfo.VelocityY = perceptions.RectangleVelocity.Y;
            rectangleInfo.Height = perceptions.RectangleHeight;

            circleInfo.X = perceptions.CirclePosition.X;
            circleInfo.Y = perceptions.CirclePosition.Y;
            circleInfo.VelocityX = perceptions.CircleVelocity.X;
            circleInfo.VelocityY = perceptions.CircleVelocity.Y;
            circleInfo.Radius = perceptions.CircleRadius;

            Array.Resize(ref collectiblesInfo, numCollectiblesLeft);

            temp = 0;
            foreach (TriangleCollectible c in collectiblesArray)
            {
                collectiblesInfo[temp] = c.GetRepresentation();
                temp++;
            }

            //also update the simulator
            simulator = CreateUpdatedSimulator();
            
            //DebugSensorsInfo();

            return 1;
        }
       

        public void LoadAgents()
        {
            // dynamically load assembly for the agents according to the agent implementation selected            
            Assembly agentsDLL = null;
            if (GeometryFriends.Properties.Settings.Default.AgentsImplementation == ""
                || GeometryFriends.Properties.Settings.Default.AgentsImplementation == null
                || !allAgentAssemblies.Keys.Contains(GeometryFriends.Properties.Settings.Default.AgentsImplementation))
            {
                //there is no implementation specified, simply select the first available or the currently selected implementation is not available, 
                if(allAgentAssemblies.Count > 0){
                    var first = allAgentAssemblies.First();
                    GeometryFriends.Properties.Settings.Default.AgentsImplementation = first.Key;
                    agentsDLL = first.Value;

                    Log.LogWarning("Settings agents implementation is not available, setting the first available.");
                }
                else{
                    Log.LogWarning("No agents implementation available to be loaded.");
                    return;
                }
            }
            else{
                //the selected implementation is available
                agentsDLL = allAgentAssemblies[GeometryFriends.Properties.Settings.Default.AgentsImplementation];
            }

            //register the implementation selected for loading
            CurrentAgentsImplementation = agentsDLL.GetName().Name;

            // get type of classes BallAgent and SquareAgent from just loaded assembly
            Type circleType = agentsDLL.GetType("GeometryFriendsAgents.CircleAgent");
            Type rectangleType = agentsDLL.GetType("GeometryFriendsAgents.RectangleAgent");

            // create instances of classes BallAgent and SquareAgent, only when they are playing, have positive coordinates
            if (circleInfo.X > 0 && circleInfo.Y > 0)
                npcCircle = (AbstractCircleAgent)Activator.CreateInstance(circleType);
            if (rectangleInfo.X > 0 && rectangleInfo.Y > 0)
                npcRectangle = (AbstractRectangleAgent)Activator.CreateInstance(rectangleType);

            circleSensorsUpdated = true;
            rectangleSensorsUpdated = true;
            circleActionsUpdated = true;
            rectangleActionsUpdated = true;

            if (npcCircle != null && npcRectangle != null)
            {   
                npcCircle.Setup(numbersInfo, rectangleInfo, circleInfo, obstaclesInfo, rectanglePlatformsInfo, circlePlatformsInfo, collectiblesInfo, level.Area, XMLLevelParser.GetTimeLimit());
                npcRectangle.Setup(numbersInfo, rectangleInfo, circleInfo, obstaclesInfo, rectanglePlatformsInfo, circlePlatformsInfo, collectiblesInfo, level.Area, XMLLevelParser.GetTimeLimit());

                // Set Team Name
                if (npcCircle.ImplementedAgent() && npcRectangle.ImplementedAgent())
                    XMLLevelParser.ChangePlayerName(npcCircle.AgentName() + "&" + npcRectangle.AgentName());
                else if (npcCircle.ImplementedAgent())
                    XMLLevelParser.ChangePlayerName(npcCircle.AgentName());
                else
                    XMLLevelParser.ChangePlayerName(npcRectangle.AgentName());
            }
            else if (npcCircle != null)
            {
                npcCircle.Setup(numbersInfo, rectangleInfo, circleInfo, obstaclesInfo, rectanglePlatformsInfo, circlePlatformsInfo, collectiblesInfo, level.Area, XMLLevelParser.GetTimeLimit());
                XMLLevelParser.ChangePlayerName(npcCircle.AgentName());
            }
            else if (npcRectangle != null)
            {
                npcRectangle.Setup(numbersInfo, rectangleInfo, circleInfo, obstaclesInfo, rectanglePlatformsInfo, circlePlatformsInfo, collectiblesInfo, level.Area, XMLLevelParser.GetTimeLimit());
                XMLLevelParser.ChangePlayerName(npcRectangle.AgentName());
            }
        }


        public void UnloadAgents()
        {
            //wait for any peding circle/rectangle update threads
            while (!circleActionsUpdated || !rectangleActionsUpdated) 
            { 
                //wait for circle/rectangle updates to finish before unloading
            }

            npcCircle = null; 
            npcRectangle = null;
        }

        private void SendInfoToAgents() 
        {
            SensorData data = new SensorData(numCollectiblesLeft, rectangleInfo, circleInfo, collectiblesInfo, simulator);
            //ensure that there is no possibility of cross-thread problems
            simulator = null;
            if (npcCircle != null && npcCircle.ImplementedAgent() && RunCircle)
            {
                if (circleSensorsUpdated)
                {
                    circleSensorsUpdated = false;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(CircleSensorCallback), data);
                }
            }
            if (npcRectangle != null && npcRectangle.ImplementedAgent() && RunRectangle)
            {
                if (rectangleSensorsUpdated)
                {
                    rectangleSensorsUpdated = false; 
                    ThreadPool.QueueUserWorkItem(new WaitCallback(RectangleSensorCallback), data);
                }
            }
        }

        //Listener that gets actions from Circle Agent
        public void CircleAgentListener()
        {
            if (npcCircle != null && npcCircle.ImplementedAgent() && RunCircle)
            {
                circleCurrentAction = npcCircle.GetAction();
                switch (circleCurrentAction)
                {
                    case Moves.ROLL_LEFT:
                        level.circle.SpinLeft();
                        break;
                    case Moves.ROLL_RIGHT:
                        level.circle.SpinRight();
                        break;
                    case Moves.JUMP:
                        level.circle.Jump(DEFAULT_CIRCLE_AGENT_JUMP_INTESITY);
                        break;
                    case Moves.GROW:
                        level.circle.Grow();
                        break;
                    default:
                        break;
                }
            }
        }

        //Listener that gets actions from Rectangle Agent
        public void RectangleAgentListener()
        {
            if (npcRectangle != null && npcRectangle.ImplementedAgent() && RunRectangle)
            {
                rectangleCurrentAction = npcRectangle.GetAction();
                switch (rectangleCurrentAction)
                {
                    case Moves.MOVE_LEFT:
                        level.rectangle.SlideLeft();
                        break;
                    case Moves.MOVE_RIGHT:
                        level.rectangle.SlideRight();
                        break;
                    case Moves.MORPH_UP:
                        level.rectangle.StretchVerticalUp();
                        break;
                    case Moves.MORPH_DOWN:
                        level.rectangle.StretchVerticalDown();
                        break;
                    default:
                        break;
                }
            }
        }

        public void Update(TimeSpan elapGameT)
        {
            elapsedGameTime = elapGameT;
            bool circleInGame = false;
            bool rectangleInGame = false;

            //handle sensor updates            
            if (npcRectangle != null && npcRectangle.ImplementedAgent() && RunRectangle)
            {
                if (npcRectangle.HasAgentGivenUp())
                    AgentsGaveUp = true;
                else
                    rectangleInGame = true;

                if (rectangleActionsUpdated)
                {
                    rectangleActionsUpdated = false;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(RectangleUpdateCallback), elapsedGameTime);
                }
            }
            if (npcCircle != null && npcCircle.ImplementedAgent() && RunCircle)
            {
                if (npcCircle.HasAgentGivenUp())
                    AgentsGaveUp = true;
                else
                    circleInGame = true;

                if (circleActionsUpdated)
                {
                    circleActionsUpdated = false;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(CircleUpdateCallback), elapsedGameTime);
                }
            }

            //get and deliver messages
            if (circleInGame && rectangleInGame)
            {
                npcRectangle.HandleAgentMessages(new List<AgentMessage>(npcCircle.GetAgentMessages().ToArray()));
                npcCircle.HandleAgentMessages(new List<AgentMessage>(npcRectangle.GetAgentMessages().ToArray()));
            }

            UpdateSensorsInfo();
            SendInfoToAgents();
            //ConnectAgents();
        }

        protected void DebugSensorsInfo()
        {
            Log.LogInformation("MANAGER - " + numbersInfo.ToString());

            Log.LogInformation("MANAGER - " + rectangleInfo.ToString());

            Log.LogInformation("MANAGER - " + circleInfo.ToString());
            
            foreach (ObstacleRepresentation i in obstaclesInfo)
            {
                Log.LogInformation("MANAGER - " + i.ToString("Obstacle"));
            }

            foreach (ObstacleRepresentation i in rectanglePlatformsInfo)
            {
                Log.LogInformation("MANAGER - " + i.ToString("Rectangle Platform"));
            }

            foreach (ObstacleRepresentation i in circlePlatformsInfo)
            {
                Log.LogInformation("MANAGER - " + i.ToString("Circle Platform"));
            }
            
            foreach (CollectibleRepresentation i in collectiblesInfo)
            {
                Log.LogInformation("MANAGER - " + i.ToString());
            }
        }

        internal void SendEndGame(int numberOfTrianglesCollected, double timeElapsed) {
            //Thread.Sleep(150); // ensures everything is updated
            UpdateSensorsInfo();
            SendInfoToAgents();
            if (npcCircle != null && npcCircle.ImplementedAgent() && RunCircle)
                npcCircle.EndGame(numberOfTrianglesCollected, (int)timeElapsed);
            if (npcRectangle != null && npcRectangle.ImplementedAgent() && RunRectangle)
                npcRectangle.EndGame(numberOfTrianglesCollected, (int)timeElapsed);
        }

        protected void CircleSensorCallback(Object data)
        {
            try
            {
                if (npcCircle != null) //prevent call to npcCircle after level has been disposed
                {
                    SensorData sensorData = (SensorData)data;
                    npcCircle.SensorsUpdated(sensorData.nC, sensorData.rI, sensorData.cI, sensorData.colI);
                    npcCircle.ActionSimulatorUpdated(sensorData.simulator);
                    circleSensorsUpdated = true;
                }
            }
            catch (Exception e)
            {
                circleSensorsUpdated = true;
                Log.LogWarning("Did not complete circle agent sensor update due to exception: " + e.Message);
                Log.LogWarning("Stacktrace: " + e.StackTrace);
            }
        }

        protected void RectangleSensorCallback(Object data)
        {
            try
            {
                if (npcRectangle != null) //prevent call to npcRectangle after level has been disposed
                {
                    SensorData sensorData = (SensorData)data;
                    npcRectangle.SensorsUpdated(sensorData.nC, sensorData.rI, sensorData.cI, sensorData.colI);
                    npcRectangle.ActionSimulatorUpdated(sensorData.simulator);
                    rectangleSensorsUpdated = true;
                }
            }
            catch (Exception e)
            {
                rectangleSensorsUpdated = true;
                Log.LogWarning("Did not complete rectangle agent sensor update due to exception: " + e.Message);
                Log.LogWarning("Stacktrace: " + e.StackTrace);
            }
        }

        protected void CircleUpdateCallback(Object data)
        {
            try
            {
                TimeSpan elapsedGameTime = (TimeSpan)data;
                npcCircle.Update(elapsedGameTime);
                if (Engine.loggerStartFlag && !Level.isFinished)
                    Engine.logger_circle.WriteLine(level.GetElapsedGameTime() + " Collectibles Left :" + numCollectiblesLeft);
                //add circle specific debug information
                AddDebugInformation(npcCircle.GetDebugInformation());
                circleActionsUpdated = true;
            }
            catch (Exception e)
            {
                circleActionsUpdated = true;
                Log.LogWarning("Did not complete circle update due to exception: " + e.Message);
                Log.LogWarning("Stacktrace: " + e.StackTrace);
            }
        }

        protected void RectangleUpdateCallback(Object data)
        {
            try
            {
                TimeSpan elapsedGameTime = (TimeSpan)data;
                npcRectangle.Update(elapsedGameTime);
                if (Engine.loggerStartFlag && !Level.isFinished)
                    Engine.logger_rect.WriteLine(level.GetElapsedGameTime() + " Collectibles Left : " + numCollectiblesLeft);
                //add rectangle specific debug information
                AddDebugInformation(npcRectangle.GetDebugInformation());
                rectangleActionsUpdated = true;
            }
            catch (Exception e)
            {
                rectangleActionsUpdated = true;
                Log.LogWarning("Did not complete rectangle update due to exception: " + e.Message);
                Log.LogWarning("Stacktrace: " + e.StackTrace);
            }
        }

        //thread safe addition to debug information
        protected void AddDebugInformation(DebugInformation[] toAdd)
        {
            if (toAdd == null)
                return;

            lock (debugInfoLock)
            {
                AgentsDebugInformation.AddRange(toAdd);
            }
        }

        public void DrawAgentsDebugInformation(DrawingInstructionsBatch instructionsBatch, SpriteFont debugFont)
        {
            lock (debugInfoLock)
            {
                if(AgentsDebugInformation.Count == 0)
                    return; //nothing to draw

                //perform cleanup before drawing
                int toRemove = -1;
                for (int i = 0; i < AgentsDebugInformation.Count; i++)
                {
                    if (AgentsDebugInformation[i].Representation == DebugInformation.RepresentationType.CLEAR)
                    {
                        toRemove = i;
                    }
                }

                if(toRemove > -1){
                    AgentsDebugInformation.RemoveRange(0, toRemove + 1); //remove clear instruction and all previous                    
                }
                //actual drawing
                foreach (DebugInformation item in AgentsDebugInformation)
                {
                    switch (item.Representation)
                    {
                        case DebugInformation.RepresentationType.CIRCLE:
                            instructionsBatch.DrawCircle(item.Position - new Vector2(item.Radius / 2, item.Radius / 2), (int)item.Radius, item.Color);
                            break;
                        case DebugInformation.RepresentationType.LINE:
                            instructionsBatch.DrawLine(item.StartPoint, item.EndPoint, 2, item.Color);
                            break;
                        case DebugInformation.RepresentationType.RECTANGLE:
                            instructionsBatch.DrawRectangle(item.Position, item.Size.Width, item.Size.Height, item.Color);
                            break;
                        case DebugInformation.RepresentationType.TEXT:
                            instructionsBatch.DrawString(debugFont, item.Text, item.Position, item.Color);
                            break;
                        case DebugInformation.RepresentationType.CLEAR:
                            //do nothing;
                            break;
                        default:
                            Log.LogWarning("AgentsManager: unrecognized debugging information type to be represented: " + item.Representation);                            
                            break;
                    }
                }
            }
        }
    }
}
