using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.LevelEditor;
using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.ScreenSystem
{
    class EditorLevelSelectScreen : LevelSelectScreen
    {
        public EditorLevelSelectScreen() : base(PlayerType.Multiplayer) //value passed is irrelevant, since the OnSelectEntry method is overridden
        { 
        }

        protected override void OnSelectEntry()
        {

            if (selectedEntry.X > 0 && selectedEntry.X < 4)
            {
                ScreenManager.menuMusicInstance.Stop();
                ExitScreen();
                ScreenManager.AddScreen(new EditorLevel((int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE) + (int)selectedEntry.Y + 1));
            }
            else if (selectedEntry.X == 4 && selectedEntry.Y == 0)
            {
                this.OnCancel();
            }
            else if (selectedEntry.X == 4 && selectedEntry.Y == 1)
            {
                if (previews.Count < 12)
                    ScreenManager.AddScreen(new EditorLevel());
                else
                    //todo: warn player he cannot add more levels to this world
                    return;
            }
        }

        public override void HandleInput(InputState input)
        {
            int index;
            // Move to the previous menu entry?            
            if (input.MenuUp)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {                    
                    index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < previews.Count)
                    {
                        previews[index].IsSelected = false;
                    }
                    else
                    {
                        try
                        {
                            previews[previews.Count - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                        }
                        selectedEntry.X = previews.Count / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = (previews.Count % PREVIEWS_PER_LINE - 1 < 0 ? 3 : previews.Count % PREVIEWS_PER_LINE - 1);
                    }

                }

                if (selectedEntry.X == 0)
                {
                    selectedEntry.X = 4;
                    selectedEntry.Y = 0;
                }
                else
                {
                    if (selectedEntry.X == 4 || selectedEntry.X == 1)
                    {
                        selectedEntry.Y = 0;
                    }
                    selectedEntry.X--;
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    while (index >= previews.Count)
                    {
                        selectedEntry.X--;
                        index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    }
                    if (index < previews.Count)
                        previews[index].IsSelected = true;
                    else
                    {
                        selectedEntry.X = (float)Math.Ceiling((float)previews.Count / (float)PREVIEWS_PER_LINE);
                        selectedEntry.Y = (previews.Count % PREVIEWS_PER_LINE - 1 < 0 ? 0 : previews.Count % PREVIEWS_PER_LINE - 1);
                        index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                        try
                        {
                            previews[index].IsSelected = true;
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < previews.Count)
                        previews[index].IsSelected = false;
                    else
                    {
                        previews[previews.Count - 1].IsSelected = false;
                        selectedEntry.X = previews.Count / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = (previews.Count % PREVIEWS_PER_LINE - 1 < 0 ? 3 : previews.Count % PREVIEWS_PER_LINE - 1);
                    }
                }

                if (selectedEntry.X == 4)
                {
                    selectedEntry.X = 0;
                }
                else
                {
                    selectedEntry.X++;
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    int minIndex = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE);
                    index = (int)(minIndex + selectedEntry.Y);
                    while (index >= previews.Count && index > minIndex)
                    {
                        index--;
                        selectedEntry.Y--;
                    }
                    if (index < previews.Count)
                        previews[index].IsSelected = true;
                    else
                    {
                        try
                        {
                            previews[previews.Count - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                        }
                        selectedEntry.X = 4;
                        selectedEntry.Y = 0;
                    }

                }
            }

            if (input.MenuLeft)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < previews.Count)
                        previews[index].IsSelected = false;
                    else
                        try
                        {
                            previews[previews.Count - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                        }

                }

                if (selectedEntry.X == 0)
                {
                    ChangeToPreviousWorld();
                }

                //for now there are only 2 options at last line: back and new level
                if (selectedEntry.X == 4)
                {
                    if (selectedEntry.Y == 0)
                    {
                        selectedEntry.Y = 1;
                    }
                    else
                    {
                        selectedEntry.Y = 0;
                    }
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    if (selectedEntry.Y == 0)
                        selectedEntry.Y = 3;
                    else
                        selectedEntry.Y--;
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < previews.Count)
                        previews[index].IsSelected = true;
                    else
                    {
                        try
                        {
                            previews[previews.Count - 1].IsSelected = true;
                        }
                        catch (Exception)
                        {
                        }
                        selectedEntry.X = previews.Count / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = (previews.Count % PREVIEWS_PER_LINE - 1 < 0 ? 3 : previews.Count % PREVIEWS_PER_LINE - 1);
                    }

                }
            }

            if (input.MenuRight)
            {
                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < previews.Count)
                        previews[index].IsSelected = false;
                    else
                        try
                        {
                            previews[previews.Count - 1].IsSelected = false;
                        }
                        catch (Exception)
                        {
                        }

                }

                if (selectedEntry.X == 0)
                {
                    ChangeToNextWorld();
                }

                //for now there are only 2 options at last line: back and new level
                if (selectedEntry.X == 4)
                {
                    if (selectedEntry.Y == 1)
                    {
                        selectedEntry.Y = 0;
                    }
                    else
                    {
                        selectedEntry.Y = 1;
                    }
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    if (selectedEntry.Y == 3)
                        selectedEntry.Y = 0;
                    else
                        selectedEntry.Y++;
                }

                if (selectedEntry.X > 0 && selectedEntry.X < 4)
                {
                    index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                    if (index < previews.Count)
                        previews[index].IsSelected = true;
                    else
                    {
                        selectedEntry.X = previews.Count / PREVIEWS_PER_LINE + 1;
                        selectedEntry.Y = 0;
                        index = (int)((selectedEntry.X - 1) * PREVIEWS_PER_LINE + selectedEntry.Y);
                        try
                        {
                            previews[index].IsSelected = true;
                        }
                        catch (Exception)
                        {
                        }
                    }

                }
            }

            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                this.OnSelectEntry();
            }
            else if (input.MenuCancel)
            {
                this.OnCancel();
            }
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.Clear(Color.White);

            string title = XMLLevelParser.GetWorldName();
            float titleX = instructionsBatch.MeasureString(title, ScreenManager.TitleFont).X / 2;
            instructionsBatch.DrawString(ScreenManager.TitleFont, title, currentWorldStringPosition + new Vector2(-titleX, 0), (selectedEntry.X != 0) ? Color.Black : Color.Red);
            title = XMLLevelParser.GetPrevWorldName();
            float newTitleX = instructionsBatch.MeasureString(title, ScreenManager.TitleFont).X;
            instructionsBatch.DrawString(ScreenManager.TitleFont, title, currentWorldStringPosition + new Vector2(-200 - titleX - newTitleX / 10, 50), 0.2f, Color.DarkGray);
            title = XMLLevelParser.GetNextWorldName();
            newTitleX = instructionsBatch.MeasureString(title, ScreenManager.TitleFont).X;
            instructionsBatch.DrawString(ScreenManager.TitleFont, title, currentWorldStringPosition + new Vector2(titleX + 200 - newTitleX / 10, 50), 0.2f, Color.DarkGray);

            //drawing "Back" option
            if (selectedEntry.X == 4 && selectedEntry.Y == 0)
                instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, "Back", new Vector2(100, 750), Color.Red);
            else
            {
                instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, "Back", new Vector2(100, 750), Color.Black);                
            }

            //Drawing "New LEvel..." option
            if (selectedEntry.X == 4 && selectedEntry.Y == 1)
            {
                instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, "New Level...", new Vector2(300, 750), Color.Red);
            }
            else
            {
                instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, "New Level...", new Vector2(300, 750), Color.Black);
            }
                
            byte fade = 255;// TransitionAlpha;                          

            Color tint = new Color(fade, fade, fade, fade);

            Vector2 itemPosition = new Vector2(position.X, position.Y);

            foreach (LevelPreview p in previews)
            {
                p.Draw(gameTime, instructionsBatch);
                if (p.IsSelected)
                    instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, p.GetExtraCongratulationsText().Split('\n')[0], new Vector2(p.GetPosition().X + 50, p.GetPosition().Y + 150), Color.Red);
                else
                    instructionsBatch.DrawString(ScreenManager.MenuSpriteFont, p.GetExtraCongratulationsText().Split('\n')[0], new Vector2(p.GetPosition().X + 50, p.GetPosition().Y + 150), Color.Black);
            }                         
        }

        protected override void OnCancel()
        {
            ExitScreen();
            ScreenManager.AddScreen(new EditorMenuScreen());
        }
    }
}
