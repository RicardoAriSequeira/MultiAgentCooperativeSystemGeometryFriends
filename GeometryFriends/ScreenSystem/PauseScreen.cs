using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.Levels;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.ScreenSystem
{
    class PauseScreen : MenuScreen
    {
        string title = "Title";
        string details = "Details";

        Color panelColor = new Color(100, 100, 100, 200);

        int levelNumber;
        Color textPanelColor = new Color(100, 100, 100, 220);

        protected bool firstTime;

        Color textColor = GameColors.MENU_SCREEN_COLOR;
        SpriteFont detailsFont;

        GameScreen oldGame;

        public float PauseMenuLeft { get { return ScreenManager.ScreenWidth * 0.2f; } }
        public float PauseMenuTop { get { return ScreenManager.ScreenHeight * 0.2f; } }
        public float PauseMenuWidth { get { return ScreenManager.ScreenWidth * 0.6f; } }
        public float PauseMenuHeight { get { return ScreenManager.ScreenHeight * 0.6f; } }
        private const int TITLE_TOP_MARGIN = 20;

        public PauseScreen(string title, string details, int levelNumber, bool firstTime, GameScreen oldGame)
        {
            this.IsPopup = true;
            this.title = title;
            this.details = details;
            this.levelNumber = levelNumber;
            TransitionOnTime = TimeSpan.FromSeconds(.2f);
            TransitionOffTime = TimeSpan.FromSeconds(.2f);
            this.firstTime = firstTime;
            this.oldGame = oldGame;
        }

        public override void Initialize()
        {
            base.Initialize();
            if (firstTime)
                MenuEntries.Add("Start!");
            else
                MenuEntries.Add("Resume");
            MenuEntries.Add("Restart");
            MenuEntries.Add("Quit");
        }

        protected override void OnSelectEntry(int entryIndex)
        {
            switch (entryIndex)
            {
                case 0:
                    if(GeometryFriends.Properties.Settings.Default.musicOn)
                        ScreenManager.levelMusicInstance.Resume();
                    ExitScreen();
                    break;

                case 1:
                    ExitScreen();
                    ScreenManager.RemoveScreen(oldGame);
                    if(Level.isMulti)
                        ScreenManager.AddScreen(new MultiplayerLevel(levelNumber));
                    else
                        ScreenManager.AddScreen(new SinglePlayerLevel(levelNumber, ScreenManager.AgentPane, ((SinglePlayerLevel)oldGame).Agents));
                break;

                case 2:                
                    ExitScreen();
                    ScreenManager.RemoveScreen(oldGame);
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
            instructionsBatch.DrawRectangle(new Vector2(PauseMenuLeft, PauseMenuTop), (int)PauseMenuWidth, (int)PauseMenuHeight, panelColor);

            //Vector2 textPanelTexturePosition = new Vector2(ScreenManager.ScreenCenter.X - panelWidth / 2 + textPanelLeftBorder, ScreenManager.ScreenCenter.Y - textPanelHeight / 2);
            //instructionsBatch.DrawRectangle(textPanelTexturePosition, panelWidth, panelHeight, textPanelColor);

            //Vector2 titlePosition = textPanelTexturePosition + new Vector2(textLeftBorder, textTopBorder);
            Vector2 titleSize = instructionsBatch.MeasureString(title, ScreenManager.MenuSpriteFont);
            Vector2 titlePosition = new Vector2(ScreenManager.ScreenWidth / 2 - titleSize.X, PauseMenuTop + TITLE_TOP_MARGIN + titleSize.Y); 
            instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, title, titlePosition, textColor);

            Vector2 detailsPosition = titlePosition + new Vector2(0, LINE_SPACING);
            instructionsBatch.DrawString(detailsFont, details, detailsPosition, textColor);

            //TODO: improve link between classes
            position = new Vector2(PauseMenuLeft + PauseMenuWidth * 0.1f, detailsPosition.Y + LINE_SPACING);

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
    }
}
