
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;

namespace GeometryFriends.LevelEditor
{
   abstract class PositionTool : Tool
    {
        public delegate void PositionToolHandler(Vector2 pos);
        public event PositionToolHandler ToolFinished;

        public override void Draw(DrawingInstructionsBatch instructionsBatch, GameTime game)
        {
            //draw icon
            instructionsBatch.DrawTexture(iconTexture, iconPosition, iconTexture.Width, iconTexture.Height, Color.PaleGreen);
            //draw cursor
            instructionsBatch.DrawTexture(cursorTexture, position, cursorTexture.Width, cursorTexture.Height, Color.White);
        }
       
        public override void HandleInput(InputState input, Vector2 cursorPosition)
        {
            this.position = cursorPosition;

            if (input.wiiInput.isWiimoteOn(WiimoteNumber.WII_1))
            {
                if (input.wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_A))
                {
                    ToolFinished.Invoke(new Vector2(this.position.X, this.position.Y));
                }

                if (input.wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_B))
                {
                    this.state = ToolState.IDLE;
                }
            }
            else
            {
                if (input.LastMouseState.LeftButton == ButtonState.Released && input.CurrentMouseState.LeftButton == ButtonState.Pressed)
                {
                    ToolFinished.Invoke(new Vector2(this.position.X, this.position.Y));
                }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.Escape) &&
                    !input.LastKeyboardState.IsKeyDown(Keys.Escape)
                    || input.CurrentMouseState.RightButton == ButtonState.Released &&
                    input.LastMouseState.RightButton == ButtonState.Pressed)
                {
                    this.state = ToolState.IDLE;
                }
            }
        }        

        public override void Unload()
        {
        }
    }    
}
