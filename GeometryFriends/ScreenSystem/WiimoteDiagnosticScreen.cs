#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
#endregion

namespace GeometryFriends.ScreenSystem
{
    /// <summary>
    /// This is the screen responsible by showing information related to the wiimote.
    /// it should also warn the player in case no wiimote is connected, giving the option to 
    /// continue or retry to detect a wiimote.
    /// </summary>
    class WiimoteDiagnosticScreen : GameScreen
    {      
        SpriteFont screenSpriteFont;
        ContentManager contentManager;
        Texture2D wiimoteTexture;
        Vector2 origin;

        public WiimoteDiagnosticScreen()
        {
            this.IsPopup = true;            
            TransitionOnTime = TimeSpan.FromSeconds(.5);
            TransitionOffTime = TimeSpan.FromSeconds(.5);
        }

        public override void LoadContent()
        {
            if (contentManager == null)
                contentManager = new ContentManager();

            wiimoteTexture = contentManager.Load<Texture2D>("Content/Common/wiimote.png");
            screenSpriteFont = ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/menuFont.spritefont");
            origin = new Vector2(wiimoteTexture.Width / 2f, wiimoteTexture.Height / 2f);
        }

        public override void UnloadContent()
        {
            contentManager.Unload();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {        
           base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.Clear(Color.White);

            byte fade = (byte)(255 * (1 - TransitionPosition));// TransitionAlpha;               

            Color tint = new Color(255, 255, 255, fade);

            instructionsBatch.DrawTexture(wiimoteTexture, new Vector2(ScreenManager.ScreenWidth - wiimoteTexture.Width * 2, ScreenManager.ScreenHeight - wiimoteTexture.Height * 2), wiimoteTexture.Width * 2, wiimoteTexture.Height * 2, tint);

            instructionsBatch.DrawString(screenSpriteFont, "Please Connect your Wiimote or press (Esc) to continue", new Vector2(120, 200), new Color(0, 0, 0, fade));
        }

        public override void HandleInput(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.Escape)) 
                ExitScreen();
            //if (input.CurrentGamePadState.Buttons.X == ButtonState.Pressed) ExitScreen();

            if (input.wiiInput.isWiimoteOn(WiimoteNumber.WII_1))
            {                
                ExitScreen();
            }
            base.HandleInput(input);
        }
    }
}
