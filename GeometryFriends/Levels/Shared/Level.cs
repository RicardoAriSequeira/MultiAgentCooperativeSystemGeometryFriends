using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.Levels.Shared;
using GeometryFriends.ScreenSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Collections.Generic;
using System.Diagnostics;


namespace GeometryFriends.Levels {
    internal abstract class Level : GameScreen {
        public const int DEFAULT_LEVEL_COMPLETE_BONUS = 200;
        public const int DEFAULT_COLLECTIBLE_CAUGHT_BONUS = 100;
        public const float DEFAULT_GAME_SPEED = 1.0f;
        private const int LINE_THICKNESS = 1;
        static bool debugViewEnabled = false;
        Vector2 TIME_TEXT_POSITION = new Vector2(1100, 10);
        Vector2 COLL_TEXT_POSITION = new Vector2(200, 10);
        protected PhysicsSimulator physicsSimulator;
        protected IGraphicsDevice graphicsDevice;
        PhysicsSimulatorView physicsSimulatorView;
        protected ContentManager contentManager;
        public CircleCharacter circle;
        public RectangleCharacter rectangle;

        public static bool isMulti;
        public static bool isFinished = false;
        
        public delegate void LevelResultsEventHandler(LevelResult result);
        public static event LevelResultsEventHandler LevelResults;
        
        private System.Drawing.Rectangle area;
        public System.Drawing.Rectangle Area
        {
            get {
                return this.area;
            }
        }

        protected double elapsedGameTime;
        protected Stopwatch realtimeCounter;

        protected int numberOfTrianglesCollected;
        protected Border border;
        protected List<TriangleCollectible> collectibles;
        protected List<RectanglePlatform> obstacles;
        protected List<Elevator> elevators;
        protected List<InfoArea> infoAreas;
        protected int borderWidth;
        protected bool hasHangingTexture;
        protected Color hangingTextureColor;
        protected Color hangingTextureBorderColor;
        protected int hangingTextureBorderThickness;
        protected int hangingTextureWidth;
        protected int hangingTextureHeight;
        protected Body hangingBody;
        protected Geom hangingGeom;
        protected FixedLinearSpring fixedLinearSpring1;

        public abstract void LoadLevelContent();
        public abstract void DrawLevel(DrawingInstructionsBatch instructionsBatch);
        public abstract string GetDescription();
        public abstract string GetHighScore();
        public abstract bool IsNewRecord(double time);
        public abstract string GetTitle();
        public abstract bool LevelPassed();
        public abstract int GetLevelNumber();
        public abstract string GetExtraCongratulationsText();
        public abstract string GetMaxStar();
        private SpriteFont spriteFont;

        protected SoundEffect circleJump, circleGrow;
        protected SoundEffect rectangleMorphUp, rectangleMorphDown;
        protected SoundEffectInstance jumpSoundInstance;

        protected SoundEffectInstance growSoundInstance;
        protected SoundEffect circleContract;
        protected SoundEffectInstance contractSoundInstance;
        protected SoundEffectInstance morphUpSoundInstance, morphDownSoundInstance;

        protected InGameMessageLayer messageLayer;

        bool firstRun = true;
        public bool updatedOnce = false;
        FixedLinearSpring mousePickSpring;
        Geom pickedGeom;

        private static float gameSpeed = DEFAULT_GAME_SPEED;
        public static float GameSpeed
        {
            get
            {
                return gameSpeed;
            }
            set
            {
                gameSpeed = value;
            }
        }

        private static int levelCompletedBonus = DEFAULT_LEVEL_COMPLETE_BONUS;
        public static int LevelCompletedBonus
        {
            get
            {
                return levelCompletedBonus;
            }
            set
            {
                levelCompletedBonus = value;
            }
        }

        private static int collectibleCaughtBonus = DEFAULT_COLLECTIBLE_CAUGHT_BONUS;
        public static int CollectibleCaughtBonus
        {
            get
            {
                return collectibleCaughtBonus;
            }
            set
            {
                collectibleCaughtBonus = value;
            }
        }

        public void UpdateElevators() {
            foreach (Elevator e in elevators) {
                e.Update(this.numberOfTrianglesCollected);
            }
        }

        public static bool IsDebugViewEnabled {
            get { return debugViewEnabled; }
        }

        public SpriteFont DebugFont {
            get { return spriteFont; }
        }

        public bool ContentLoaded { get; set; }

        public string EndGameMessage { get; set; }

        public Level() {
            physicsSimulator = new PhysicsSimulator(new Vector2(0, 300));
            physicsSimulator.MaxContactsToDetect = 2; //for stacked objects, simultaneous collision are the bottlenecks so limit them to 2 per geometric pair.
            physicsSimulatorView = new PhysicsSimulatorView(physicsSimulator);
            elapsedGameTime = 0f;
            realtimeCounter = new Stopwatch();
            realtimeCounter.Start();
            numberOfTrianglesCollected = 0;
            elevators = new List<Elevator>();
            collectibles = new List<TriangleCollectible>();
            obstacles = new List<RectanglePlatform>();
            infoAreas = new List<InfoArea>();
            messageLayer = new InGameMessageLayer();
            //infoAreaManager = new InfoAreaManager(physicsSimulator, messageLayer);
            ContentLoaded = false;

            EndGameMessage = "";
        }

        public int GetBorderWidth() {
            return this.borderWidth;
        }

        public List<InfoArea> GetTips() {
            return this.infoAreas;
        }

        public List<TriangleCollectible> GetCollectibles() {
            return this.collectibles;
        }

        public List<RectanglePlatform> GetObstacles() {
            return this.obstacles;
        }

        public List<Elevator> GetElevators() {
            return this.elevators;
        }

        public PhysicsSimulatorView GetPhysicsSimulatorView() {
            return this.physicsSimulatorView;
        }

        public PhysicsSimulator GetPhysicsSimulator() {
            return this.physicsSimulator;
        }

        protected void IncreaseCollectibleCount() {
            this.numberOfTrianglesCollected++;
        }

        public override void HandleInput(InputState input) {
            /*if (!(Engine.timeControlFlag && Engine.quickStartFlag)) {
                if (firstRun) {
                    ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDescription(), GetLevelNumber(), true));
                    firstRun = false;
                }
            }*/
            if (!Engine.quickStartFlag)
            {
                if (firstRun)
                {
                    ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDescription(), GetLevelNumber(), true, this));
                    firstRun = false;
                }
            }
            else
            {
                GeometryFriends.Properties.Settings.Default.SquareHumanControl = false;
                GeometryFriends.Properties.Settings.Default.CircleHumanControl = false;
            }

            if (input.PauseGame) {
                ScreenManager.levelMusicInstance.Pause();
                ScreenManager.AddScreen(new PauseScreen(GetTitle(), GetDescription(), GetLevelNumber(), false, this));
            } else {
                HandleCircleControls(input);
                HandleRectangleControls(input);
                HandleKeyboardInput(input);
                HandleMouseInput(input);
                HandleWiiMoteInput(input);
            }
            base.HandleInput(input);
            //aqui
            circle.SetCollisionState(false);
            rectangle.SetCanStretch(true);
        }

        //bool firstDraw_debug = true;                                            //só foi criado para teste

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            if (!updatedOnce) { return; }

            circle.Draw(instructionsBatch);
            rectangle.Draw(instructionsBatch);

            DrawLevel(instructionsBatch);

            if (mousePickSpring != null) {
                instructionsBatch.DrawLine(mousePickSpring.Body.GetWorldPosition(mousePickSpring.BodyAttachPoint), mousePickSpring.WorldAttachPoint, LINE_THICKNESS, Color.Black);
            }

            if (debugViewEnabled)
            {
                physicsSimulatorView.Draw(instructionsBatch);
                
            }

            instructionsBatch.DrawString(spriteFont, "Time: " + (int)this.elapsedGameTime, TIME_TEXT_POSITION, GameColors.TIME_COLOR);
            instructionsBatch.DrawString(spriteFont, "Caught: " + this.numberOfTrianglesCollected, COLL_TEXT_POSITION, GameColors.CAUGHT_COLOR);

            // if (firstDraw_debug)                                                //só foi criado para teste
            // {                                                                   //só foi criado para teste
            //messageLayer.AddMessage("Teste de mensagem longa", 10000);     //só foi criado para teste
            //     messageLayer.AddMessage("Teste de mensagem curta", 2500);      //só foi criado para teste
            //     messageLayer.AddMessage("Teste de mensagem normal");           //só foi criado para teste
            //     messageLayer.AddMessage("Teste de mensagem curta2", 2500);     //só foi criado para teste
            //     messageLayer.AddMessage("Teste de mensagem normal2");          //só foi criado para teste
            //    messageLayer.AddMessage("Teste de mensagem longa2", 10000);    //só foi criado para teste
            //     infoAreaManager.AddInfoArea("Message", "Tip", new Vector2(600, 725), 50);
            //     firstDraw_debug = false;                                        //só foi criado para teste
            // }                                                                   //só foi criado para teste

            messageLayer.Draw(instructionsBatch, this.elapsedGameTime);
        }

        public void LoadCircleCharacter(CircleCharacter character) {
            this.circle = character;
            circle.Load(physicsSimulator);
        }

        public void LoadRectangleCharacter(RectangleCharacter character) {
            this.rectangle = character;
            rectangle.Load(physicsSimulator);
        }

        public override void LoadContent() {
            if (ContentLoaded)
                return;
            if (contentManager == null) 
                contentManager = new ContentManager();
            graphicsDevice = ScreenManager.GraphicsDevice;

            physicsSimulatorView.LoadContent(graphicsDevice, contentManager);
            borderWidth = (int)(ScreenManager.ScreenHeight * .05f);
            this.area = new System.Drawing.Rectangle(0 + borderWidth, 0 + borderWidth, ScreenManager.ScreenWidth - (borderWidth * 2), ScreenManager.ScreenHeight - (borderWidth * 2));
            this.spriteFont = ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/menuFont.spritefont");

            messageLayer.Load(contentManager);

            circleJump = ScreenManager.ContentManager.Load<SoundEffect>("Content/Sound/spring1.wav");
            jumpSoundInstance = circleJump.CreateInstance();
            circleGrow = ScreenManager.ContentManager.Load<SoundEffect>("Content/Sound/grow.wav");
            growSoundInstance = circleGrow.CreateInstance();
            circleContract = ScreenManager.ContentManager.Load<SoundEffect>("Content/Sound/contract.wav");
            contractSoundInstance = circleContract.CreateInstance();
            rectangleMorphDown = ScreenManager.ContentManager.Load<SoundEffect>("Content/Sound/down.wav");
            morphDownSoundInstance = rectangleMorphDown.CreateInstance();
            rectangleMorphUp = ScreenManager.ContentManager.Load<SoundEffect>("Content/Sound/up.wav");
            morphUpSoundInstance = rectangleMorphUp.CreateInstance();

            LoadLevelContent();
            ContentLoaded = true;
        }

        public override void UnloadContent() {
            contentManager.Unload();
            physicsSimulator.Clear();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
            if (IsActive) {                                 
                //physicsSimulator.Update(gameTime.ElapsedGameTime.Milliseconds * .001f); // -> update previous to cross-platform migration
                for (float i = 0; i <= gameTime.ElapsedGameTime.Milliseconds * GameSpeed * .001f; i += .0005f)
                {
                    physicsSimulator.Update(.0005f);
                    //fix problem with varying simulation step this way (cross-platform implementation)
                    //TODO: fix this at the Engine level
                    if (circle.CurrentSpin == CircleCharacter.Spin.Left || circle.CurrentSpin == CircleCharacter.Spin.Both)
                        circle.SpinLeft();
                    if (circle.CurrentSpin == CircleCharacter.Spin.Right || circle.CurrentSpin == CircleCharacter.Spin.Both)
                        circle.SpinRight();
                    if (rectangle.CurrentSlide == RectangleCharacter.Slide.Left || rectangle.CurrentSlide == RectangleCharacter.Slide.Both)
                        rectangle.SlideLeft();
                    if (rectangle.CurrentSlide == RectangleCharacter.Slide.Right || rectangle.CurrentSlide == RectangleCharacter.Slide.Both)
                        rectangle.SlideRight();
                }
                //reset circle and rectangle 
                circle.CurrentSpin = CircleCharacter.Spin.None;
                rectangle.CurrentSlide = RectangleCharacter.Slide.None;
                elapsedGameTime += gameTime.ElapsedGameTime.TotalMilliseconds * .001f * GameSpeed;
                circle.JumpingDelayInterval += gameTime.ElapsedGameTime.Milliseconds * GameSpeed;
                updatedOnce = true;
                UpdateElevators();

                if (LevelPassed()) {
                    isFinished = true;
                    IsNewRecord(elapsedGameTime);
                    //if(!newRecord)

                    if (Engine.loggerStartFlag) {
                        Engine.logger_circle.Close();
                        Engine.logger_rect.Close();
                    }
                    if (Engine.quickStartFlag && Engine.timeControlFlag) {
                        EndGame();
                    } else {
                        ScreenManager.AddScreen(new VictoryScreen(elapsedGameTime, false, GetHighScore(), GetLevelNumber(), XMLLevelParser.GetPlayerName(), GetExtraCongratulationsText(), this));
                    }
                }

                messageLayer.Update(gameTime.ElapsedGameTime.Milliseconds * GameSpeed);

                foreach (InfoArea ia in infoAreas) {
                    ia.Update(gameTime.ElapsedGameTime.Milliseconds * GameSpeed);
                }
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        protected abstract void EndGame();

        /// <summary>
        /// Responsible for the Circle character motion control. 
        /// </summary>
        /// <param name="input"> Used in case it is controlled by a human player</param>
        protected abstract void HandleCircleControls(InputState input);

        /// <summary>
        /// Responsible for the Rectangle character motion control. 
        /// </summary>
        /// <param name="input">InputState. Used in case it is controlled by a human player</param>
        protected abstract void HandleRectangleControls(InputState input);

        private void HandleKeyboardInput(InputState input) {
            if (!input.LastKeyboardState.IsKeyDown(Keys.F1) && input.CurrentKeyboardState.IsKeyDown(Keys.F1)) {
                debugViewEnabled = !debugViewEnabled;
                physicsSimulator.EnableDiagnostics = debugViewEnabled;
                rectangle.DebugDraw = !rectangle.DebugDraw;
                rectangle.debugFont = spriteFont;
                circle.DebugDraw = !circle.DebugDraw;
                circle.debugFont = spriteFont;
            }
        }

        private void HandleMouseInput(InputState input) {
            Vector2 point = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);
            if (input.LastMouseState.LeftButton == ButtonState.Released && input.CurrentMouseState.LeftButton == ButtonState.Pressed) {
                //create mouse spring
                pickedGeom = physicsSimulator.Collide(point);
                if (pickedGeom != null) {
                    mousePickSpring = ControllerFactory.Instance.CreateFixedLinearSpring(physicsSimulator, pickedGeom.Body, pickedGeom.Body.GetLocalPosition(point), point, 20, 10);
                }
            } else if (input.LastMouseState.LeftButton == ButtonState.Pressed && input.CurrentMouseState.LeftButton == ButtonState.Released) {
                //destroy mouse spring
                if (mousePickSpring != null && mousePickSpring.IsDisposed == false) {
                    mousePickSpring.Dispose();
                    mousePickSpring = null;
                }
            }

            //move anchor point
            if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && mousePickSpring != null) {
                mousePickSpring.WorldAttachPoint = point;
            }
        }

        private void HandleWiiMoteInput(InputState input) {
            //player1
            /* if (input.wiiInput.isKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_1))
             {
                 debugViewEnabled = !debugViewEnabled;
                 physicsSimulator.EnableDiagnostics = debugViewEnabled;
             }*/
        }

        public static void IsMulti(bool multi_level) {
            isMulti = multi_level;
        }

        public double GetElapsedGameTime() {
            return elapsedGameTime;
        }

        public long GetElapsedRealTime()
        {
            return realtimeCounter.ElapsedMilliseconds;
        }

        protected void SendLevelResults(string timeStamp, bool levelPassed, double elapsedGameTime, double timeLimit, int collectiblesCaught, int collectiblesAvailable)
        {
            if (LevelResults != null)
            {
                LevelResult results = new LevelResult(timeStamp, levelPassed, elapsedGameTime, timeLimit, collectiblesCaught, collectiblesAvailable);
                LevelResults(results);
            }
        }

        protected void SendLevelResults(LevelResult results)
        {
            if (LevelResults != null)
            {
                LevelResults(results);
            }
        }
    }
}
