#region Using Statements
using GeometryFriends.AI;
using GeometryFriends.Components;
using GeometryFriends.Levels;
using GeometryFriends.ScreenSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.IO;
#endregion

namespace GeometryFriends
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 

    class Engine : Game
    {
        ContentManager content;
        ScreenManager screenManager;
        
        public static Boolean quickStartFlag = false;
        public static Boolean loggerStartFlag = false;
        public static Boolean timeControlFlag = false;
        public static Boolean newResultsFileFlag = false;

        public static StreamWriter logger_circle = null;
        public static StreamWriter logger_rect = null;

        public const int DEFAULT_SIMULATION_COUNT = 1;

        protected AgentDebugPane agentPane;
        public AgentDebugPane AgentPane
        {
            get
            {
                return this.agentPane;
            }
        }

        protected InGameConsole console;
        public InGameConsole Console
        {
            get
            {
                return this.console;
            }
        }

        protected OnScreenKeyboard keyboard;
        public OnScreenKeyboard ScreenKeyboard
        {
            get
            {
               return this.keyboard;
            }

        }

        // capture: variables
        private bool capture = false;
        private int imgcount = 0;
        private TimeSpan last_capture_timestamp = TimeSpan.Zero;
        private float capture_interval = 1000 / 24;  // 24 fps. maybe let the user set this in the future

        public Engine(InteractivePlatform interactivePlatform, bool fixedTimeStep = true, float gameSpeed = Level.DEFAULT_GAME_SPEED, int simulationsCount = DEFAULT_SIMULATION_COUNT, bool capture = false)
            : base(interactivePlatform)
        {
            content = new ContentManager();

            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 10); // Value previous to cross-platform migration
            this.IsFixedTimeStep = fixedTimeStep;

            // capture: evaluate constructor parameter
            this.capture = capture;

            // capture: create directory 'Capture' if it doesn't exist yet
            if (capture) {
                System.IO.Directory.CreateDirectory ("Capture");
            }

            //setup the game speed
            Level.GameSpeed = gameSpeed;

            // capture: adjust capture interval having game speed into consideration
            // commented out because it still doesn't work very well with sped up games
            // capture_interval = capture_interval / gameSpeed;

            agentPane = new AgentDebugPane(GraphicsDevice);
            agentPane.DrawOrder = 103;
            Components.Add(agentPane);

            keyboard = new OnScreenKeyboard(GraphicsDevice);
            keyboard.DrawOrder = 101;
            Components.Add(keyboard);

            //new-up components and add to Game.Components
            screenManager = new ScreenManager(this, keyboard, console, agentPane);
            Components.Add(screenManager);

            FrameRateCounter frameRateCounter = new FrameRateCounter(GraphicsDevice);
            frameRateCounter.DrawOrder = 101;
            Components.Add(frameRateCounter);

            console = new InGameConsole(GraphicsDevice);
            console.DrawOrder = 101;
            Components.Add(console);            

            // Start here!
            string[] commandLine = Program.quickStartup;
            if (commandLine.Length > 0)
            {
                //special case for specifying the agent implementation to run
                if (commandLine.Length > 1)
                {
                    if (commandLine[commandLine.Length - 2].Equals("-a")) //a is trigger for loading a specific agent implementation from the last command line parameter
                    { 
                        string agentDllPath = commandLine[commandLine.Length - 1];
                        if(!agentDllPath.EndsWith(".dll")){
                            Log.LogRaw("Commandline trigger '-a' was specified without the last argument being a path for a dll file to be loaded.");
                        }
                        else if(!File.Exists(agentDllPath)){
                            Log.LogRaw("The dll file specified does not exist or the specified path is incorrect.");
                        }
                        else{
                            //check if the dll file is already in the agents folder of the game
                            Uri gameAgentsFolder = new Uri(AgentsManager.GetAgentsDLLDirectory());
                            Uri specifiedDllFolder;
                            if (Path.IsPathRooted(agentDllPath))
                            {//is absolute path
                                specifiedDllFolder = new Uri(Path.GetDirectoryName(agentDllPath));
                            }
                            else
                            {//is relative path
                                specifiedDllFolder = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(agentDllPath)));
                            }

                            //if the dll file is not in the agents folder, put it there
                            if (gameAgentsFolder != specifiedDllFolder)
                            {                            
                                string destFile = Path.Combine(AgentsManager.GetAgentsDLLDirectory(), Path.GetFileName(agentDllPath));
                                File.Copy(agentDllPath, destFile, true);
                            }
                            //specify this agent implementation as the one to be loaded
                            GeometryFriends.Properties.Settings.Default.AgentsImplementation = AgentsManager.GetHumanReadableDllID(Path.GetFileNameWithoutExtension(agentDllPath));
                        }
                    }
                }

                if (commandLine[0].Equals("-s"))
                {   
                    quickStartFlag = true;

                    //completely disable sound
                    SoundEffect.SoundDisabled = true;

                    Func<GameScreen> simulationScreenCreator = () => 
                    {
                        return CreateSinglePlayerScreen(Convert.ToInt32(commandLine[2]), Convert.ToInt32(commandLine[1]));
                    };

                    screenManager.AddScreen(new SimulationScreen(simulationsCount, simulationScreenCreator));
                }
                else if (commandLine[0].Equals("-m"))
                {
                    quickStartFlag = true;
                    GameScreen levelScreen = CreateMultiPlayerScreen(Convert.ToInt32(commandLine[2]), Convert.ToInt32(commandLine[1]));
                    if(levelScreen == null)
                    {
                        throw new InvalidArgumentsException("Invalid Arguments when creating multiplayer level.");
                    }
                    else
                    {
                        screenManager.AddScreen(levelScreen);
                    }
                }
                else if (commandLine[0].Equals("-st"))
                {
                    quickStartFlag = true;
                    timeControlFlag = true;

                    //completely disable sound
                    SoundEffect.SoundDisabled = true;

                    Func<GameScreen> simulationScreenCreator = () =>
                    {
                        return CreateSinglePlayerScreen(Convert.ToInt32(commandLine[2]), Convert.ToInt32(commandLine[1]));
                    };

                    screenManager.AddScreen(new SimulationScreen(simulationsCount, simulationScreenCreator));
                }
                else if (commandLine[0].Equals("-stn"))
                {
                    quickStartFlag = true;
                    timeControlFlag = true;
                    newResultsFileFlag = true;

                    //completely disable sound
                    SoundEffect.SoundDisabled = true;

                    Func<GameScreen> simulationScreenCreator = () =>
                    {
                        return CreateSinglePlayerScreen(Convert.ToInt32(commandLine[2]), Convert.ToInt32(commandLine[1]));
                    };

                    screenManager.AddScreen(new SimulationScreen(simulationsCount, simulationScreenCreator));
                }
                else if (commandLine[0].Equals("-l"))
                {
                    //Logger here
                    // Directory Creation
                    string circleLogfileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogCore.LOG_FOLDER, "Circle");
                    if (!Directory.Exists(circleLogfileDir))
                        Directory.CreateDirectory(circleLogfileDir);

                    // Directory Creation
                    string rectangleLogfileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogCore.LOG_FOLDER, "Rectangle");
                    if (!Directory.Exists(rectangleLogfileDir))
                        Directory.CreateDirectory(rectangleLogfileDir);

                    //completely disable sound
                    SoundEffect.SoundDisabled = true;

                    Func<GameScreen> simulationScreenCreator = () =>
                    {
                        // StreamWriter Creation
                        string path_circle = circleLogfileDir + Path.DirectorySeparatorChar + DateTime.Now.Day + "-" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".txt";
                        string path_rect = rectangleLogfileDir + Path.DirectorySeparatorChar + DateTime.Now.Day + "-" + DateTime.Now.Hour + "_" + DateTime.Now.Minute + "_" + DateTime.Now.Second + ".txt";
                        
                        logger_circle = new StreamWriter(path_circle);
                        logger_rect = new StreamWriter(path_rect);
  
                        return CreateSinglePlayerScreen(Convert.ToInt32(commandLine[2]), Convert.ToInt32(commandLine[1]));
                    };

                    quickStartFlag = true;
                    loggerStartFlag = true;                    
                    screenManager.AddScreen(new SimulationScreen(simulationsCount, simulationScreenCreator));
                }
                else
                {
                    throw new InvalidArgumentsException("Invalid Arguments");
                }
            }
            else //no commandline arguments
            {
                screenManager.AddScreen(new LogoScreen());
            }
        }

        private GameScreen CreateSinglePlayerScreen(int level, int world)
        {
            try
            {
                return new SinglePlayerLevel(level, world, agentPane, SinglePlayerLevel.AgentsToRun.Both);             
            }
            catch (Exception e)
            {
                Log.LogError("Could not launch SinglePlayerLevel for world " + world + " for level " + level);
                Log.LogError(e.StackTrace);
                return null;
            }
        }

        private GameScreen CreateMultiPlayerScreen(int level, int world)
        {
            try
            {
                return new MultiplayerLevel(level, world);
            }
            catch (Exception e)
            {
                Log.LogError("Could not launch MultiPlayerLevel for world " + world + " for level " + level);
                Log.LogError(e.StackTrace);
                return null;
            }
        }

        public ScreenManager ScreenManager
        {
            get
            {
                return this.screenManager;
            }
            set
            {
                this.screenManager = value;
            }
        }

        public bool updatedOnce = false;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            /*if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();*/

            // capture: take screenshots periodically and save them locally
            if (capture && (gameTime.TotalGameTime - last_capture_timestamp).TotalMilliseconds > capture_interval) {
                System.Drawing.Bitmap bmp = XNAStub.Game.GamePlatform.GraphicsDevice.getScreenshot();
                string image_number = (imgcount++).ToString("D5");
                string filename = "image_" + image_number + ".png";
                bmp.Save(System.IO.Path.Combine("Capture", filename));
                last_capture_timestamp = gameTime.TotalGameTime;
            }

            // TODO: Add your update logic here

            updatedOnce = true;

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            if (!updatedOnce) return;

            instructionsBatch.Clear(Color.SteelBlue);

            base.Draw(gameTime, instructionsBatch);
        }
    }
}
