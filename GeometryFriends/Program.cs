using GeometryFriends.GameViewers;
using GeometryFriends.Levels;
using GeometryFriends.XNAStub;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GeometryFriends
{
    static class Program
    {
        public static string[] quickStartup;
        public const string NO_RENDERING = "--no-rendering";
        public const string DISABLE_FIXED_TIME_STEP = "--disable-fixed-time-step";
        public const string SET_GAME_SPEED = "--speed";
        public const string SET_SIMULATION_COUNT = "--simulations";
        public const string RUN_BATCH = "--batch-simulator";
        public const string LOG_TO_FILE = "--log-to-file";

        // capture: command line argument
        public const string CAPTURE = "--capture";

        public static bool HasRendering {get; set;}
        public static bool HasFixedTimeStep { get; set; }
        public static bool LogToFile { get; set; }
        public static float GameSpeed { get; set; }
        public static int SimulationsCount { get; set; }

        // capture: flag indicating whether or not to capture
        public static bool capture = false;

        private static bool runBatch;        
        private static InteractivePlatform iPlatform;

        private static bool compatibleTextRenderingDefaultDone = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            HasRendering = true;
            HasFixedTimeStep = true;
            GameSpeed = Level.DEFAULT_GAME_SPEED;
            SimulationsCount = Engine.DEFAULT_SIMULATION_COUNT;
            runBatch = false;
            LogToFile = false;
            iPlatform = null;

            //setup cmd line arguments
            quickStartup = PrepareGeometryFriendsArguments(Environment.GetCommandLineArgs());

            if (runBatch)
            {
                //prepare the viewer
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                GeometryFriendsBatchSimulator batchViewer = new GeometryFriendsBatchSimulator();
                //start viewer
                Application.Run(batchViewer);
            }
            else if (HasRendering)
            {
                StartGameEngineWithRendering();
            }
            else
            {
                StartGameEngineWithoutRendering();
            }
        }

        internal static Engine StartGameEngineWithRendering()
        {
            //setup visual display of messages
            Log.ShowVisualMessage = new Action<string>(ShowMessageToPlayer);

            //prepare the viewer
            Application.EnableVisualStyles();
            if (!compatibleTextRenderingDefaultDone)
            {
                Application.SetCompatibleTextRenderingDefault(false);
                compatibleTextRenderingDefaultDone = true;
            }
            GeometryFriendsViewer gameViewer = new GeometryFriendsViewer();
            iPlatform = new InteractivePlatform(gameViewer, gameViewer);
            gameViewer.Platform = iPlatform;

            //start game engine
            Engine game = StartGameEngine();

            //start viewer
            Application.Run(gameViewer);

            return game;
        }

        internal static Engine StartGameEngineWithoutRendering()
        {
            //console mode execution
            Engine game = StartGameEngine();

            //prepare the viewer
            GeometryFriendsSimulationController simulationController = new GeometryFriendsSimulationController(game);

            //start controller
            Application.Run(simulationController);

            return game;
        }

        private static Engine StartGameEngine()
        {
            //apply log to file configuration
            Log.LogToFile = LogToFile;

            //start game engine
            Engine game = LoadGameEngine(iPlatform, HasFixedTimeStep, GameSpeed, SimulationsCount);
            Thread gameThread = new Thread(game.Run);
            gameThread.IsBackground = true;
            gameThread.Start();

            return game;
        }

        private static Engine LoadGameEngine(InteractivePlatform platform, bool fixedStep, float speed, int simulationCount)
        {
            try
            {
                // capture: passed the argument to the game constructor
                return new Engine (platform, fixedStep, speed, SimulationsCount, capture);
            }
            catch (InvalidArgumentsException)
            {
                ShowGameCommandLineArguments();
                Environment.Exit(0);
                return null;
            }
        }

        public static string[] PrepareGeometryFriendsArguments(string[] initialArguments)
        {
            //remove first element that contains the path to the executable so it is compatible with the previous cmd loading method
            List<string> listArgs = initialArguments.ToList();
            listArgs.RemoveAt(0);

            //check for writting the log file
            if (listArgs.Contains(LOG_TO_FILE))
            {
                LogToFile = true;
                listArgs.Remove(LOG_TO_FILE);
            }

            //check for running the batch simulator
            if (listArgs.Contains(RUN_BATCH))
            {
                runBatch = true;
                listArgs.Remove(RUN_BATCH);
            }

            //check for no rendering trigger
            if (listArgs.Contains(NO_RENDERING))
            {
                HasRendering = false;
                listArgs.Remove(NO_RENDERING);
            }

            //check for disable fixed time step
            if (listArgs.Contains(DISABLE_FIXED_TIME_STEP))
            {
                HasFixedTimeStep = false;
                listArgs.Remove(DISABLE_FIXED_TIME_STEP);
            }

            //check for speed configuration
            if (listArgs.Contains(SET_GAME_SPEED))
            {
                //check that following it there is a float number
                int triggerIndex = listArgs.IndexOf(SET_GAME_SPEED);
                if (triggerIndex + 1 >= listArgs.Count)
                {
                    //number is missing
                    Log.LogWarning(GetSpeedTriggerMissingValueError());
                }
                else
                {
                    //try to get the speed value
                    try
                    {
                        string toParse = listArgs[triggerIndex + 1];
                        //try to take into account cases where the decimal separator is a comma
                        toParse = toParse.Replace(",", ".");
                        // uses format with . as decimal separator, no matter what the current culture is.
                        GameSpeed = float.Parse(toParse, CultureInfo.InvariantCulture);
                        if (GameSpeed <= 0){
                            Log.LogWarning("The game speed given must be a value greater than 0. Speed reset to the default value: " + Level.DEFAULT_GAME_SPEED, true);
                            GameSpeed = Level.DEFAULT_GAME_SPEED;
                        }
                        listArgs.RemoveAt(triggerIndex + 1);
                    }
                    catch (Exception)
                    {
                        Log.LogWarning(GetSpeedTriggerMissingValueError());
                    }
                }
                listArgs.Remove(SET_GAME_SPEED);
            }

            //check for simulations configuration
            if (listArgs.Contains(SET_SIMULATION_COUNT))
            {
                //check that following it there is an integer number
                int triggerIndex = listArgs.IndexOf(SET_SIMULATION_COUNT);
                if (triggerIndex + 1 >= listArgs.Count)
                {
                    //number is missing
                    Log.LogWarning(GetSimulationsTriggerMissingValueError());
                }
                else
                {
                    //try to get the simulations count
                    try
                    {
                        string toParse = listArgs[triggerIndex + 1];
                        SimulationsCount = int.Parse(toParse);
                        if (SimulationsCount <= 0)
                        {
                            Log.LogWarning("The simulations count given must be a value greater than 0. Simulations count reset to the default value 1.", true);
                            SimulationsCount = 1;
                        }
                        listArgs.RemoveAt(triggerIndex + 1);
                    }
                    catch (Exception)
                    {
                        Log.LogWarning(GetSimulationsTriggerMissingValueError());
                    }
                }
                listArgs.Remove(SET_SIMULATION_COUNT);
            }

            // capture: check for existance of command line argument
            if (listArgs.Contains (CAPTURE)) {
                capture = true;
                listArgs.Remove (CAPTURE);
            }

            return listArgs.ToArray();
        }

        public static string GetSpeedTriggerMissingValueError()
        {
            return "Speed trigger '" + SET_GAME_SPEED + "' must always be followed by a float number. Ignoring this trigger.";
        }

        public static string GetSimulationsTriggerMissingValueError()
        {
            return "Simulations trigger '" + SET_SIMULATION_COUNT + "' must always be followed by an integer number greater than 0. Ignoring this trigger.";
        }

        public static void ShowGameCommandLineArguments()
        {
            Log.LogError("Invalid arguments. Check proper arguments below: \n\n" +
                "\tGeometryFriends.exe" +
                "\n\t\t[ " + NO_RENDERING + " ]" +
                "\n\t\t[ " + DISABLE_FIXED_TIME_STEP + " ]" +
                "\n\t\t[ " + LOG_TO_FILE + " ]" +
                "\n\t\t[ " + CAPTURE + " ]" +
                "\n\t\t[ " + SET_GAME_SPEED + " speed_value ]" +
                "\n\t\t[ " + SET_SIMULATION_COUNT + " simulation_count ]" +
                "\n\t\t[ -m | -s | -st | -stn | -l world_number level_number ]" +
                "\n\t\t[ -a agent_dll_path ]",
                true);            
        }

        public static void ShowMessageToPlayer(string message)
        {
            MessageBox.Show(message);
        }
    }
}
