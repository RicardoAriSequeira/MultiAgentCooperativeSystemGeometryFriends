#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Collections.Generic;
#endregion

namespace GeometryFriends.ScreenSystem
{
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    abstract class MenuScreen : GameScreen
    {
        #region Fields

        protected SpriteFont menuSpriteFont;
        protected List<string> menuEntries = new List<string>();
        protected int selectedEntry = 0;
        protected Vector2 position = Vector2.Zero;
        private float leftBorder = 100;
        #endregion

        #region Properties


        /// <summary>
        /// Gets the list of menu entry strings, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<string> MenuEntries
        {
            get { return menuEntries; }
        }

        public float LeftBorder
        {
            get { return leftBorder; }
            set { leftBorder = value; }
        }


        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public MenuScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Back) && input.CurrentKeyboardState.IsKeyUp(Keys.Back))
            {
                ScreenManager.menuMusicInstance.Stop();
                ScreenManager.menuMusicInstance = ScreenManager.altMenuMusic.CreateInstance();
                ScreenManager.menuMusicInstance.Play();
            }
            // Move to the previous menu entry?
            if (input.MenuUp)
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;
            }

            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                this.OnSelectEntry(selectedEntry);
            }
            else if (input.MenuCancel)
            {
                this.OnCancel();
            }
        }


        /// <summary>
        /// Notifies derived classes that a menu entry has been chosen.
        /// </summary>
        protected abstract void OnSelectEntry(int entryIndex);


        /// <summary>
        /// Notifies derived classes that the menu has been cancelled.
        /// </summary>
        protected abstract void OnCancel();

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            menuSpriteFont = ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/menuFont.spritefont");
            CalculatePosition();
        }

        private void CalculatePosition()
        { 
            float totalHeight;
            totalHeight = ScreenManager.GraphicsDevice.MeasureString("T", menuSpriteFont).Y;
            //totalHeight += ScreenManager.MenuSpriteFont.LineSpacing;
            totalHeight *= menuEntries.Count;

            position.Y = (GameAreaInformation.DRAWING_HEIGHT - totalHeight) / 2;
            position.X = leftBorder;
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {

            Vector2 itemPosition = position;

            // Draw each menu entry in turn.
            //ScreenManager.SpriteBatch.Begin();

            for (int i = 0; i < menuEntries.Count; i++)
            {
                Color color;
                float scale;

                if (IsActive && (i == selectedEntry))
                {
                    //// The selected entry is yellow, and has an animating size.
                    double time = gameTime.TotalGameTime.TotalSeconds;

                    float pulsate = (float)Math.Sin(time * 3) + 1;
                    scale = 1 + pulsate * 0.05f;

                    color = GameColors.MENU_SCREEN_COLOR;
                    //scale = 1;
                }
                else
                {
                    // Other entries are white.
                    color = Color.Black;
                    scale = 1;
                }

                // Modify the alpha to fade text out during transitions.
                color = new Color(color.R, color.G, color.B, TransitionAlpha);

                // Draw text, centered on the middle of each line.
                Vector2 origin = new Vector2(0, LINE_SPACING / 2);

                instructionsBatch.DrawString(menuSpriteFont, menuEntries[i], itemPosition + origin, scale, color);
                // itemPosition.Y += ScreenManager.MenuSpriteFont.MeasureString("T").Y;
                itemPosition.Y += LINE_SPACING;
            }

            // ScreenManager.SpriteBatch.End();
        }
    }
}
