using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.Levels;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.ScreenSystem
{
    class VictoryScreen : MenuScreen
    {
        string title = "Title";
        string details;

        Texture2D panelTexture;
        int panelWidth = 640;
        int panelHeight = 512;
        Color panelColor = new Color(100, 100, 100, 200);

        Texture2D textPanelTexture;
        int textPanelWidth = 440;
        int textPanelHeight = 512;
        int textLeftBorder = 20;
        int textTopBorder = 20;
        int textPanelLeftBorder = 200;
        Color textPanelColor = new Color(100, 100, 100, 220);

        Color textColor = Color.White;
        SpriteFont detailsFont;
        int levelNumber;

        GameScreen oldGame;

        public VictoryScreen(double score, bool newRecord, string highscoreList , int levelNumber, string name, string extraCongratulationsText, GameScreen oldGame)
        {
            this.IsPopup = true;
            this.levelNumber = levelNumber;

            this.title = "STAGE CLEAR !!!";

            if (newRecord)
            {
                this.details = "SCORE:    " + (int)score + " sec" + "\n" +
                               extraCongratulationsText + "\n\n" +
                               highscoreList + "\n   " +
                               "CONGRATULATIONS!\n   " +
                               name + "\n   " +
                               "YOU SET A NEW RECORD!";
            }
            else
            {
                this.details = "SCORE:    " + (int)score + " sec" + "\n" +
                                extraCongratulationsText + "\n\n" +
                              highscoreList;
            }
            TransitionOnTime = TimeSpan.FromSeconds(.2f);
            TransitionOffTime = TimeSpan.FromSeconds(.2f);

            this.oldGame = oldGame;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (levelNumber == XMLLevelParser.CurrentWorldNumberOfLevels() - 1)
            {
                MenuEntries.Add("Last Level");
            }
            else if (levelNumber != XMLLevelParser.CurrentWorldNumberOfLevels())
            {
                MenuEntries.Add("Next Level");
            }            
           
            MenuEntries.Add("Retry");
            MenuEntries.Add("Main Menu");

            //setup a proper left border for the menu options
            LeftBorder = 350;
        }

        protected override void OnSelectEntry(int entryIndex)
        {
            ScreenManager.RemoveScreen(oldGame);
            ExitScreen();
            switch (entryIndex)
            {
                case 0:
                        if (levelNumber == XMLLevelParser.CurrentWorldNumberOfLevels())
                        {
                            LoadLevel(oldGame, levelNumber);
                        }
                        else
                        {
                            LoadLevel(oldGame, levelNumber + 1);
                        }
                        ScreenManager.victoryMusicInstance.Stop();
                        break;
                case 1:
                        if (MenuEntries.Count == 2)
                            goto case 2;
                        ScreenManager.victoryMusicInstance.Stop();
                        LoadLevel(oldGame, levelNumber);
                        break;    
                case 2:
                        ScreenManager.victoryMusicInstance.Stop();
                        ScreenManager.AddScreen(new MainMenuScreen());                       
                        break;
            }
        }

        public override void LoadContent()
        {
            detailsFont = ScreenManager.ContentManager.Load<SpriteFont>(@"Content\Fonts\detailsFont.spritefont");            
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            Vector2 panelOrigin = new Vector2(ScreenManager.ScreenCenter.X - panelWidth / 2, ScreenManager.ScreenCenter.Y - panelHeight / 2);
            instructionsBatch.DrawRectangle(panelOrigin, panelWidth, panelHeight, panelColor);

            Vector2 textPanelTexturePosition = new Vector2(ScreenManager.ScreenCenter.X - panelWidth / 2 + textPanelLeftBorder, ScreenManager.ScreenCenter.Y - textPanelHeight / 2);
            instructionsBatch.DrawRectangle(textPanelTexturePosition, panelWidth, panelHeight, textPanelColor);

            Vector2 titlePosition = textPanelTexturePosition + new Vector2(textLeftBorder, textTopBorder);
            instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, title, titlePosition, textColor);

            Vector2 detailsPosition = titlePosition + new Vector2(0, 40);
            instructionsBatch.DrawString(detailsFont, details, detailsPosition, textColor);

            base.Draw(gameTime, instructionsBatch);
        }

        public override void HandleInput(InputState input)
        {
            if (input.MenuCancel)
            {
                ExitScreen();
            }
            base.HandleInput(input);
        }

        protected override void OnCancel()
        {
            ScreenManager.AddScreen(new MainMenuScreen());
        }

        protected void LoadLevel(GameScreen oldGame, int newLevel)
        {
            if (oldGame.GetType() == typeof(MultiplayerLevel))
            {
                //we have a multiplayer level
                ScreenManager.AddScreen(new MultiplayerLevel(newLevel));
            }
            else
            {
                //we have a single player level
                ScreenManager.AddScreen(new SinglePlayerLevel(newLevel, ((SinglePlayerLevel)oldGame).AgentPane, ((SinglePlayerLevel)oldGame).Agents));
            }
        }
    }
}
