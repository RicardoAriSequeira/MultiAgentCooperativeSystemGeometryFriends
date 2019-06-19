using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.ScreenSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.LevelEditor
{
    abstract class DialogTool : Tool
    {
        public delegate bool DialogToolHandler1();
        public delegate bool DialogToolHandler2();
        public event DialogToolHandler1 DialogOption1Selected;
        public event DialogToolHandler2 DialogOption2Selected;

        protected Texture2D dialogTexture;
        
        protected String option1 = "Yes";
        protected String option2 = "No";
        protected String dialogText = "";
        protected Color selectedColor = Color.Blue;
        
        protected Vector2 dialogPosition;
        protected Vector2 dialogTextPosition;
        protected Vector2 option1Position;
        protected Vector2 option2Position;
        protected SpriteFont dialogSpriteFont;

        public override void Draw(DrawingInstructionsBatch instructionsBatch, GameTime game)
        {            
            //these calculations are being done every frame, which is not good performance wise. But then again, its a dialog window: everything else should be paused anyway
            dialogPosition = new Vector2((instructionsBatch.DWidth - dialogTexture.Width) / 2, (instructionsBatch.DHeight - dialogTexture.Height) / 2);
            option1Position = new Vector2((dialogPosition.X + (dialogTexture.Width / 4)), (dialogPosition.Y + dialogTexture.Height - 30));
            option2Position = new Vector2((dialogPosition.X + (dialogTexture.Width * 3 / 4)), (dialogPosition.Y + dialogTexture.Height - 30));
            dialogTextPosition = new Vector2((dialogPosition.X + 5), (dialogPosition.Y + 50));

            //draw icon
            instructionsBatch.DrawTexture(iconTexture, iconPosition, iconTexture.Width, iconTexture.Height, Color.PaleGreen);
            
            //draw dialog window
            instructionsBatch.DrawTexture(dialogTexture, dialogPosition, dialogTexture.Width, dialogTexture.Height, Color.White);
            instructionsBatch.DrawString(dialogSpriteFont, dialogText, dialogTextPosition, Color.Black);

            //draw options
            if (IsOverOption1())
            {
                instructionsBatch.DrawString(dialogSpriteFont, option1, option1Position, selectedColor);
                instructionsBatch.DrawString(dialogSpriteFont, option2, option2Position, Color.Black);
            }
            else
            {
                if (IsOverOption2())
                {
                    instructionsBatch.DrawString(dialogSpriteFont, option2, option2Position, selectedColor);
                    instructionsBatch.DrawString(dialogSpriteFont, option1, option1Position, Color.Black);
                }
                else
                {
                    instructionsBatch.DrawString(dialogSpriteFont, option1, option1Position, Color.Black);
                    instructionsBatch.DrawString(dialogSpriteFont, option2, option2Position, Color.Black);
                }
            }            

            //draw cursor            
            instructionsBatch.DrawTexture(cursorTexture, position, cursorTexture.Width, cursorTexture.Height, Color.White);
        }

        public override void HandleInput(InputState input, Vector2 cursorPosition)
        {
            this.position = cursorPosition;

            this.previousState = this.state;

            if (input.wiiInput.isWiimoteOn(WiimoteNumber.WII_1))
            {
                if (input.wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_A))
                {
                    if (IsOverOption1())
                    {
                        if (!DialogOption1Selected.Invoke())
                        {
                            this.dialogText = "WARNING: cannot save! Missing character start point or at least 1 collectible. Press Esc to edit level or No to exit";
                        }
                        else
                        {
                            this.state = ToolState.IDLE;
                        }
                    }

                    if (IsOverOption2())
                    {
                        DialogOption2Selected.Invoke();
                        this.state = ToolState.IDLE;
                    }
                }

                if (input.wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_B))
                {
                    this.state = ToolState.IDLE;
                }
            }
            else
            {
                if (input.CurrentMouseState.LeftButton == ButtonState.Released &&
                    input.LastMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (IsOverOption1())
                    {
                        if (!DialogOption1Selected.Invoke())
                        {
                            this.dialogText = "WARNING: cannot save! Missing character start point or at least 1 collectible. Press Esc to edit level or No to exit";
                        }
                        else
                        {
                            this.state = ToolState.IDLE;
                        }
                    }

                    if (IsOverOption2())
                    {
                        DialogOption2Selected.Invoke();
                        this.state = ToolState.IDLE;
                    }
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

        protected bool IsOverOption1()
        {
            return (this.position.X > option1Position.X &&
                this.position.X < option1Position.X + GameScreen.LINE_SPACING * option1.Length &&
                this.position.Y > option1Position.Y &&
                this.position.Y < option1Position.Y + 50);
        }

        protected bool IsOverOption2()
        {
            return (this.position.X > option1Position.X &&
                this.position.X < option2Position.X + GameScreen.LINE_SPACING * option2.Length &&
                this.position.Y > option2Position.Y &&
                this.position.Y < option2Position.Y + 50);
        }

        public override void Unload()
        {
        }
    }
}
