using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.ScreenSystem
{
    internal class DiagnosticsScreen : GameScreen
    {
        Vector2 panelTextureSize = new Vector2(200, 100);
        Vector2 panelTexturePosition = new Vector2(50, 50);
        public DiagnosticsScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0);
        }

        public override void LoadContent()
        {
            //panelTexture = DrawingSystem.DrawingHelper.CreateRectangleTexture(ScreenManager.GraphicsDevice, (int)panelTextureSize.X, (int)panelTextureSize.Y, new Color(new Vector4(0, 0, 0, .5f)));
            base.LoadContent();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            byte fade = TransitionAlpha;
        }
    }
}
