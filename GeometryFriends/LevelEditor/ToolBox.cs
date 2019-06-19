using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.ScreenSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Collections.Generic;


namespace GeometryFriends.LevelEditor
{
    class ToolBox
    {        
        private const int TOOLBOX_POS_X = 0;
        private const int TOOLBOX_POS_Y = 12;

        private SpriteFont toolboxFont;
        private Vector2 position;
        //private Texture2D toolBoxTexture;
        private ContentManager contentManager;
        private List<Tool> tools;
        private Tool activeTool;
        private EditorCursor cursor;
        //private int currentTool = -1;

        public ToolBox(EditorLevel level)
        {            
            this.position = new Vector2(TOOLBOX_POS_X, TOOLBOX_POS_Y);
            tools = new List<Tool>();
            tools.Add(new BlackPlatformTool());
            ((BlackPlatformTool)tools[0]).ToolFinished += new PlatformTool.PlatformToolHandler(level.AddBlackObstacle);
            tools.Add(new GreenPlatformTool());
            ((GreenPlatformTool)tools[1]).ToolFinished += new PlatformTool.PlatformToolHandler(level.AddGreenObstacle);
            tools.Add(new YellowPlatformTool());
            ((YellowPlatformTool)tools[2]).ToolFinished += new PlatformTool.PlatformToolHandler(level.AddYellowObstacle);
            tools.Add(new CircleStartTool());
            ((CircleStartTool)tools[3]).ToolFinished += new PositionTool.PositionToolHandler(level.SetCircleStart);
            tools.Add(new RectangleStartTool());
            ((RectangleStartTool)tools[4]).ToolFinished += new PositionTool.PositionToolHandler(level.SetRectangleStart);
            tools.Add(new CollectibleTool());
            ((CollectibleTool)tools[5]).ToolFinished += new PositionTool.PositionToolHandler(level.AddCollectible);
            tools.Add(new EraserTool());
            ((EraserTool)tools[6]).ToolFinished += new PositionTool.PositionToolHandler(level.DeleteShapeAtPosition);
            tools.Add(new ExitTool());
            ((ExitTool)tools[7]).DialogOption1Selected += new DialogTool.DialogToolHandler1(level.SaveGameContent);
            ((ExitTool)tools[7]).DialogOption2Selected += new DialogTool.DialogToolHandler2(level.ExitLevelEditor);

            this.cursor = new EditorCursor(level);
        }

        public void LoadContent(ScreenManager screenManager)
        {
            Vector2 offset = position;

            if (contentManager == null)
                contentManager = new ContentManager();
            toolboxFont = screenManager.ContentManager.Load<SpriteFont>("Content/Fonts/menuFont.spritefont");

            cursor.Load(screenManager.GraphicsDevice);

            foreach (Tool t in tools)
            {
                t.LoadContent(contentManager);
                t.SetIconPosition(offset);
                offset.Y += t.GetIconTexture().Height;
            }
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch, GameTime gameTime)
        {
            Color tint = Color.White;            
            tint.A = 155;            

            if (activeTool == null)
            {                                          
                foreach (Tool t in tools)
                {
                    instructionsBatch.DrawTexture(t.GetIconTexture(), t.GetIconPosition(), t.GetIconTexture().Width, t.GetIconTexture().Height, tint);
                }

                cursor.Draw(instructionsBatch, gameTime);      
            }
            else
            {
                activeTool.Draw(instructionsBatch, gameTime);
                DrawToolName(instructionsBatch, gameTime);
                DrawTooltip(instructionsBatch, gameTime);
            }
        }

        private void DrawToolName(DrawingInstructionsBatch instructionsBatch, GameTime gameTime)
        {
            instructionsBatch.DrawString(toolboxFont, activeTool.GetName() + "  (press Esc key to exit this tool)", new Vector2(45, 5), Color.Aquamarine);
            //TODO: por isto mais bonito
        }

        private void DrawTooltip(DrawingInstructionsBatch instructionsBatch, GameTime gameTime)
        {
            instructionsBatch.DrawString(toolboxFont, activeTool.GetDescription(), new Vector2(45, instructionsBatch.DHeight - 40), Color.Aquamarine);
            //TODO: por isto mais bonito
        }

        public void HandleInput(InputState input)
        {
            this.cursor.HandleInput(input);            
         
          //if no tool is selected, select the new one
            if (activeTool == null)
            {
                if (input.CurrentMouseState.LeftButton == ButtonState.Pressed &&
                    input.LastMouseState.LeftButton == ButtonState.Released ||
                    input.wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_A))
                {
                    foreach (Tool t in tools)
                    {
                        if (IsOverTexture(this.cursor.Position, t))
                        {
                            this.activeTool = t;
                            activeTool.Activate();                            
                            input.Update();
                        }
                    }
                }
            }
            else
            {
                if (activeTool.GetState() == ToolState.IDLE)
                {
                    this.activeTool = null;
                }
                else
                {
                    activeTool.HandleInput(input, cursor.Position);
                }
            }
        }
                   
        private bool IsOverTexture(Vector2 mousePos, Tool t)
        {
            int tX, tY, tH, tW;

            tX = (int)t.GetIconPosition().X;
            tY = (int)t.GetIconPosition().Y;
            tH = t.GetIconTexture().Height;
            tW = t.GetIconTexture().Width;

            return mousePos.X > tX &&
                mousePos.X < (tW + tX) &&
                mousePos.Y > tY &&
                mousePos.Y < (tY + tH);
        }

       /* public void selectTool(CursorTool tool)
        {
            int toolNum = -1;
            switch (tool)
            {
                case CursorTool.BLACK_RECT:
                    toolNum = 0;
                    break;
                case CursorTool.GREEN_RECT:
                    toolNum = 1;
                    break;
                case CursorTool.YELLOW_RECT:
                    toolNum = 2;
                    break;
                case CursorTool.BALL_START:
                    toolNum = 3;
                    break;
                case CursorTool.SQUARE_START:
                    toolNum = 4;
                    break;
                case CursorTool.ELEVATOR:
                    toolNum = 5;
                    break;
                case CursorTool.COLLECTIBLE:
                    toolNum = 6;
                    break;
                case CursorTool.DELETE:
                    toolNum = 7;
                    break;
                case CursorTool.MOVE:
                    toolNum = 8;
                    break;
                case CursorTool.RESIZE:
                    toolNum = 9;
                    break;
                case CursorTool.SAVE:
                    toolNum = 10;
                    break;
                case CursorTool.EXIT:
                    toolNum = 11;
                    break;
            }
            this.selectTool(toolNum);
        }

        private void selectTool(int newTool)
        {
            this.currentTool = newTool;
        }*/

        ///<summary>
        ///returns the selected tool 
        ///</summary>
        public Tool GetSelectedTool()
        {
            return activeTool;
        }

        public void Unload()
        {
            cursor.Unload();

            foreach (Tool t in tools)
            {
                t.Unload();
            }
        }
    }    
}
