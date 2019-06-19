using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Levels.Shared;
using GeometryFriends.ScreenSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace GeometryFriends.Levels
{
    internal abstract class XMLLevel : Level
    {
        protected int levelNumber;       
        protected XMLLevelParser parser;
        protected List<String> levelDescription;
        protected List<RectanglePlatform> greenObstacles, blackObstacles, yellowObstacles;
        protected List<Elevator> greenElevators, orangeElevators;

        protected bool levelFinished;

        public XMLLevel()
        {
            parser = new XMLLevelParser();
            levelDescription = new List<String>();
            greenObstacles = new List<RectanglePlatform>();
            blackObstacles = new List<RectanglePlatform>();
            yellowObstacles = new List<RectanglePlatform>();
            greenElevators = new List<Elevator>();
            orangeElevators = new List<Elevator>();

            levelFinished = false;
        }

        public XMLLevel(int levelNumber)
        {
            this.levelNumber = levelNumber;
            parser = new XMLLevelParser();
            parser.LoadLevel(this.levelNumber);
            greenObstacles = new List<RectanglePlatform>();
            blackObstacles = new List<RectanglePlatform>();
            yellowObstacles = new List<RectanglePlatform>();
            greenElevators = new List<Elevator>();
            orangeElevators = new List<Elevator>();

            levelFinished = false;
        }

        // Test Code 1
        public XMLLevel(int levelNumber, int worldNumber)
        {
            XMLLevelParser.LoadXMLDocument();
            XMLLevelParser.ChangeWorldTo(worldNumber);
            this.levelNumber = levelNumber;
            parser = new XMLLevelParser();
            parser.LoadLevel(this.levelNumber);
            greenObstacles = new List<RectanglePlatform>();
            blackObstacles = new List<RectanglePlatform>();
            yellowObstacles = new List<RectanglePlatform>();
            greenElevators = new List<Elevator>();
            orangeElevators = new List<Elevator>();

            levelFinished = false;
        }

        public override bool LevelPassed()
        {
            Boolean passed;

            if (Engine.timeControlFlag) {
                if ((elapsedGameTime < XMLLevelParser.GetTimeLimit() && numberOfTrianglesCollected != collectibles.Count) || levelFinished) {
                    passed = false;
                } else {
                    passed = true;
                }
            } else {
                if (numberOfTrianglesCollected != collectibles.Count || levelFinished) {
                    passed = false;
                } else {
                    passed = true;
                }
            }

            if (!passed) {
                return passed;
            } else {
                ScreenManager.levelMusicInstance.Stop();
                if (GeometryFriends.Properties.Settings.Default.musicOn && !Engine.quickStartFlag)
                    ScreenManager.victoryMusicInstance.Play();
                levelFinished = true;
                //MojoLog.Instance().WriteLine("FINISH " + timeElapsed);
                return true;
            }
        }

        public override string GetHighScore()
        {
            return parser.GetHighScoreList();
        }

        public override bool IsNewRecord(double time)
        {
            return parser.IsNewRecord(time, ScreenManager);
        }

        public override int GetLevelNumber()
        {
            return parser.GetLevelNumber();
        }

        public override string GetTitle()
        {
            return "Level " + parser.GetLevelNumber();
        }

        public override string GetExtraCongratulationsText()
        {
            return parser.GetExtraCongratulationsText();
        }

        public override string GetMaxStar()
        {
            return parser.GetMaxStar();
        }

        public override string GetDescription()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in levelDescription)
            {
                sb.AppendLine(s);
            }            
            return sb.ToString();
        }

        protected virtual void LoadBorders()
        {
            border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, borderWidth, ScreenManager.ScreenCenter);
            border.Load(base.GetPhysicsSimulator());
        }

        public override void LoadLevelContent()
        {
            LoadBorders();

            this.levelDescription = parser.GetDescription();
            base.LoadCircleCharacter(parser.GetBallCharacter());

            base.LoadRectangleCharacter(parser.GetSquareCharacter());

            if (ScreenManager != null)
            {
                this.infoAreas = parser.GetTips();
                foreach (InfoArea ia in infoAreas)
                {
                    ia.Load(messageLayer);
                }
            }

            this.collectibles = parser.GetCollectibles();
            foreach (TriangleCollectible col in collectibles)
            {
                col.Load(base.GetPhysicsSimulator());
                col.collectedEvent += new TriangleCollectible.CollectibleCaughtHandler(this.IncreaseCollectibleCount);
            }

            this.obstacles = parser.GetObstacles();
            this.greenObstacles = parser.GetGreenObstacles();
            this.yellowObstacles = parser.GetYellowObstacles();
            this.blackObstacles = parser.GetBlackObstacles();

            foreach (RectanglePlatform obst in obstacles)
            {
                obst.Load(base.GetPhysicsSimulator());
            }

            this.elevators = parser.GetElevators();
            this.greenElevators = parser.GetGreenElevators();
            this.orangeElevators = parser.GetOrangeElevators();

            foreach (Elevator e in elevators)
            {
                e.Load(base.GetPhysicsSimulator(), contentManager.Load<SpriteFont>("Content/Fonts/elevatorCollFont.spritefont"));
            }

            //Specific Level Objects
            if (parser.GetSpecialPlatform())
            {
                hasHangingTexture = true;
                hangingTextureWidth = 128;
                hangingTextureHeight = 50;
                hangingTextureBorderThickness = 1;
                hangingTextureColor = GameColors.HANGING_FILL_COLOR;
                hangingTextureBorderColor = GameColors.HANGING_BORDER_COLOR;

                hangingBody = BodyFactory.Instance.CreateRectangleBody(this.GetPhysicsSimulator(), 128, 50, 0.3f);
                hangingBody.Position = new Vector2(1160, 430);
                hangingGeom = GeomFactory.Instance.CreateRectangleGeom(this.GetPhysicsSimulator(), hangingBody, 128, 50);
                fixedLinearSpring1 = ControllerFactory.Instance.CreateFixedLinearSpring(this.GetPhysicsSimulator(), hangingBody, new Vector2(0, -5), new Vector2(1180, 350), 70, 1.3f);
            }            
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


        public override void DrawLevel(DrawingInstructionsBatch instructionsBatch)
        {
            border.Draw(instructionsBatch);
            

            foreach(TriangleCollectible col in collectibles)
            {
                col.Draw(instructionsBatch);
            }

            foreach (RectanglePlatform obst in obstacles)
            {
                obst.Draw(instructionsBatch);
            }

            foreach (Elevator e in elevators)
            {
                e.Draw(instructionsBatch);
            }

            foreach (InfoArea ia in infoAreas)
            {
                ia.Draw(instructionsBatch, this.circle.Body.Position, this.rectangle.Body.Position, ScreenManager.DetailsFont);
            }

            if (hasHangingTexture)
            {
                instructionsBatch.DrawRectangle(new Vector2(hangingGeom.Position.X - hangingTextureWidth / 2, hangingGeom.Position.Y - hangingTextureHeight / 2), hangingTextureWidth, hangingTextureHeight, hangingTextureColor, hangingTextureBorderThickness, hangingTextureBorderColor, hangingGeom.Rotation);
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlNode description = doc.CreateElement("Description");
            XmlNode tips = doc.CreateElement("Tips");
            XmlNode col = doc.CreateElement("Collectibles");
            XmlNode gObs = doc.CreateElement("GreenObstacles");
            XmlNode bObs = doc.CreateElement("BlackObstacles");
            XmlNode yObs = doc.CreateElement("YellowObstacles");
            XmlNode gElev = doc.CreateElement("GreenElevators");
            XmlNode oElev = doc.CreateElement("OrangeElevators");            

            XmlElement node = doc.CreateElement("Level" + levelNumber);
            XmlElement auxN;

            foreach (String s in this.levelDescription)
            {
                
                auxN = doc.CreateElement("Line");
                auxN.SetAttribute("value", s);
                
                description.AppendChild(auxN);
            }
            node.AppendChild(description);

            foreach (InfoArea ia in infoAreas)
            {
                tips.AppendChild(ia.ToXml(doc));
            }
            node.AppendChild(tips);
            node.AppendChild(this.circle.ToXml(doc));
            node.AppendChild(this.rectangle.ToXml(doc));

            foreach (TriangleCollectible c in this.collectibles)
            {
                col.AppendChild(c.ToXml(doc));
            }
            node.AppendChild(col);

            foreach (RectanglePlatform o in this.greenObstacles)
            {
                gObs.AppendChild(o.ToXml(doc));
            }
            node.AppendChild(gObs);

            foreach (RectanglePlatform o in this.blackObstacles)
            {
                bObs.AppendChild(o.ToXml(doc));
            }
            node.AppendChild(bObs);

            foreach (RectanglePlatform o in this.yellowObstacles)
            {
                yObs.AppendChild(o.ToXml(doc));
            }
            node.AppendChild(yObs);

            foreach (Elevator e in this.greenElevators)
            {
                gElev.AppendChild(e.ToXml(doc));
            }
            node.AppendChild(gElev);

            foreach (Elevator e in this.orangeElevators)
            {
                oElev.AppendChild(e.ToXml(doc));
            }
            node.AppendChild(oElev);
            return node;
        }

    }
}
