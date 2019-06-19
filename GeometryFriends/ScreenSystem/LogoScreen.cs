#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
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
    class LogoScreen : GameScreen
    {
        ContentManager contentManager;
        Texture2D farseerLogoTexture;
        Vector2 origin;
        bool wiimoteOn;

        public LogoScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(.75);
            TransitionOffTime = TimeSpan.FromSeconds(.75);
        }

        public override void LoadContent()
        {
            if (contentManager == null)
                contentManager = new ContentManager();

            farseerLogoTexture = contentManager.Load<Texture2D>("Content/Common/logo.png");
            origin = new Vector2(farseerLogoTexture.Width / 2f, farseerLogoTexture.Height / 2f);
        }

        public override void UnloadContent()
        {
            contentManager.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
           if (this.TransitionPosition == 0 && ScreenState == ScreenState.Active)
           {
               ExitScreen();
           }
           if (ScreenState == ScreenState.TransitionOff && TransitionPosition > .9f)
           {
                ScreenManager.RemoveScreen(this);
                ScreenManager.AddScreen(new BackgroundScreen());
                ScreenManager.AddScreen(new MainMenuScreen());
                if (!wiimoteOn)
                {
                    ScreenManager.AddScreen(new WiimoteDiagnosticScreen());
                }               
           }
            
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.Clear(GameColors.LOGO_BACKGROUND);

            byte fade = (byte) (255 * (1 - TransitionPosition));// TransitionAlpha;               

            Color tint = new Color(255, 255, 255, fade);

            Vector2 logoPosition = new Vector2(ScreenManager.ScreenCenter.X - farseerLogoTexture.Width / 2, ScreenManager.ScreenCenter.Y - farseerLogoTexture.Height / 2);

            instructionsBatch.DrawTexture(farseerLogoTexture, logoPosition, farseerLogoTexture.Width, farseerLogoTexture.Height, tint);
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Escape)) 
                ScreenManager.Game.Exit();
            //if (input.CurrentGamePadState.Buttons.X == ButtonState.Pressed) ScreenManager.Game.Exit();
            wiimoteOn = input.wiiInput.isWiimoteOn(0);
            base.HandleInput(input);
        }
    }
}
