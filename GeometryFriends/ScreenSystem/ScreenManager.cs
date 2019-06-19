#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Components;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Collections.Generic;
#endregion

namespace GeometryFriends.ScreenSystem
{
    /// <summary>
    /// The screen manager is a component which manages one or more GameScreen
    /// instances. It maintains a stack of screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes input to the
    /// topmost active screen.
    /// </summary>
    internal class ScreenManager : DrawableGameComponent
    {
        #region Fields

        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();

        InputState input = new InputState();

        protected InGameConsole console;

        public SoundEffect menuMusic;
        public SoundEffect altMenuMusic;
        public SoundEffectInstance menuMusicInstance;

        protected SoundEffect levelMusic;
        public SoundEffectInstance levelMusicInstance;

        protected SoundEffect victoryMusic;
        public SoundEffectInstance victoryMusicInstance;

        protected AgentDebugPane agentPane;
        public AgentDebugPane AgentPane
        {
            get
            {
                return this.agentPane;
            }
        }

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

        ContentManager contentManager;
        SpriteFont diagnosticSpriteFont;
        SpriteFont menuSpriteFont;
        SpriteFont titleFont;
        SpriteFont subtitleFont;
        SpriteFont detailsFont;
        Texture2D blankTexture;
        
        bool traceEnabled;

        #endregion

        #region Properties


        /// <summary>
        /// Expose access to our Game instance (this is protected in the
        /// default GameComponent, but we want to make it public).
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// A content manager used to load data that is shared between multiple
        /// screens. This is never unloaded, so if a screen requires a large amount
        /// of temporary data, it should create a local content manager instead.
        /// </summary>
        public ContentManager ContentManager
        {
            get { return contentManager; }
        }
        
        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont DiagnosticSpriteFont
        {
            get { return diagnosticSpriteFont; }
        }

        public SpriteFont DetailsFont
        {
            get { return detailsFont; }
        }

        /// <summary>
        /// A default font shared by all the screens. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public SpriteFont MenuSpriteFont
        {
            get { return menuSpriteFont; }
        }

      
        public SpriteFont TitleFont
        {
            get { return titleFont; }
        }

        public SpriteFont SubTitleFont
        {
            get { return subtitleFont; }
        }

        public Vector2 ScreenCenter
        {
            get { return new Vector2(GameAreaInformation.DRAWING_WIDTH / 2, GameAreaInformation.DRAWING_HEIGHT / 2); }
        }

        public int ScreenWidth
        {
            get { return GameAreaInformation.DRAWING_WIDTH; }
        }

        public int ScreenHeight
        {
            get { return GameAreaInformation.DRAWING_HEIGHT; }
        }

        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }


        #endregion

        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(Game game, OnScreenKeyboard keyboard, InGameConsole console, AgentDebugPane agentPane) 
            : base(game.GraphicsDevice)
        {
            this.Game = game;
            this.agentPane = agentPane;
            contentManager = new ContentManager();

            this.keyboard = keyboard;
            this.console = console;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            foreach (GameScreen screen in screens)
            {
                screen.Initialize();
            }
            base.Initialize();
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load content belonging to the screen manager.
            diagnosticSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/diagnosticFont.spritefont");
            menuSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/menuFont.spritefont");
            titleFont = contentManager.Load<SpriteFont>("Content/Fonts/titleFont.spritefont");
            subtitleFont = contentManager.Load<SpriteFont>("Content/Fonts/subtitleFont.spritefont");
            detailsFont = contentManager.Load<SpriteFont>("Content/Fonts/detailsFont.spritefont");
            blankTexture = contentManager.Load<Texture2D>("Content/Common/blank.png");

            altMenuMusic = contentManager.Load<SoundEffect>("Content/Sound/nyanyan.wav");
            menuMusic = contentManager.Load<SoundEffect>("Content/Sound/menuMusic.wav");
            menuMusicInstance = menuMusic.CreateInstance();
            menuMusicInstance.IsLooped = true;
            menuMusicInstance.Volume *= .4f;

            levelMusic = contentManager.Load<SoundEffect>("Content/Sound/levelMusic.wav");
            levelMusicInstance = levelMusic.CreateInstance();
            levelMusicInstance.IsLooped = true;
            levelMusicInstance.Volume *= .5f;

            victoryMusic = contentManager.Load<SoundEffect>("Content/Sound/victoryMusic.wav");
            victoryMusicInstance = victoryMusic.CreateInstance();
            victoryMusicInstance.IsLooped = true;
            victoryMusicInstance.Volume *= .7f;

            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            contentManager.Unload();

            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // Read the keyboard and gamepad.
            input.Update();

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            //foreach (GameScreen screen in screens)  
            //screensToUpdate.Add(screen);
            for (int i = 0; i < screens.Count; i++)
                screensToUpdate.Add(screens[i]);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        if (keyboard.Visible)
                            keyboard.HandleInput(input);
                        else
                            screen.HandleInput(input);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            // Print debug trace?
            if (traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            List<string> screenNames = new List<string>();

            foreach (GameScreen screen in screens)
                screenNames.Add(screen.GetType().Name);

            Log.LogInformation(string.Join(", ", screenNames.ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            //foreach (GameScreen screen in screens)
            //{
            //    if (screen.ScreenState == ScreenState.Hidden)
            //        continue;

            //    screen.Draw(gameTime);
            //}
            if (keyboard.Visible)
                keyboard.Draw(gameTime, instructionsBatch);

            for (int i = 0; i < screens.Count; i++)
            {
                if (screens[i].ScreenState == ScreenState.Hidden)
                    continue;

                screens[i].Draw(gameTime, instructionsBatch);
            }

            //spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            //Vector2 stringSize = menuSpriteFont.MeasureString("Farseer Physics Engine");
            //Vector2 origin = new Vector2(stringSize.X/2,stringSize.Y/2);
            //spriteBatch.DrawString(menuSpriteFont, "Farseer Physics Engine", ScreenCenter, new Color(255, 255, 255, 30), -0, origin, 1, SpriteEffects.None, 0);
            //spriteBatch.End();
            //new Vector2(ScreenWidth - 25,ScreenHeight-10)
        }

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;
            screen.Initialize();
            // If we have a graphics device, tell the screen to load content.
            if (GraphicsDevice != null)
            {
                screen.LoadContent();
            }

            screens.Add(screen);
        }


        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (GraphicsDevice != null)
            {
                screen.UnloadContent();
            }

            screens.Remove(screen);
            screensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        /*public void FadeBackBufferToBlack(int alpha)
        {
            GraphicsDevice.DrawTexture(blankTexture, new Vector2(0, 0), ScreenWidth, ScreenHeight, new Color(0, 0, 0, (byte)alpha));
        }*/
    }
}
