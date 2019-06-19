#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Collections.Generic;


#endregion

namespace GeometryFriends.Components
{

    internal class InGameConsole : DrawableGameComponent
    {
        ContentManager content;
        SpriteFont spriteFont;

        Queue<string> lines = new Queue<string>(50);

        System.Globalization.NumberFormatInfo format;
        
        public InGameConsole(IGraphicsDevice drawingDevice)
            : base(drawingDevice)
        {
            content = new ContentManager();

            format = new System.Globalization.NumberFormatInfo();
            format.NumberDecimalSeparator = ".";
        }

        public void print(string s)
        {
            this.lines.Enqueue(s);
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
            while (lines.Count > 50)
            {
                lines.Dequeue();
            }
        }


        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            int i = 0;
            foreach (string s in lines)
            {
                instructionsBatch.DrawString(spriteFont, s, new Vector2(100, 80 + i * spriteFont.LineSpacing), Color.White);
                ++i;
            }
        }
    }
}