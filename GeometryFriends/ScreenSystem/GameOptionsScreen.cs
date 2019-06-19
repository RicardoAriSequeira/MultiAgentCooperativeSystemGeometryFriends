using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.AI;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Collections.Generic;


namespace GeometryFriends.ScreenSystem
{
    class GameOptionsScreen : MenuScreen
    {
        protected List<string[]> possibleOptions;
        protected List<int> selectedOptions;
        protected List<string> agentImplementations;

        protected const string NO_AGENTS_IMPLEMENTATION = "(No Implementations Available)";

        public GameOptionsScreen()
        {            
            possibleOptions = new List<string[]>();
            
            MenuEntries.Add("Music: ");
            possibleOptions.Add(new string[2] { "Enabled", "Disabled" });
            
            MenuEntries.Add("Controls layout: ");
            possibleOptions.Add(new string[2] { "Wiimote Only", "Wiimote + Nunchuk" });
            
            MenuEntries.Add("Log Input");
            possibleOptions.Add(new string[2] { "Enabled", "Disabled" });
            
            MenuEntries.Add("Single Player Mode");
            possibleOptions.Add(new string[2] { "Human + Rectangle AI", "Human + Circle AI" });
            
            MenuEntries.Add("Agents Implementation");
            agentImplementations = AgentsManager.GetAgentImplementations();
            if(agentImplementations.Count == 0)
                possibleOptions.Add(new string[1] { NO_AGENTS_IMPLEMENTATION});
            else
                possibleOptions.Add(agentImplementations.ToArray());

            MenuEntries.Add("Save & Exit");

            MenuEntries.Add("Cancel");

            selectedOptions = new List<int>(possibleOptions.Count);
            ReadSettingsFile();
        }

        protected void ReadSettingsFile()
        {
            if (GeometryFriends.Properties.Settings.Default.musicOn)
                selectedOptions.Insert(0, 0);
            else
                selectedOptions.Insert(0, 1);
            if (GeometryFriends.Properties.Settings.Default.controls)
                selectedOptions.Insert(1, 0);
            else
                selectedOptions.Insert(1, 1);
            //write log
            if (GeometryFriends.Properties.Settings.Default.writeLog)
                selectedOptions.Insert(2, 0);
            else
                selectedOptions.Insert(2, 1);
            //Single Player AI combination
            if(GeometryFriends.Properties.Settings.Default.CircleHumanControl)
                selectedOptions.Insert(3, 0);
            else // only other possibility is the square control
                selectedOptions.Insert(3, 1); 
            //agents implementation to be used for AI
            string selectedImplementation = GeometryFriends.Properties.Settings.Default.AgentsImplementation;
            if (agentImplementations.Contains(selectedImplementation))
                selectedOptions.Insert(4, agentImplementations.IndexOf(selectedImplementation));
            else
                selectedOptions.Insert(4, 0); //the previously used implementation is no longer available, select the first
        }

        protected void WriteSettingsFile()
        {
            //musicOn
            if (selectedOptions[0] == 0)
                GeometryFriends.Properties.Settings.Default.musicOn = true;
            else
                GeometryFriends.Properties.Settings.Default.musicOn = false;
            
            //controls
            if (selectedOptions[1] == 0)
                GeometryFriends.Properties.Settings.Default.controls = true;
            else
                GeometryFriends.Properties.Settings.Default.controls = false;

            //write log
            if (selectedOptions[2] == 0)
                GeometryFriends.Properties.Settings.Default.writeLog = true;                
            else
                GeometryFriends.Properties.Settings.Default.writeLog = false;

            //AI to square
            if (selectedOptions[3] == 0)
            {// human is circle
                GeometryFriends.Properties.Settings.Default.SquareHumanControl = false;
                GeometryFriends.Properties.Settings.Default.CircleHumanControl = true;
            }
            else
            { // human is rectangle
                GeometryFriends.Properties.Settings.Default.SquareHumanControl = true;
                GeometryFriends.Properties.Settings.Default.CircleHumanControl = false;
            }

            //save selected agents implementation
            if (possibleOptions[4][selectedOptions[4]] == NO_AGENTS_IMPLEMENTATION)
                GeometryFriends.Properties.Settings.Default.AgentsImplementation = ""; //no implementation selected
            else
                GeometryFriends.Properties.Settings.Default.AgentsImplementation = possibleOptions[4][selectedOptions[4]];

            GeometryFriends.Properties.Settings.Default.Save();
            GeometryFriends.Properties.Settings.Default.Reload();
        }

        protected override void OnSelectEntry(int entryIndex)
        {
            switch (entryIndex)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    ToggleOption(entryIndex);
                    break;
                case 5:
                    WriteSettingsFile();
                    goto case 6;
                case 6:
                    if (!GeometryFriends.Properties.Settings.Default.musicOn)
                        ScreenManager.menuMusicInstance.Stop();
                    ExitScreen();
                    ScreenManager.AddScreen(new MainMenuScreen());
                    break;                
            }
        }

        protected void ToggleOption(int index)
        {
            if (selectedOptions[index] < possibleOptions[index].Length - 1)
                ++selectedOptions[index];
            else
                selectedOptions[index] = 0;
        }

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

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.Clear(Color.White);

            instructionsBatch.DrawString(ScreenManager.TitleFont, "Game Options", new Vector2(290, 150), Color.DarkBlue);

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

                //only the items that have options require extra information to be drawn
                if (i < possibleOptions.Count)
                {
                    instructionsBatch.DrawString(
                        menuSpriteFont,
                        possibleOptions[i][selectedOptions[i]],
                        new Vector2(itemPosition.X + 300, itemPosition.Y) + originText,
                        scale,
                        Color.Tomato);
                }

                itemPosition.Y += LINE_SPACING;
            }
        }
    }
}
