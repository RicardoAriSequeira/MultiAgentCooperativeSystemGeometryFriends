using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Levels.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace GeometryFriends.Levels
{
    internal class XMLLevelParser
    {
        private int MAX_NUMBER_OF_HIGHSCORE_RECORDS = 5;

        public static string LEVELS_FOLDER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Levels");
        private static string CONTENT_LEVELS_FOLDER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Levels");

        private const string LEVELS_NODE_ID = "/Levels";

        static int nLevels = -1;
        static String prevWorldName;
        static String currentWorldName;
        static String nextWorldName;

        CircleCharacter ball;
        RectangleCharacter square;
        static protected FileInfo[] worlds;
        static int currentWorld;
        List<InfoArea> infoAreas;
        List<String> levelDescription;
        List<TriangleCollectible> collectibles;
        List<RectanglePlatform> greenObstacles, blackObstacles, yellowObstacles;
        List<Elevator> greenElevators, orangeElevators;
        List<KeyValuePair<string, int>> highScoreList;
        List<Dictionary<string, object>> medalsList;

        ScreenSystem.ScreenManager screenManager;

        private const string DEFAULT_PLAYER_NAME = "No Name";
        private static string playerName;
        private static double timeLimit;
        private int playerScore;

        int levelNumber;
        public static XmlDocument xmlDocument;

        public struct LevelInformation
        {
            public double timeLimit;

            public LevelInformation(double timeLimit)
            {
                this.timeLimit = timeLimit;
            }
        }

        public XMLLevelParser()
        {
            playerName = DEFAULT_PLAYER_NAME;

            levelDescription = new List<String>();
            infoAreas = new List<InfoArea>();
            collectibles = new List<TriangleCollectible>();
            greenObstacles = new List<RectanglePlatform>();
            blackObstacles = new List<RectanglePlatform>();
            yellowObstacles = new List<RectanglePlatform>();
            greenElevators = new List<Elevator>();
            orangeElevators = new List<Elevator>();

            highScoreList = new List<KeyValuePair<string, int>>();
            medalsList = new List<Dictionary<string, object>>();            
        }

        public static int CurrentWorldNumberOfLevels()
        {
            if (xmlDocument == null)
            {
                LoadXMLDocument();
            }

            return GetWorldNumberOfLevels(xmlDocument);
        }

        public static int GetWorldNumberOfLevels(XmlDocument world)
        {
            XmlNodeList nodeList = world.GetElementsByTagName("BallStartingPosition");
            return nodeList.Count;            
        }

        public static Dictionary<string, List<LevelInformation>> GetAllWorldsLevelsStructure()
        {
            Dictionary<string, List<LevelInformation>> worldsAndLevels = new Dictionary<string, List<LevelInformation>>();

            worldsAndLevels = new Dictionary<string, List<LevelInformation>>();

            if (worlds == null)
            {
                LoadWorlds();
            }

            XmlDocument worldXmlDocument = new XmlDocument();
            string worldName;
            List<LevelInformation> levelsInformation = new List<LevelInformation>();
            int levelCount;
            foreach (FileInfo item in worlds)
            {
                worldName = GetWorldName(item);
                try
                {
                    levelsInformation = new List<LevelInformation>();
                    worldXmlDocument.Load(item.FullName);
                    levelCount = GetWorldNumberOfLevels(worldXmlDocument);
                    for (int i = 0; i < levelCount; i++)
                    {
                        levelsInformation.Add(new LevelInformation(GetSpecificWorldLevelTimeLimit(worldXmlDocument, i + 1)));
                    }
                }
                catch (Exception)
                {
                    Log.LogWarning("Could not load world file: " + item.FullName);
                    worldsAndLevels.Add(worldName, levelsInformation);
                    continue;
                }
                //worldsAndLevels.Add(worldName, GetWorldNumberOfLevels(worldXmlDocument));
                worldsAndLevels.Add(worldName, levelsInformation);
            }

            return worldsAndLevels;
        }

        public static void LoadWorlds()
        {
            //if the Levels directory does not exist, populate one with all the level examples that we have available from content
            if (!Directory.Exists(LEVELS_FOLDER) && Directory.Exists(CONTENT_LEVELS_FOLDER))
            {
                Directory.CreateDirectory(LEVELS_FOLDER);
                string[] files = Directory.GetFiles(CONTENT_LEVELS_FOLDER);
                string fileName, destFile;

                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    fileName = Path.GetFileName(s);
                    destFile = Path.Combine(LEVELS_FOLDER, fileName);
                    File.Copy(s, destFile, true);
                }
            }
            //now load all the worlds
            DirectoryInfo di = new DirectoryInfo(LEVELS_FOLDER);
            worlds = di.GetFiles("*.xml");
            for (int i = 0; i < worlds.Length; ++i)
            {
                if (LEVELS_FOLDER + Path.DirectorySeparatorChar + worlds[i].Name == LEVELS_FOLDER + Path.DirectorySeparatorChar + Properties.Settings.Default.levelsfile)
                {
                    currentWorld = i;
                    break;
                }
            }
        }

        /// <summary>
        /// loads a new XMLDocument
        /// </summary>
        /// 
        public static void LoadXMLDocument()
        {
            xmlDocument = new XmlDocument();
            if (worlds == null)
            {
                LoadWorlds();
            }

            bool errorLoading;
            do
            {
                errorLoading = false;
                try
                {
                    xmlDocument.Load(worlds[currentWorld].FullName);
                }
                catch (Exception)
                {
                    errorLoading = true;
                    currentWorld++;
                    currentWorld %= worlds.Length; 
                }
            } while (errorLoading);
            XmlNode node = xmlDocument.SelectSingleNode(LEVELS_NODE_ID);
            //try
            //{
            //    XMLLevelParser.worldName = node.Attributes.GetNamedItem("worldName").Value.ToString();
            //}
            //catch (Exception)
            //{
            if(currentWorld > 0)
                XMLLevelParser.prevWorldName = GetWorldName(worlds[(currentWorld - 1) % worlds.Length]);
            else
                XMLLevelParser.prevWorldName = GetWorldName(worlds[worlds.Length - 1]);
            XMLLevelParser.currentWorldName = GetWorldName(worlds[currentWorld]);
            XMLLevelParser.nextWorldName = GetWorldName(worlds[(currentWorld + 1) % worlds.Length]);
            //}
        }

        public static string GetWorldName(FileInfo worldFile)
        {
            return Path.GetFileNameWithoutExtension(worldFile.Name);
        }

        public int GetLevelNumber()
        {
            return this.levelNumber;
        }

        public static double GetSpecificWorldLevelTimeLimit(XmlDocument worldXML, int level)
        {
            //Read XML File
            XmlNode node;
            String xPathPrefix = @"/Levels/Level" + level;

            //Time Limit
            int parsed;
            try
            {
                node = worldXML.SelectSingleNode(xPathPrefix + "/TimeLimit");
                parsed = int.Parse(node.Attributes.GetNamedItem("value").Value);
            }
            catch (Exception)
            {   
                return double.PositiveInfinity;
            }
            return Convert.ToDouble(parsed);
        }

        public void LoadLevel(int lvlNum)
        {
            this.levelNumber = lvlNum;

            if (xmlDocument == null)
            {
                LoadXMLDocument();
            }

            try
            {
                //Read XML File
                XmlNodeList nodeList;
                XmlNode node;
                String xPathPrefix = @"/Levels/Level" + levelNumber;                

                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/Description/Line");
                foreach (XmlNode n in nodeList)
                {
                    levelDescription.Add(n.Attributes.GetNamedItem("value").Value);
                }

                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/Tips/Tip");
                foreach (XmlNode n in nodeList)
                {
                    infoAreas.Add(new InfoArea(n.Attributes.GetNamedItem("message").Value, n.Attributes.GetNamedItem("tip").Value, new Vector2(float.Parse(n.Attributes.GetNamedItem("X").Value), float.Parse(n.Attributes.GetNamedItem("Y").Value)), int.Parse(n.Attributes.GetNamedItem("radius").Value)));
                }

                node = xmlDocument.SelectSingleNode(xPathPrefix + "/BallStartingPosition");
                this.ball = new CircleCharacter(new Vector2(float.Parse(node.Attributes.GetNamedItem("X").Value), float.Parse(node.Attributes.GetNamedItem("Y").Value)));


                node = xmlDocument.SelectSingleNode(xPathPrefix + "/SquareStartingPosition");
                this.square = new RectangleCharacter(new Vector2(float.Parse(node.Attributes.GetNamedItem("X").Value), float.Parse(node.Attributes.GetNamedItem("Y").Value)));

                //Collectibles
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/Collectibles/Collectible");

                foreach (XmlNode n in nodeList)
                {
                    collectibles.Add(new TriangleCollectible(new Vector2(float.Parse(n.Attributes.GetNamedItem("X").Value), float.Parse(n.Attributes.GetNamedItem("Y").Value))));
                }

                //BlackObstacles
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/BlackObstacles/Obstacle");

                foreach (XmlNode n in nodeList)
                {
                    blackObstacles.Add(new RectanglePlatform(
                                    int.Parse(n.Attributes.GetNamedItem("width").Value),
                                    int.Parse(n.Attributes.GetNamedItem("height").Value),
                                    new Vector2(float.Parse(n.Attributes.GetNamedItem("X").Value),
                                                float.Parse(n.Attributes.GetNamedItem("Y").Value)),
                                    bool.Parse(n.Attributes.GetNamedItem("centered").Value),
                                    GameColors.BLACK_OBSTACLE_COLOR,
                                    GameColors.BLACK_OBSTACLE_COLOR, 0));
                }

                //GreenObstacles
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/GreenObstacles/Obstacle");

                foreach (XmlNode n in nodeList)
                {
                    greenObstacles.Add(new RectanglePlatform(
                                    int.Parse(n.Attributes.GetNamedItem("width").Value),
                                    int.Parse(n.Attributes.GetNamedItem("height").Value),
                                    new Vector2(float.Parse(n.Attributes.GetNamedItem("X").Value),
                                                float.Parse(n.Attributes.GetNamedItem("Y").Value)),
                                    bool.Parse(n.Attributes.GetNamedItem("centered").Value),
                                    GameColors.YELLOW_OBSTACLE_COLOR,
                                    GameColors.YELLOW_OBSTACLE_COLOR, 0));
                }

                //YellowObstacles
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/YellowObstacles/Obstacle");

                foreach (XmlNode n in nodeList)
                {
                    yellowObstacles.Add(new RectanglePlatform(
                                    int.Parse(n.Attributes.GetNamedItem("width").Value),
                                    int.Parse(n.Attributes.GetNamedItem("height").Value),
                                    new Vector2(float.Parse(n.Attributes.GetNamedItem("X").Value),
                                                float.Parse(n.Attributes.GetNamedItem("Y").Value)),
                                    bool.Parse(n.Attributes.GetNamedItem("centered").Value),
                                    GameColors.GREEN_OBSTACLE_COLOR,
                                    GameColors.GREEN_OBSTACLE_COLOR, 0));
                }

                //GreenElevators
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/GreenElevators/Elevator");

                foreach (XmlNode n in nodeList)
                {
                    greenElevators.Add(new Elevator(
                                    int.Parse(n.Attributes.GetNamedItem("width").Value),
                                    int.Parse(n.Attributes.GetNamedItem("height").Value),
                                    new Vector2(float.Parse(n.Attributes.GetNamedItem("startX").Value),
                                                float.Parse(n.Attributes.GetNamedItem("startY").Value)),
                                    new Vector2(float.Parse(n.Attributes.GetNamedItem("endX").Value),
                                                float.Parse(n.Attributes.GetNamedItem("endY").Value)),
                                    n.Attributes.GetNamedItem("direction").Value,
                                    int.Parse(n.Attributes.GetNamedItem("collNeeded").Value),
                                    bool.Parse(n.Attributes.GetNamedItem("repeatMov").Value),
                                    GameColors.GREEN_ELEVATOR_COLOR,
                                    GameColors.GREEN_ELEVATOR_BORDER_COLOR, 0));
                }

                //OrangeElevators
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/OrangeElevators/Elevator");

                foreach (XmlNode n in nodeList)
                {
                    orangeElevators.Add(new Elevator(
                                    int.Parse(n.Attributes.GetNamedItem("width").Value),
                                    int.Parse(n.Attributes.GetNamedItem("height").Value),
                                    new Vector2(float.Parse(n.Attributes.GetNamedItem("startX").Value),
                                                float.Parse(n.Attributes.GetNamedItem("startY").Value)),
                                    new Vector2(float.Parse(n.Attributes.GetNamedItem("endX").Value),
                                                float.Parse(n.Attributes.GetNamedItem("endY").Value)),
                                    n.Attributes.GetNamedItem("direction").Value,
                                    int.Parse(n.Attributes.GetNamedItem("collNeeded").Value),
                                    bool.Parse(n.Attributes.GetNamedItem("repeatMov").Value),
                                    GameColors.YELLOW_ELEVATOR_COLOR,
                                    GameColors.YELLOW_ELEVATOR_BORDER_COLOR, 0));
                }

                //HighScore
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/HighScores/HighScore");

                string name;
                foreach (XmlNode n in nodeList)
                {
                    name = n.Attributes.GetNamedItem("name").Value;
                    highScoreList.Add(new KeyValuePair<string, int>(
                        name == null ? "" : name,
                        int.Parse(n.Attributes.GetNamedItem("value").Value)));
                }

                //Medals
                nodeList = xmlDocument.SelectNodes(xPathPrefix + "/Medals/Medal");

                foreach (XmlNode n in nodeList)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    try
                    {
                        dic.Add("star", n.Attributes.GetNamedItem("star").Value);
                    }
                    catch (Exception)
                    {
                        dic.Add("star", "none");
                    }
                    try
                    {
                        dic.Add("desc", n.Attributes.GetNamedItem("desc").Value);
                    }
                    catch (Exception)
                    {
                        dic.Add("desc", "");
                    }
                    try
                    {
                        dic.Add("minvalue", int.Parse(n.Attributes.GetNamedItem("minvalue").Value));
                    }
                    catch (Exception)
                    {
                        dic.Add("minvalue", 0);
                    }
                    try
                    {
                        dic.Add("maxvalue", int.Parse(n.Attributes.GetNamedItem("maxvalue").Value));
                    }
                    catch (Exception)
                    {
                        dic.Add("maxvalue", int.MaxValue);
                    }
                    medalsList.Add(dic);
                }

                //Time Limit
                try
                {
                    node = xmlDocument.SelectSingleNode(xPathPrefix + "/TimeLimit");
                    ChangeTimeLimit(int.Parse(node.Attributes.GetNamedItem("value").Value));
                }
                catch (Exception)
                {
                    Log.LogWarning("Exception handling: No existing time limit for level, setting infinity.");
                    ChangeTimeLimit(double.PositiveInfinity);
                }
            }
            catch (Exception e)
            {
                Log.LogError("Required level does not exist or there is a problem with the level definition: " + e.Message);
                Log.LogError(e.StackTrace);
                throw e;
            }
        }

        private void ChangeTimeLimit(double limit) {
            timeLimit = limit;
        }        

        public void SaveLevel(XmlNode level, int lvlNum)
        {
            XmlElement root;
            XmlNode node;
            String xPathPrefix = @"/Levels";

            //TODO add medals to level

            root = xmlDocument.DocumentElement;
                if(lvlNum <= XMLLevelParser.CurrentWorldNumberOfLevels())
                {                    
                    node = root.SelectSingleNode(xPathPrefix + "/Level" + lvlNum);
                    root.ReplaceChild(level, node);
                }
                else
                {
                    node = root.SelectSingleNode(xPathPrefix);
                    root.AppendChild(level);
                }        
                xmlDocument.Save(worlds[currentWorld].FullName);
                xmlDocument = null;
        }

        public bool GetSpecialPlatform()
        {
            String xPathPrefix = @"/Levels/Level" + levelNumber;
            return (xmlDocument.SelectSingleNode(xPathPrefix + "/SpecialPlatform") != null);
        }

        public List<String> GetDescription()
        {
            return this.levelDescription;
        }

        public string GetHighScoreList()
        {
            string highScoreTextList = "";
            highScoreList.Sort(
                delegate(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
                {
                    return x.Value.CompareTo(y.Value);
                }
            );
            int cont = 0;
            foreach (KeyValuePair<string, int> item in highScoreList)
            {
                if (cont++ < MAX_NUMBER_OF_HIGHSCORE_RECORDS)
                {
                    highScoreTextList += item.Key;
                    for (int i = item.Key.Length; i < 15; i++)
                    {
                        highScoreTextList += " ";
                    }
                    highScoreTextList += " " + item.Value + "\n";
                }
                else { break; }
            }
            return highScoreTextList;
        }

        public string GetMaxStar()
        {
            string starsText = "Stars: ";
            if (medalsList.Count == 0)
                return starsText + "N/A";
            if (highScoreList.Count == 0)
                return "Locked";
            int maxValue = highScoreList[0].Value;
            Dictionary<string, object> erned_medal = new Dictionary<string, object>();
            foreach (Dictionary<string, object> medal in medalsList)
            {
                if (Convert.ToInt32(medal["maxvalue"]) >= maxValue && Convert.ToInt32(medal["minvalue"]) <= maxValue)
                {
                    erned_medal = medal;
                    break;
                }
            }
            return starsText + erned_medal["star"];
        }

        public bool IsNewRecord(double time, ScreenSystem.ScreenManager screenManager)
        {
            this.screenManager = screenManager;
            this.playerScore = (int)time;
            //playerName = DEFAULT_PLAYER_NAME;

            foreach (KeyValuePair<string, int> item in highScoreList)
            {
                if (item.Value > time)
                {
                    //this.screenManager.ScreenKeyboard.Enable(changePlayerName, "Congratulations, new highscore!", 8, "player");
                    ChangePlayerName(playerName, false);
                    return true;
                }
            }
            if (highScoreList.Count < MAX_NUMBER_OF_HIGHSCORE_RECORDS)
            {
                //this.screenManager.ScreenKeyboard.Enable(changePlayerName, "Congratulations, new highscore!", 8, "player");
                ChangePlayerName(playerName, false); 
                return true;
            }
            return false;
        }

        public static void ChangePlayerName(String name){
            playerName = name;
        }

        public static string GetPlayerName()
        {
            return playerName;
        }

        private void ChangePlayerName(String name, Boolean change)
        {
            /*if (change)
                playerName = name;
            else
                playerName = DEFAULT_PLAYER_NAME;
             */

            highScoreList.Add(new KeyValuePair<string, int>(playerName, playerScore));
            highScoreList.Sort(
                delegate( KeyValuePair<string, int> x, KeyValuePair<string, int> y)
                {
                    return x.Value.CompareTo(y.Value);
                }
            );

            //SAVE
            XmlNode levelNode = xmlDocument.SelectSingleNode(@"/Levels/Level" + levelNumber);
            XmlNode highScoresNode = levelNode.SelectSingleNode("HighScores");
            if (highScoresNode == null)
            {
                highScoresNode = xmlDocument.CreateElement("HighScores");
            }
            else
            {
                highScoresNode.RemoveAll();
            }

            foreach (KeyValuePair<string, int> highScore in highScoreList)
            {
                if (highScoresNode.ChildNodes.Count < MAX_NUMBER_OF_HIGHSCORE_RECORDS)
                {
                    XmlElement highScoreNode;
                    highScoreNode = xmlDocument.CreateElement("HighScore");
                    highScoreNode.SetAttribute("name", highScore.Key);
                    highScoreNode.SetAttribute("value", highScore.Value.ToString());

                    highScoresNode.AppendChild(highScoreNode);
                }
                else { break; }
            }
            levelNode.AppendChild(highScoresNode);

            SaveLevel(levelNode, levelNumber);

            //this.screenManager.AddScreen(new GeometryFriends.ScreenSystem.VictoryScreen(playerScore, true, getHighScoreList(), getLevelNumber(), playerName, getExtraCongratulationsText()));

            //playerName = DEFAULT_PLAYER_NAME;
            playerScore = 999;
        }

        public string GetExtraCongratulationsText()
        {
            if (medalsList.Count == 0)
                return "stars: N/A";
            Dictionary<string, object> erned_medal = new Dictionary<string, object>();
            foreach (Dictionary<string, object> medal in medalsList)
            {
                if (Convert.ToInt32(medal["maxvalue"]) >= playerScore && Convert.ToInt32(medal["minvalue"]) <= playerScore)
                {
                    erned_medal = medal;
                    break;
                }
            }
            return "stars: " + erned_medal["star"] + "\n" + erned_medal["desc"];
        }

        public CircleCharacter GetBallCharacter()
        {
            return this.ball;
        }

        public RectangleCharacter GetSquareCharacter()
        {
            return this.square;
        }

        public List<TriangleCollectible> GetCollectibles()
        {
            return this.collectibles;
        }

        public List<InfoArea> GetTips()
        {
            return this.infoAreas;
        }

        public List<RectanglePlatform> GetObstacles()
        {
            List<RectanglePlatform> obs;
            obs = new List<RectanglePlatform>();
            obs.AddRange(blackObstacles);
            obs.AddRange(greenObstacles);
            obs.AddRange(yellowObstacles);
            return obs;
        }

        public List<RectanglePlatform> GetBlackObstacles()
        {
            return this.blackObstacles;
        }

        public List<RectanglePlatform> GetGreenObstacles()
        {
            return this.greenObstacles;
        }

        public List<RectanglePlatform> GetYellowObstacles()
        {
            return this.yellowObstacles;
        }

        public List<Elevator> GetElevators()
        {
            List<Elevator> elev;
            elev = new List<Elevator>();
            elev.AddRange(greenElevators);
            elev.AddRange(orangeElevators);
            return elev;
        }

        public List<Elevator> GetGreenElevators()
        {
            return this.greenElevators;
        }

        public List<Elevator> GetOrangeElevators()
        {
            return this.orangeElevators;
        }

        public static String GetPrevWorldName()
        {
            if (xmlDocument == null)
            {
                LoadXMLDocument();              
            }
            
            return prevWorldName;
        }

        public static String GetWorldName()
        {
            if (xmlDocument == null)
            {
                LoadXMLDocument();              
            }
            
            return currentWorldName;
        }

        public static String GetNextWorldName()
        {
            if (xmlDocument == null)
            {
                LoadXMLDocument();              
            }
            
            return nextWorldName;
        }

        private static void ChangeWorld()
        {
            GeometryFriends.Properties.Settings.Default.levelsfile = worlds[currentWorld].Name;
            GeometryFriends.Properties.Settings.Default.Save();
            GeometryFriends.Properties.Settings.Default.Reload();
            LoadXMLDocument();
        }

        public static void ChangeWorldTo(int world_num)
        {
            if (world_num >= worlds.Length || world_num < 0)
            {
                throw new System.Exception("Wrong Parameters. World does not exist");
            }
            else
            {
                currentWorld = world_num;
                ChangeWorld();
            }
        }

        public static void ChangeToNextWorld()
        {
            if (currentWorld < worlds.Length - 1)
                ++currentWorld;
            else
                currentWorld = 0;

            ChangeWorld();
        }

        public static void ChangeToPreviousWorld()
        {
            if (currentWorld > 0)
                --currentWorld;
            else
                currentWorld = worlds.Length - 1;

            ChangeWorld();
        }

        public static double GetTimeLimit() {
            return timeLimit;
        }
    }
}
