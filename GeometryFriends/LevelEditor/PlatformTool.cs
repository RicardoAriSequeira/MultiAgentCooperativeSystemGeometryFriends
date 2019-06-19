using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.LevelEditor
{
    abstract class PlatformTool : Tool
    {
        public delegate bool PlatformToolHandler(Vector2 dim, Vector2 pos);
        public event PlatformToolHandler ToolFinished;
        protected Texture2D rectangleTexture;

        public override void Draw(DrawingInstructionsBatch instructionsBatch, GameTime game)
        {
            //draw icon
            instructionsBatch.DrawTexture(iconTexture, iconPosition, iconTexture.Width, iconTexture.Height, Color.PaleGreen);
            //draw cursor
            instructionsBatch.DrawTexture(cursorTexture, position, cursorTexture.Width, cursorTexture.Height, Color.White);

            if (this.state == ToolState.DRAGGING)
            {
                System.Drawing.Rectangle area = RectangleDrawingArea();
                instructionsBatch.DrawTexture(rectangleTexture, new Vector2(area.X, area.Y), area.Width, area.Height, Color.White);
            }
        }

        protected System.Drawing.Rectangle RectangleDrawingArea()
        {
            System.Drawing.Rectangle result = new System.Drawing.Rectangle(0, 0, 0, 0);
            float rectWidth, rectHeight = 0;

            rectWidth = position.X - dragOrigin.X;
            rectHeight = position.Y - dragOrigin.Y;

            if (rectWidth > 0 && rectHeight > 0)
            {
                result = new System.Drawing.Rectangle((int)(dragOrigin.X), (int)(dragOrigin.Y), (int)(position.X - dragOrigin.X), (int)(position.Y - dragOrigin.Y));
            }
            if (rectWidth < 0 && rectHeight < 0)
            {
                result = new System.Drawing.Rectangle((int)(position.X), (int)(position.Y), (int)(Math.Abs(position.X - dragOrigin.X)), (int)(Math.Abs(position.Y - dragOrigin.Y)));
            }
            if (rectWidth > 0 && rectHeight < 0)
            {
                result = new System.Drawing.Rectangle((int)(dragOrigin.X), (int)(position.Y), (int)(position.X - dragOrigin.X), (int)(Math.Abs(position.Y - dragOrigin.Y)));
            }
            if (rectWidth < 0 && rectHeight > 0)
            {
                result = new System.Drawing.Rectangle((int)(position.X), (int)(dragOrigin.Y), (int)(Math.Abs(position.X - dragOrigin.X)), (int)(position.Y - dragOrigin.Y));
            }
            return result;
        }

        public override void HandleInput(InputState input, Vector2 cursorPosition)
        {
            this.position = cursorPosition;

            this.previousState = this.state;

            if (input.wiiInput.isWiimoteOn(WiimoteNumber.WII_1))
            {
                if (input.wiiInput.IsKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_A))
                {
                    if (this.previousState == ToolState.STANDBY)
                    {
                        this.state = ToolState.ACTIVE;
                    }

                    if (this.previousState == ToolState.ACTIVE)
                    {
                        this.dragOrigin = cursorPosition;
                        this.state = ToolState.DRAGGING;
                    }
                }
                else
                {
                    this.state = ToolState.STANDBY;
                }

                if (input.wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_B))
                {
                    this.state = ToolState.IDLE;
                }

                if (this.state == ToolState.STANDBY && this.previousState == ToolState.DRAGGING)
                {
                    System.Drawing.Rectangle r = RectangleDrawingArea();
                    if (r.X > 0 && r.Y > 0 && r.Width > 0 && r.Height > 0)
                        ToolFinished.Invoke(new Vector2(r.Width, r.Height), new Vector2(r.X, r.Y));
                    //create a platform with the rectangleDrawingArea() dimensions
                }
            }
            else
            {
                if (input.CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    input.LastMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (this.previousState == ToolState.STANDBY)
                    {
                        this.state = ToolState.ACTIVE;
                    }

                    if (this.previousState == ToolState.ACTIVE)
                    {
                        this.dragOrigin = cursorPosition;
                        this.state = ToolState.DRAGGING;
                    }
                }
                else
                {
                    this.state = ToolState.STANDBY;
                }

                if (input.CurrentKeyboardState.IsKeyDown(Keys.Escape) &&
               !input.LastKeyboardState.IsKeyDown(Keys.Escape)
                   || input.CurrentMouseState.RightButton == ButtonState.Released &&
                   input.LastMouseState.RightButton == ButtonState.Pressed)
                {
                    this.state = ToolState.IDLE;
                }

                if (this.state == ToolState.STANDBY && this.previousState == ToolState.DRAGGING)
                {
                    System.Drawing.Rectangle r = RectangleDrawingArea();
                    if (r.X > 0 && r.Y > 0 && r.Width > 0 && r.Height > 0)
                        ToolFinished.Invoke(new Vector2(r.Width, r.Height), new Vector2(r.X, r.Y));
                    //create a platform with the rectangleDrawingArea() dimensions
                }
            }
        }        

        public override void Unload()
        {
        }
    }
}
