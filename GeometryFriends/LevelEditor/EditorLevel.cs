using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using GeometryFriends.ScreenSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Linq;


namespace GeometryFriends.LevelEditor
{
    internal enum ElevatorDirections
    {
        HOR_LEFT,
        HOR_RIGHT,
        VER_UP,
        VER_DOWN
    }

    internal class EditorLevel : XMLLevel, IXmlSerializable
    {
        private ToolBox toolBox;
        private bool isCircleStartSet, isRectangleStartSet;
        public const int GRID_SNAP_SIZE = 16;

        private Vector2 GRID_DIMENSIONS;       
        public Vector2 GridDimensions
        {
            get
            {
                return GRID_DIMENSIONS;
            }
        }                

        private Vector2 GRID_OFFSET;
        public Vector2 GridOffset 
        {
            get
            {
                return GRID_OFFSET;
            }
        }

        private LineBrush gridBrush;

        /// <summary>
        /// Constructor used when one wants to create an empty EditorLevel to create a new level.
        /// </summary>
        public EditorLevel()
        {                      
            //this.borderWidth = (int)(ScreenManager.ScreenHeight * .05f);
            this.toolBox = new ToolBox(this);
            gridBrush = new LineBrush(1, new Color(50, 205, 50, 25));
            this.levelNumber = XMLLevelParser.CurrentWorldNumberOfLevels() + 1;
            isCircleStartSet = isRectangleStartSet = false;
        }

        /// <summary>
        /// Creates an EditorLevel in order to edit the level passed as parameter. 
        /// Use the constructor with no params if you wish to create a new level.
        /// </summary>
        /// <param name="lvlNum"> number of the level to edit. must be a positive number.</param>
        public EditorLevel(int lvlNum) : base(lvlNum)
        {            
            this.toolBox = new ToolBox(this);
            gridBrush = new LineBrush(1, new Color(50, 205, 50, 25));
            isCircleStartSet = isRectangleStartSet = true;            
        }

       public override void LoadContent()
        {
            toolBox.LoadContent(ScreenManager);                        
            base.LoadContent();
        }

       public override void LoadLevelContent()
       {
           if (!isCircleStartSet || !isRectangleStartSet)
           {
               border = new Border(ScreenManager.ScreenWidth, ScreenManager.ScreenHeight, borderWidth, ScreenManager.ScreenCenter);
               border.Load(base.GetPhysicsSimulator());               

               this.LoadCircleCharacter(new CircleCharacter(new Vector2(-1000, -1000)));
               this.LoadRectangleCharacter(new RectangleCharacter(new Vector2(-1000, -1000)));
           }
           else
           {
               base.LoadLevelContent();
           }

           this.GRID_DIMENSIONS = new Vector2(ScreenManager.ScreenWidth - this.borderWidth, ScreenManager.ScreenHeight - this.borderWidth);
           this.GRID_OFFSET = new Vector2(this.borderWidth, this.borderWidth);
           
       }        
        /// <summary>
        /// This method saves the Level content onto the corresponding file.
        /// </summary>
       public bool SaveGameContent()
       {
           if ((isCircleStartSet || isRectangleStartSet) && this.collectibles.Count > 0)
           {
               this.parser.SaveLevel(this.ToXml(XMLLevelParser.xmlDocument), this.levelNumber);
               return true;
           }
           else
           {
               //TODO Mensagem de erro a dizer o que é obrigatório
           }
           return false;
       }

        /// <summary>
        /// Unload content from memory.
        /// </summary>
        public override void UnloadContent()
        {            
            toolBox.Unload();
            base.UnloadContent();
        }

        //em principio fazer override ao update resolve o meu problema de ter o physics simulator a funcionar no modo de edição
        //(assim como toda a logica de jogo)
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {            
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            DrawGrid(instructionsBatch);
            this.DrawLevel(instructionsBatch);
            toolBox.Draw(instructionsBatch, gameTime);            
        }

        protected void DrawGrid(DrawingInstructionsBatch instructionsBatch)
        {
            for (int i = (int)GRID_OFFSET.X; i <= GRID_DIMENSIONS.X; i += GRID_SNAP_SIZE)
            {
                gridBrush.Draw(instructionsBatch, new Vector2(i, GRID_OFFSET.Y), new Vector2(i, GRID_DIMENSIONS.Y));
            }

            for (int i = (int)GRID_OFFSET.Y; i <= GRID_DIMENSIONS.Y; i += GRID_SNAP_SIZE)
            {
                gridBrush.Draw(instructionsBatch, new Vector2(GRID_OFFSET.X, i), new Vector2(GRID_DIMENSIONS.X, i));
            }
        }

        protected override void HandleCircleControls(InputState input)
        {
            //Have to override abstract methods, but they are not supposed to be controlled in an editor level hence the empty method
        }

        protected override void HandleRectangleControls(InputState input)
        {
            //Have to override abstract methods, but they are not supposed to be controlled in an editor level hence the empty method
        }

        public override void HandleInput(InputState input)
        {
            toolBox.HandleInput(input);                              
        }

        //TODO: detectar a shape a editar para tools de editing
        private GeoShape DetectSelectedShape(Vector2 cursorPos)
        {
            foreach (GeoShape s in this.collectibles)
            {
                if (IsOverShape(s, cursorPos))
                {
                    return s;
                }
            }

            foreach (GeoShape s in this.obstacles)
            {
                if (IsOverShape(s, cursorPos))
                {
                    return s;
                }
            }

            // Return Circle Character
            if (IsOverShape(circle, cursorPos))
                return circle;

            // Return Rectangle Character
            if (IsOverShape(rectangle, cursorPos))
                return rectangle;

            return null;
        }

        public void DeleteShapeAtPosition(Vector2 pos)
        {
            GeoShape s = DetectSelectedShape(pos);
            if (s != null)
            {
                DeleteShape(s);
            }
        }
        
        private bool IsOverShape(GeoShape s, Vector2 cursorPos)
        {
            return (cursorPos.X > s.X - s.Width / 2 &&
                cursorPos.X < s.X + s.Width / 2 &&
                cursorPos.Y > s.Y - s.Height / 2 &&
                cursorPos.Y < s.Y + s.Height / 2);
        }

        private void DeleteShape(GeoShape s)
        {
            try
            {
                if (s is RectangleCharacter)
                {
                    DeleteRectangle();
                    return;
                }

                if (s is CircleCharacter)
                {
                    DeleteCircle();
                    return;
                }
                
                if (obstacles.Contains((RectanglePlatform)s))
                {
                    obstacles.Remove((RectanglePlatform)s);
                    if (blackObstacles.Contains((RectanglePlatform)s))
                        blackObstacles.Remove((RectanglePlatform)s);
                    if (greenObstacles.Contains((RectanglePlatform)s))
                        greenObstacles.Remove((RectanglePlatform)s);
                    if (yellowObstacles.Contains((RectanglePlatform)s))
                        yellowObstacles.Remove((RectanglePlatform)s);
                    return;
                }
            }
            catch (InvalidCastException)
            {
                if (collectibles.Contains((TriangleCollectible)s))
                {
                    collectibles.Remove((TriangleCollectible)s);
                }
            }                      
        }

        public void AddCollectible(Vector2 pos)
        {
            collectibles.Add(new TriangleCollectible(pos));
            collectibles.Last().Load(this.GetPhysicsSimulator());
        }

        public bool AddBlackObstacle(Vector2 dimension, Vector2 position)
        {
            RectanglePlatform platform;
            try
            {
                platform = new RectanglePlatform((int)dimension.X, (int)dimension.Y, position, false, GameColors.BLACK_OBSTACLE_COLOR, GameColors.EDITOR_OBSTACLE_BORDER_COLOR, 0);
                platform.Load(base.GetPhysicsSimulator());
                blackObstacles.Add(platform);
                obstacles.Add(platform);
                return true;
            }
            catch (Exception e)
            {
                Log.LogError("Exception thrown in addBlackObstacle:" + e.Message);
                return false;
            }
        }

        public bool AddGreenObstacle(Vector2 dimension, Vector2 position)
        {
            RectanglePlatform platform;
            try
            {
                platform = new RectanglePlatform((int)dimension.X, (int)dimension.Y, position, false, GameColors.GREEN_OBSTACLE_COLOR, GameColors.EDITOR_OBSTACLE_BORDER_COLOR, 0);
                platform.Load(base.GetPhysicsSimulator());
                yellowObstacles.Add(platform);
                obstacles.Add(platform);
                return true;
            }
            catch (Exception e)
            {
                Log.LogError("Exception thrown in addGreenObstacle:" + e.Message);
                return false;
            }
        }

        public bool AddYellowObstacle(Vector2 dimension, Vector2 position)
        {
            RectanglePlatform platform;
            try
            {
                platform = new RectanglePlatform((int)dimension.X, (int)dimension.Y, position, false, GameColors.YELLOW_OBSTACLE_COLOR, GameColors.EDITOR_OBSTACLE_BORDER_COLOR, 0);
                platform.Load(base.GetPhysicsSimulator());
                greenObstacles.Add(platform);
                obstacles.Add(platform);
                return true;
            }
            catch (Exception e)
            {
                Log.LogError("Exception thrown in addYellowObstacle:" + e.Message);
                return false;
            }
        }

        protected void AddObstacle(Vector2 position, Vector2 dimension, CursorTool tool)
        {
            RectanglePlatform platform;

            switch (tool)
            {
                case CursorTool.BLACK_PLATFORM:
                    platform = new RectanglePlatform((int)dimension.X, (int)dimension.Y, position, false, GameColors.BLACK_OBSTACLE_COLOR, GameColors.EDITOR_OBSTACLE_BORDER_COLOR, 0);
                    blackObstacles.Add(platform);
                    obstacles.Add(platform);
                    break;
                case CursorTool.GREEN_PLATFORM:
                    platform = new RectanglePlatform((int)dimension.X, (int)dimension.Y, position, false, GameColors.GREEN_OBSTACLE_COLOR, GameColors.EDITOR_OBSTACLE_BORDER_COLOR, 0);
                    greenObstacles.Add(platform);
                    obstacles.Add(platform);
                    break;
                case CursorTool.YELLOW_PLATFORM:
                    platform = new RectanglePlatform((int)dimension.X, (int)dimension.Y, position, false, GameColors.YELLOW_OBSTACLE_COLOR, GameColors.EDITOR_OBSTACLE_BORDER_COLOR, 0);
                    yellowObstacles.Add(platform);
                    obstacles.Add(platform);
                    break;
            }
            obstacles.Last().Load(this.GetPhysicsSimulator());
        }

        protected void AddElevator(Vector2 position, Vector2 dimension, Vector2 destination, ElevatorDirections dir, bool repeatMovement, int collectiblesNeeded)
        {
            string direction;
            switch(dir)
            {
                case ElevatorDirections.HOR_LEFT:
                    direction = "Horizontal-Left";
                    break;
                case ElevatorDirections.HOR_RIGHT:
                    direction = "Horizontal-Right";
                    break;
                case ElevatorDirections.VER_DOWN:
                    direction = "Vertical-Down";
                    break;
                case ElevatorDirections.VER_UP:
                    direction = "Vertical-Up";
                    break;
                default:
                    direction = "Vertical-Up";
                    break;
            }

            elevators.Add(new Elevator((int)dimension.X, (int)dimension.Y, position, destination, direction, collectiblesNeeded, repeatMovement, GameColors.EDITOR_ELEVATOR_FILL_COLOR, GameColors.EDITOR_ELEVATOR_BORDER_COLOR, 0));
        }

        public void DeleteRectangle()
        {
            rectangle.SetPosition(new Vector2(-5000,-5000));
            isRectangleStartSet = false;
        }

        public void DeleteCircle()
        {
            circle.SetPosition(new Vector2(-5000, -5000));
            isCircleStartSet = false;
        }

        public void SetCircleStart(Vector2 position)
        {
            //this.loadCircleCharacter(new CircleCharacter(new Vector2(0, 0)));
            circle.SetPosition(position);
            isCircleStartSet = true;
        }

        public void SetRectangleStart(Vector2 position)
        {
            //this.loadRectangleCharacter(new RectangleCharacter(new Vector2(0, 0)));
            rectangle.SetPosition(position);            
            isRectangleStartSet = true;
        }

        public bool ExitLevelEditor()
        {
            ScreenManager.RemoveScreen(this);
            ScreenManager.AddScreen(new MainMenuScreen());
            return true;
        }

        public override void DrawLevel(DrawingInstructionsBatch instructionsBatch)
        {
            border.Draw(instructionsBatch);

            if (isCircleStartSet)
            {
                circle.Draw(instructionsBatch);
            }

            if (isRectangleStartSet)
            {
                rectangle.Draw(instructionsBatch);
            }

            foreach(TriangleCollectible col in collectibles)
            {
                col.Draw(instructionsBatch);
            }

            foreach (RectanglePlatform obst in obstacles)
            {
                obst.Draw(instructionsBatch);
            }

            foreach (Elevator e in elevators)
            {
                e.Draw(instructionsBatch);
            }
        }

        protected override void EndGame() {
            // empty
        }
    }
}
