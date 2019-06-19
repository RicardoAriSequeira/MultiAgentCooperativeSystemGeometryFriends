#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Collections.Generic;

#endregion

namespace GeometryFriends.ScreenSystem
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class LevelSelectScreen : MenuScreen
    {
        Texture2D geometryFriendsLogoTexture;
        protected Vector2 origin;
        protected new Vector2 selectedEntry;
        protected Vector2 currentWorldStringPosition = new Vector2(100, 50);
        protected List<LevelPreview> previews;
        protected int unlockedCount;
        protected int screen, screenMax;

        private const string BACK_TO_MAIN_MENU = "Back to Menu";
        private const string PREVIOUS_LEVELS = "Previous Levels";
        private const string MORE_LEVELS = "More Levels";

        public enum PlayerType
        {
            SinglePlayer,
            Multiplayer,
            Agents
        }

        protected PlayerType playerType;
        protected Vector2 previewsOffset = new Vector2(50, 170);
        protected const int PREVIEWS_PER_LINE = 4;
        protected const int PREVIEWS_PER_SCREEN = 12;

        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public LevelSelectScreen(PlayerType playerType)
        {
            this.playerType = playerType;
            previews = new List<LevelPreview>();
            unlockedCount = 0;
            screen = 0;
            screenMax = 0;
          /*  for (int i = 1; i <= XMLLevelParser.numberOfLevels(); i++)
            {
                MenuEntries.Add("Level " + i);                           
            }*/
            MenuEntries.Add("Exit");
            LeftBorder = 300;
        }

        protected override void  OnSelectEntry(int entryIndex)
        {
        }
      
        /// <summary>
        /// Responds to user menu selections.
        /// </summary>
        protected virtual void OnSelectEntry()
        {
            if (selectedEntry.X > 0 && selectedEntry.X < 4)
            {
                ScreenManager.menuMusicInstance.Stop();
                if (GeometryFriends.Properties.Settings.Default.musicOn)
                    ScreenManager.levelMusicInstance.Play();
                ExitScreen();
                switch (playerType)
                {
                    case PlayerType.SinglePlayer:
                        //select which agent should play with the player
                        SinglePlayerLevel.AgentsToRun run;
                        if (GeometryFriends.Properties.Settings.Default.CircleHumanControl)
                            run = SinglePlayerLevel.AgentsToRun.Rectangle;
                        else
                            run = SinglePlayerLevel.AgentsToRun.Circle;

                        ScreenManager.AddScreen(new SinglePlayerLevel((int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE) + (int)selectedEntry.Y + 1, ScreenManager.AgentPane, run));              
                        break;
                    case PlayerType.Multiplayer:
                        ScreenManager.AddScreen(new MultiplayerLevel((int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE) + (int)selectedEntry.Y + 1));
                        break;
                    case PlayerType.Agents:
                        ScreenManager.AddScreen(new SinglePlayerLevel((int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE) + (int)selectedEntry.Y + 1, ScreenManager.AgentPane, SinglePlayerLevel.AgentsToRun.Both));              
                        break;
                    default:
                        throw new Exception("No recognized player type selected.");
                }
            }

            if (selectedEntry.X == 4)
            {
                if (selectedEntry.Y == 0)
                {
                    ExitScreen();
                    ScreenManager.AddScreen(new MainMenuScreen());
                }
                else if(selectedEntry.Y == 1){
                    //more levels
                    screen++;
                    if (screen > screenMax)
                        screen = screenMax;                    
                    selectedEntry.Y = 0;
                    selectedEntry.X = 0;
                }
                else if (selectedEntry.Y == 2)
                {
                    //previous levels
                    screen--;
                    if (screen < 0)
                        screen = 0;
                    selectedEntry.Y = 0;
                    selectedEntry.X = 0;
                }
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            geometryFriendsLogoTexture = this.ScreenManager.ContentManager.Load<Texture2D>("Content/Common/geometryfriends-logo.png");
            origin = new Vector2(geometryFriendsLogoTexture.Width / 2f, geometryFriendsLogoTexture.Height / 2f);

            currentWorldStringPosition = new Vector2((GameAreaInformation.DRAWING_WIDTH / 2), 50);

            LoadPreviews();

            unlockedCount = 0;
        }

        protected void LoadPreviews()
        {
            screen = 0;
            previews.Clear();

            int previewScreen = 0;
            for (int i = 1; i <= XMLLevelParser.CurrentWorldNumberOfLevels(); i++)
            {
                previews.Add(new LevelPreview(
                    i, ScreenManager.GraphicsDevice,
                    new Vector2(previewsOffset.X + ((i - 1) % PREVIEWS_PER_LINE) * 300, previewsOffset.Y + ((i - 1 - previewScreen * PREVIEWS_PER_SCREEN) / PREVIEWS_PER_LINE) * 200)));

                if (i % PREVIEWS_PER_SCREEN == 0)
                    previewScreen++;
            }

            foreach (LevelPreview lp in previews)
            {
                lp.LoadContent(ScreenManager.ContentManager);
                lp.IsSelected = false;
            }
            //calculate the maximum number of screens that we will have in this world
            screenMax = (int)Math.Floor((double)XMLLevelParser.CurrentWorldNumberOfLevels() / PREVIEWS_PER_SCREEN);
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


        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            //ScreenManager.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            instructionsBatch.Clear(Color.White);

            string title = XMLLevelParser.GetWorldName();
            Vector2 titleSize = instructionsBatch.MeasureString(title, ScreenManager.TitleFont);
            float titleX = titleSize.X / 2;            
            float remainderSpace = instructionsBatch.DWidth - titleSize.X;
            float padding = 200;
            float screenPart = 5 * (instructionsBatch.DWidth / 6);
            if(remainderSpace <= 0){
                padding = 0;
            }
            else if (remainderSpace < screenPart)
            {
                //adjust padding for smaller spaces
                padding -= 200 * (screenPart - remainderSpace) / screenPart;
            }
            
            //instructionsBatch.DrawRectangle(currentWorldStringPosition + new Vector2(-titleX, 0), (int)titleSize.X, (int)titleSize.Y, Color.DarkCyan);
            instructionsBatch.DrawString(ScreenManager.TitleFont, title, currentWorldStringPosition + new Vector2( - titleX, 0), (selectedEntry.X != 0) ? Color.Black : Color.Red);
            //write previous world name
            title = XMLLevelParser.GetPrevWorldName();
            titleSize = instructionsBatch.MeasureString(title, ScreenManager.SubTitleFont);
            instructionsBatch.DrawString(ScreenManager.SubTitleFont, title, currentWorldStringPosition + new Vector2(- padding - titleX - titleSize.X, 50), Color.DarkGray);
            //write next world name
            title = XMLLevelParser.GetNextWorldName();
            titleSize = instructionsBatch.MeasureString(title, ScreenManager.SubTitleFont);
            instructionsBatch.DrawString(ScreenManager.SubTitleFont, title, currentWorldStringPosition + new Vector2(titleX + padding, 50), Color.DarkGray);

            if (selectedEntry.X != 4)
            {
                DrawBottomMenu(Color.Black, Color.Black, Color.Black, instructionsBatch);
            }
            else if (selectedEntry.X == 4)
            {
                if (selectedEntry.Y == 0)
                    DrawBottomMenu(Color.Red, Color.Black, Color.Black, instructionsBatch);
                else if (selectedEntry.Y == 1)
                    DrawBottomMenu(Color.Black, Color.Red, Color.Black, instructionsBatch);                    
                else if (selectedEntry.Y == 2)
                    DrawBottomMenu(Color.Black, Color.Black, Color.Red, instructionsBatch);
                else
                    DrawBottomMenu(Color.Black, Color.Black, Color.Black, instructionsBatch);
            }

            byte fade = 255;// TransitionAlpha;                          

            Color tint = new Color(fade, fade, fade, fade);

            int lockedCount = 0;
            unlockedCount = 0;
            int drawCount = 0;
            int previewCount = 0;
            bool skip = false;
            foreach (LevelPreview p in previews)
            {
                previewCount++;
                //skip initial screens
                if (previewCount <= screen * PREVIEWS_PER_SCREEN)
                    skip = true;
                else
                    skip = false;

                if (drawCount == PREVIEWS_PER_SCREEN)
                    break; //no more level previews fit the screen

                string stars = p.GetMaxStar();
                if (stars.Equals("Locked"))
                {
                    lockedCount++;
                    if (lockedCount == 1)
                        stars = "New Level";
                }
                if (!stars.Equals("Locked") || lockedCount == 1)
                {
                    if (!skip)
                    {
                        p.Draw(gameTime, instructionsBatch);
                        drawCount++;
                    }
                    unlockedCount++;                    
                }
                if(!skip)
                    instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, stars, new Vector2(p.GetPosition().X + 50, p.GetPosition().Y + 150), p.IsSelected ? Color.Red : Color.Black);
            }
        }

        protected void DrawBottomMenu(Color backColor, Color previousColor, Color moreColor, DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, BACK_TO_MAIN_MENU, new Vector2(100, 750), backColor);
            if (screen < screenMax) 
                instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, MORE_LEVELS, new Vector2(350, 750), previousColor);
            if (screen > 0)
                instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, PREVIOUS_LEVELS, new Vector2(600, 750), moreColor);
        }

        public override void HandleInput(InputState input)
        {
            int index;
            // Move to the previous menu entry?
            if (input.MenuUp)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < unlockedCount)
                    {
                        previews[index].IsSelected = false;
                    }
                    else
                    {
                        try
                        {
                            previews[unlockedCount - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                            Log.LogRaw("Error accessing previews.");
                        }
                        selectedEntry.X = unlockedCount / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = (unlockedCount % PREVIEWS_PER_LINE - 1 < 0 ? 3 : unlockedCount % PREVIEWS_PER_LINE - 1);
                    }

                }

                if (selectedEntry.X == 0)
                {
                    selectedEntry.X = 4;
                    selectedEntry.Y = 0;
                }
                else
                {
                    if (selectedEntry.X == 4 || selectedEntry.X == 1)
                    {
                        selectedEntry.Y = 0;
                    }
                    selectedEntry.X--;
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    while (index >= unlockedCount)
                    {
                        selectedEntry.X--;
                        index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    }
                    if (index < unlockedCount)
                        previews[index].IsSelected = true;
                    else
                    {
                        selectedEntry.X = (float)Math.Ceiling((float)unlockedCount / (float)PREVIEWS_PER_LINE);
                        selectedEntry.Y = (unlockedCount % PREVIEWS_PER_LINE - 1 < 0 ? 0 : unlockedCount % PREVIEWS_PER_LINE - 1);
                        index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                        try
                        {
                            previews[index].IsSelected = true;
                        }
                        catch (Exception)
                        {
                        }
                    }
                
                }
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < unlockedCount)
                        previews[index].IsSelected = false;
                    else
                    {
                        previews[unlockedCount - 1].IsSelected = false;
                        selectedEntry.X = unlockedCount / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = (unlockedCount % PREVIEWS_PER_LINE - 1 < 0 ? 3 : unlockedCount % PREVIEWS_PER_LINE - 1);
                    }
                }

                if (selectedEntry.X == 4)
                {
                    selectedEntry.X = 0;
                }
                else
                {
                    selectedEntry.X++;
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    int minIndex = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE);
                    index = (int)(minIndex + selectedEntry.Y);
                    while (index >= unlockedCount && index > minIndex)
                    {
                        index--;
                        selectedEntry.Y--;
                    }
                    if (index < unlockedCount)
                        previews[index].IsSelected = true;
                    else
                    {
                        try
                        {
                            previews[unlockedCount - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                        }
                        selectedEntry.X = 4;
                        selectedEntry.Y = 0;
                    }
                }
                else if (selectedEntry.X == 4)
                {
                    selectedEntry.Y = 0;
                }
            }

            if (input.MenuLeft)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < unlockedCount)
                        previews[index].IsSelected = false;
                    else
                        try
                        {
                            previews[unlockedCount - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                        }

                }

                if (selectedEntry.X == 0)
                {
                    ChangeToPreviousWorld();
                }

                if(selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    if (selectedEntry.Y == 0)
                        selectedEntry.Y = 3;
                    else
                        selectedEntry.Y--;
                }
                else if (selectedEntry.X == 4)
                {
                    selectedEntry.Y = GetPreviousBottomMenuIndex(selectedEntry.Y);
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < unlockedCount)
                        previews[index].IsSelected = true;
                    else
                    {
                        try
                        {
                            previews[unlockedCount - 1].IsSelected = true;
                        }
                        catch (Exception)
                        {
                        }
                        selectedEntry.X = unlockedCount / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = (unlockedCount % PREVIEWS_PER_LINE - 1 < 0 ? 3 : unlockedCount % PREVIEWS_PER_LINE - 1);
                    }

                }      
            }

            if (input.MenuRight)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < unlockedCount)
                        previews[index].IsSelected = false;
                    else
                        try
                        {
                            previews[unlockedCount - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                        }

                }

                if (selectedEntry.X == 0)
                {
                    ChangeToNextWorld();
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    if (selectedEntry.Y == 3)
                        selectedEntry.Y = 0;
                    else
                        selectedEntry.Y++;
                }
                else if (selectedEntry.X == 4)
                {
                    selectedEntry.Y = GetNextBottomMenuIndex(selectedEntry.Y);
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((screen * PREVIEWS_PER_SCREEN) + (selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < unlockedCount)
                        previews[index].IsSelected = true;
                    else
                    {
                        selectedEntry.X = unlockedCount / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = 0;
                        index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                        try
                        {
                            previews[index].IsSelected = true;
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
            }

            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                this.OnSelectEntry();
            }
            else if (input.MenuCancel)
            {
                this.OnCancel();
            }
        }

        protected float GetNextBottomMenuIndex(float previous)
        {
            float next = previous + 1;
            bool skipMore = false;
            bool skipLess = false;
            //check which options are skipped
            if(screen == 0)
                skipLess = true;
            if(screen == screenMax)
                skipMore = true;
            
            if(next > 2)
                return 0;//cycle to main menu
            else if(next == 1){
                if(skipMore && skipLess)
                    return 0; //cycle to main menu
                else if(skipMore)
                    return 2; //cycle to previous option
                else
                    return 1; //do not skip the more then it is the chosen one
            }
            else if(next == 2){
                if(skipLess)
                    return 0; //cycle to main menu
                else
                    return 2; //do not skip the previous option
            }

            return next; //should be just cycling to back to main menu
        }

        protected float GetPreviousBottomMenuIndex(float previous)
        {
            float next = previous - 1;
            bool skipMore = false;
            bool skipLess = false;
            //check which options are skipped
            if (screen == 0)
                skipLess = true;
            if (screen == screenMax)
                skipMore = true;

            if (next < 0){
                if (skipMore && skipLess)
                    return 0; //cycle to main menu
                else if (skipMore)
                    return 2; //cycle to the previous levels item as it is the more that is to be skipped
                else
                    return 1; //cycle to the more levels as it is the previous that is to be skipped
            }
            else if (next == 1)
            {
                if (skipMore)
                    return 0; //cycle to back option
                else
                    return 1; //cycle to the more levels as it is the previous that is to be skipped
            }
            
            return next; //should be just cycling to back to main menu
        }

        protected void ChangeToNextWorld()
        {
            //TESTE 1
            //ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //ScreenManager.SpriteBatch.DrawString(this.ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/menuFont"), "Loading...", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2), Color.Black);
            //ScreenManager.SpriteBatch.End();

            //TESTE 2
            //LoadingScreen l = new LoadingScreen();
            //ScreenManager.AddScreen(l);
            //l.Draw();

            XMLLevelParser.ChangeToNextWorld();

            LoadPreviews();

            //ScreenManager.RemoveScreen(l);
        }

        protected void ChangeToPreviousWorld()
        {
            //TESTE 1
            //ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            //ScreenManager.SpriteBatch.DrawString(this.ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/menuFont"), "Loading...", new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2), Color.Black);
            //ScreenManager.SpriteBatch.End();

            //TESTE 2
            //LoadingScreen l = new LoadingScreen();
            //ScreenManager.AddScreen(l);
            //l.Draw();

            XMLLevelParser.ChangeToPreviousWorld();

            LoadPreviews();

            //ScreenManager.RemoveScreen(l);
        }
    }
}
