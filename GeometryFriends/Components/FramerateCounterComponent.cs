#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Levels;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;


#endregion

namespace GeometryFriends.Components
{

    internal class FrameRateCounter : DrawableGameComponent
    {
        ContentManager content;
        SpriteFont spriteFont;

        float frameTime = 0;
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;

        System.Globalization.NumberFormatInfo format;


        public FrameRateCounter(IGraphicsDevice drawingDevice)
            : base(drawingDevice)
        {
            content = new ContentManager();

            format = new System.Globalization.NumberFormatInfo();
            format.NumberDecimalSeparator = ".";
        }


        protected override void LoadContent()
        {
            spriteFont = content.Load<SpriteFont>(@"Content\Fonts\frameRateCounterFont.spritefont");
        }

        protected override void UnloadContent()
        {
            content.Unload();
        }


        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            frameCounter++;

            if (frameRate > 0) { frameTime = 1000f / frameRate; }

            if (Level.IsDebugViewEnabled)
            {

                string fps = string.Format(format, "fps: {0}", frameRate);
                string ft = string.Format(format, " ft: {0:F}", frameTime);

                instructionsBatch.DrawString(spriteFont, fps, new Vector2(100, 15), Color.White);
            }
        }
    }
}