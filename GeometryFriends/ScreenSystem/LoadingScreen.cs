#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
#endregion

namespace GeometryFriends.ScreenSystem
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    class LoadingScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D backgroundTexture;
        SpriteFont textFont;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadingScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(.5f);
            TransitionOffTime = TimeSpan.FromSeconds(.5f);
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the ScreenManager, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager();

            backgroundTexture = content.Load<Texture2D>("Content/Common/background.png");

            textFont = content.Load<SpriteFont>("Content/Fonts/menuFont.spritefont");
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }


        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            Draw(instructionsBatch);
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            byte fade = TransitionAlpha;

            instructionsBatch.DrawString(textFont, "Loading...", new Vector2(GameAreaInformation.DRAWING_WIDTH / 2, GameAreaInformation.DRAWING_HEIGHT / 2), Color.White);
        }
    }
}
