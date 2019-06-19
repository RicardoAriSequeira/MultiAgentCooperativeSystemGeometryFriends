#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Levels;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

#endregion

namespace GeometryFriends.ScreenSystem
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MultiplayerMenuScreen : MenuScreen
    {
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MultiplayerMenuScreen()
        {            
            MenuEntries.Add("All Levels");
            MenuEntries.Add("Select Level");            
            MenuEntries.Add("Back");
            LeftBorder = 300;
        }

        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            switch (entryIndex)
            {
                case 0:
                    ExitScreen();
                    ScreenManager.AddScreen(new MultiplayerLevel(1));
                    break;
                case 1:                    
                    ExitScreen();
                    ScreenManager.AddScreen(new LevelSelectScreen(LevelSelectScreen.PlayerType.Multiplayer));
                    break;                
 
                case 3:
                    ExitScreen();
                    ScreenManager.AddScreen(new MainMenuScreen());
                    break;
            }
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
            ExitScreen();
            ScreenManager.AddScreen(new MainMenuScreen());
        }


        /// <summary>
        /// Loading screen callback for activating the gameplay screen.
        /// </summary>
        void LoadGameplayScreen(object sender, EventArgs e)
        {
            //ScreenManager.AddScreen(new GameplayScreen());
        }


        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            //ScreenManager.SpriteBatch.DrawString(ScreenManager.TitleFont, "GEOMETRY FRIENDS", new Vector2(290, 150), Color.Azure);
            Texture2D geometryFriendsLogoTexture;

            geometryFriendsLogoTexture = this.ScreenManager.ContentManager.Load<Texture2D>("Content/Common/geometryfriends-logo.png");

            instructionsBatch.Clear(Color.White);

            byte fade = 255;// TransitionAlpha;                          

            Color tint = new Color(fade, fade, fade, fade);

            instructionsBatch.DrawTexture(geometryFriendsLogoTexture, new Vector2(ScreenManager.ScreenCenter.X - geometryFriendsLogoTexture.Width / 2f, ScreenManager.ScreenCenter.Y - 150 - geometryFriendsLogoTexture.Height / 2f), geometryFriendsLogoTexture.Width, geometryFriendsLogoTexture.Height, tint);

            Vector2 itemPosition = new Vector2(position.X, position.Y + 150);

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

                    color = Color.DarkBlue;
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
                Vector2 originText = new Vector2(0, ScreenManager.MenuSpriteFont.LineSpacing / 2);

                instructionsBatch.DrawString(menuSpriteFont, menuEntries[i], itemPosition + originText, scale, color);
              
                itemPosition.Y += menuSpriteFont.LineSpacing;
            }
        }
    }
}
