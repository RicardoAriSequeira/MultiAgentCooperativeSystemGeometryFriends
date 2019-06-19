#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.IO;

#endregion
namespace GeometryFriends.ScreenSystem
{
    class EditorMenuScreen : MenuScreen
      {
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public EditorMenuScreen()
        {            
            MenuEntries.Add("Create World");
            MenuEntries.Add("Edit Levels");
            MenuEntries.Add("Back");
            //MenuEntries.Add("New Level");
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
                    ScreenManager.ScreenKeyboard.Enable(this.createNewLevelFile, "Name the new World", 10, "World");
                    break;
                case 1:
                    ExitScreen();
                    ScreenManager.AddScreen(new EditorLevelSelectScreen());
                    break;
                case 2:
                    ExitScreen();
                    ScreenManager.AddScreen(new MainMenuScreen());
                    break;
                //case 3:
                //    ExitScreen();
                //    ScreenManager.AddScreen(new EditorLevel());
                //    break;
            }
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
            ScreenManager.Game.Exit();
        }

        private void createNewLevelFile(String fileName, Boolean save)
        {
            if (save)
            {
                FileInfo fi = new FileInfo(Path.Combine("Levels", fileName + ".xml"));
                FileStream fs = fi.OpenWrite();
                TextWriter tw = new StreamWriter(fs);
                tw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                tw.WriteLine("<Levels>");
                tw.WriteLine("</Levels>");
                tw.Close();
            }
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.Clear(Color.White);

            instructionsBatch.DrawString(ScreenManager.TitleFont, "Level Editor", new Vector2(290, 150), Color.DarkBlue);

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
              
                itemPosition.Y += LINE_SPACING;
            }
        }
    }
}

