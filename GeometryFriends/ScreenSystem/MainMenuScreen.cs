#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.AI;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Windows.Forms;

#endregion

namespace GeometryFriends.ScreenSystem
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        Texture2D geometryFriendsLogoTexture;
        Vector2 origin;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
        {
            MenuEntries.Add("Single Player");
            MenuEntries.Add("Multiplayer");
            MenuEntries.Add("Agents Only");
            MenuEntries.Add("Editor");
            MenuEntries.Add("Options");
            MenuEntries.Add("Exit");
            LeftBorder = 300;
        }

        public override void LoadContent()
        {
            geometryFriendsLogoTexture = this.ScreenManager.ContentManager.Load<Texture2D>("Content/Common/geometryfriends-logo.png");
            origin = new Vector2(geometryFriendsLogoTexture.Width / 2f, geometryFriendsLogoTexture.Height / 2f);
            base.LoadContent();
        }

        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected override void OnSelectEntry(int entryIndex)
        {
            switch (entryIndex)
            {
               /* case 0:
                    ExitScreen();
                    ScreenManager.ScreenKeyboard.Enable(changePlayerName,"Choose Team Name",10,ScreenManager.ScreenKeyboard.getDefaultName());//ScreenManager.AddScreen(new SinglePlayerLevel(1));                    
                    break; */
                case 0:
                    if (!AgentsManager.HasAgentsImplementation())
                    {
                        MessageBox.Show("No agents implementation available, therefore no agent will play the AI player.", "AI Player Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    ExitScreen();
                    ScreenManager.AddScreen(new LevelSelectScreen(LevelSelectScreen.PlayerType.SinglePlayer));
                    break;
                case 1:
                    ExitScreen();
                    ScreenManager.AddScreen(new LevelSelectScreen(LevelSelectScreen.PlayerType.Multiplayer));
                    break;
                case 2:
                    if (AgentsManager.HasAgentsImplementation())
                    {
                        ExitScreen();
                        ScreenManager.AddScreen(new LevelSelectScreen(LevelSelectScreen.PlayerType.Agents));
                    }
                    else
                    {
                        MessageBox.Show("No agents implementation available.", "Cannot Run Agents Only", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    break;
                case 3:
                    ExitScreen();
                    ScreenManager.AddScreen(new EditorMenuScreen());
                    break;
                case 4:
                    ExitScreen();
                    ScreenManager.AddScreen(new GameOptionsScreen());
                    break;
                case 5:
                    // Exit the sample.
                    OnCancel();
                    break;
            }
        }

    /*    private void changePlayerName(String name, Boolean change)
        {
            if(change)
                ScreenManager.ScreenKeyboard.SetDefaultName(name);
            ScreenManager.AddScreen(new MainMenuScreen());
        } */

        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        /// <summary>
        /// Loading screen callback for activating the gameplay screen.
        /// </summary>
        void LoadGameplayScreen(object sender, EventArgs e)
        {
            //ScreenManager.AddScreen(new GameplayScreen());
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!otherScreenHasFocus && !ScreenManager.menuMusicInstance.IsPlaying && GeometryFriends.Properties.Settings.Default.musicOn)
                ScreenManager.menuMusicInstance.Play();
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {      
            instructionsBatch.Clear(Color.White);
            instructionsBatch.DrawTexture(geometryFriendsLogoTexture, new Vector2(ScreenManager.ScreenCenter.X - origin.X, ScreenManager.ScreenCenter.Y - 150 - origin.Y), geometryFriendsLogoTexture.Width, geometryFriendsLogoTexture.Height);
        
            Vector2 itemPosition = new Vector2(position.X, position.Y + 150);
            
            // Draw each menu entry in turn.
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

                itemPosition.Y += LINE_SPACING;
            }
        }
    }
}
