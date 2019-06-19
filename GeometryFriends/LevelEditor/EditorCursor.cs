using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Timers;
//using GeometryFriends.DrawingSystem;

namespace GeometryFriends.LevelEditor
{
    internal enum CursorState
    {
        IDLE,
        DRAGGING,
        SELECTED
    }

    internal enum CursorTool
    {
        NONE,
        BLACK_PLATFORM,
        GREEN_PLATFORM,
        YELLOW_PLATFORM,
        CIRCLE_START,
        RECTANGLE_START,
        COLLECTIBLE,
        ELEVATOR,
        MOVE,
        RESIZE,
        DELETE,
        SAVE,
        EXIT
    }

    //delegate responsável por conter a função de draw da ferramenta seleccionada a determinado momento
    delegate void DrawTool(DrawingInstructionsBatch instructionsBatch, GameTime gameTime);

    class EditorCursor
    {
        //cursor circle- 0
        private const int CURSOR_RADIUS = 5;
        private Color cursorColor = new Color(220, 20, 60, 155);
        //black rectangle - 1
        private const int BR_WIDTH = 500;
        private const int BR_HEIGHT = 300;
        private Color brColor = new Color(0, 0, 0, 155);        
        //yellow rectangle - 2
        private const int YR_WIDTH = 500;
        private const int YR_HEIGHT = 300;
        private Color yrColor = new Color(255, 255, 0, 155);
        //green rectangle - 3
        private const int gR_WIDTH = 500;
        private const int gR_HEIGHT = 300;
        private Color grColor = new Color(173, 255, 47, 155);
        //ball start position circle - 4
        private const int SP_RADIUS = 32;
        private Color spColor = new Color(255, 255, 0, 155);        
        //square start position rectangle - 5
        private const int SR_WIDTH = 32;
        private const int SR_HEIGHT = 64;
        private Color srColor = new Color(0, 255, 0, 155);        
        //collectible rectangle - 6
        private const int CR_WIDTH = 64;
        private const int CR_HEIGHT = 64;
        private Color crColor = new Color(75, 0, 130, 155);        
        //elevator rectangle - 7
        private const int ER_WIDTH = 500;
        private const int ER_HEIGHT = 300;
        private Color erColor = new Color(173, 255, 47, 155);
        //save window rectangle - 8
        private int WR_WIDTH;
        private int WR_HEIGHT;
        private Color wrColor = Color.AntiqueWhite;        

        private Timer joystickTimer;
        private const double JOYSTICK_INPUT_INTERVAL = 50;
        private bool canMove = true;

        private EditorLevel level;
        private DrawTool drawTool;
        private CursorState previousState, state;
        private CursorTool tool;
        private bool firstInput = true;
        private Vector2 position, drawOrigin;
        public Vector2 Position
        {
            get
            {
                return this.position;
            }
        }

        private System.Drawing.Rectangle lastRect;

        public EditorCursor(EditorLevel level)
        {
            this.state = this.previousState = CursorState.IDLE;
            this.tool = CursorTool.BLACK_PLATFORM;    
            drawTool = new DrawTool(this.DrawCursor);
            this.level = level;
        }

        public void Load(IGraphicsDevice graphicsDevice)
        {                    
            WR_WIDTH = GameAreaInformation.DRAWING_WIDTH / 3;
            WR_HEIGHT = GameAreaInformation.DRAWING_HEIGHT / 3;

            joystickTimer = new Timer(JOYSTICK_INPUT_INTERVAL);
            joystickTimer.Elapsed += new ElapsedEventHandler(CanMoveAgain);
        }

        public void Unload()
        {
        }

        private void MoveTo(Vector2 point)
        {
            this.position = point;
        }

        private void DrawFrom(Vector2 point)
        {
            this.drawOrigin = point;
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch, GameTime gameTime)
        {
            this.drawTool(instructionsBatch, gameTime);
        }

        private void DrawCursor(DrawingInstructionsBatch instructionsBatch, GameTime gameTime)
        {
            instructionsBatch.DrawCircle(new Vector2(this.position.X - CURSOR_RADIUS, this.position.Y - CURSOR_RADIUS), CURSOR_RADIUS, cursorColor);
        }

        private void DrawRectangle(DrawingInstructionsBatch instructionsBatch, GameTime gameTime)
        {
            float rectWidth, rectHeight = 0;
            if (this.state == CursorState.DRAGGING)
            {
                rectWidth = position.X - drawOrigin.X;
                rectHeight = position.Y - drawOrigin.Y;
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 0, 0);               
                if (rectWidth > 0 && rectHeight > 0)
                {
                    rect = new System.Drawing.Rectangle((int)(drawOrigin.X), (int)(drawOrigin.Y), (int)(position.X - drawOrigin.X), (int)(position.Y - drawOrigin.Y));
                }
                if (rectWidth < 0 && rectHeight < 0)
                {
                    rect = new System.Drawing.Rectangle((int)(position.X), (int)(position.Y), (int)(Math.Abs(position.X - drawOrigin.X)), (int)(Math.Abs(position.Y - drawOrigin.Y)));
                }
                if (rectWidth > 0 && rectHeight < 0)
                {
                    rect = new System.Drawing.Rectangle((int)(drawOrigin.X), (int)(position.Y), (int)(position.X - drawOrigin.X), (int)(Math.Abs(position.Y - drawOrigin.Y)));
                }
                if (rectWidth < 0 && rectHeight > 0)
                {
                    rect = new System.Drawing.Rectangle((int)(position.X), (int)(drawOrigin.Y), (int)(Math.Abs(position.X - drawOrigin.X)), (int)(position.Y - drawOrigin.Y));
                }

                switch (this.tool)
                {
                    case CursorTool.BLACK_PLATFORM:
                        instructionsBatch.DrawRectangle(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, brColor);
                        break;
                    case CursorTool.YELLOW_PLATFORM:
                        instructionsBatch.DrawRectangle(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, yrColor);
                        break;
                    case CursorTool.GREEN_PLATFORM:
                        instructionsBatch.DrawRectangle(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, grColor);
                        break;
                    case CursorTool.ELEVATOR:
                        instructionsBatch.DrawRectangle(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, erColor);
                        break;
                    default:
                        instructionsBatch.DrawRectangle(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, brColor);
                        break;
                }
                lastRect = rect;
            }
        }

        private void DrawSaveWindow(DrawingInstructionsBatch instructionsBatch, GameTime gameTime)
        {
            //SpriteFont saveWindowFont = ScreenManager.ContentManager.Load<SpriteFont>("Content/Fonts/gameFont");
            Vector2 windowPosition = new Vector2(instructionsBatch.DWidth/2 - WR_WIDTH, instructionsBatch.DHeight/2 - WR_HEIGHT);

            instructionsBatch.DrawRectangle(new Vector2(windowPosition.X, windowPosition.Y), WR_WIDTH, WR_HEIGHT, wrColor);
            instructionsBatch.DrawRectangle(new Vector2(windowPosition.X + 100, windowPosition.Y + WR_HEIGHT *2 / 3), 60, 20, yrColor, Color.Green);
            instructionsBatch.DrawRectangle(new Vector2(windowPosition.X + WR_WIDTH - 200, windowPosition.Y + WR_HEIGHT * 2 / 3), 60, 20, yrColor, Color.Red);
        }

        public void CanMoveAgain(object Sender, EventArgs args)
        {
            canMove = true;
        }

        public void HandleInput(InputState input)
        {
            if (firstInput)
            {
                this.position = new Vector2(10 * EditorLevel.GRID_SNAP_SIZE + level.GridOffset.X, 10 * EditorLevel.GRID_SNAP_SIZE + level.GridOffset.Y);
                firstInput = false;
            }

            Vector2 here;
            if (input.wiiInput.isWiimoteOn(WiimoteNumber.WII_1) && input.wiiInput.IsNunchukOn(WiimoteNumber.WII_1))
            {
                if (canMove)
                {
                    canMove = false;
                    Vector2 joystick = input.wiiInput.GetNunchukJoystickPosition(WiimoteNumber.WII_1);
                    here = this.position;                    

                    if (joystick.X > 0.3 && here.X < level.GridDimensions.X + level.GridOffset.X)
                        here.X += EditorLevel.GRID_SNAP_SIZE;
                    if (joystick.X < -0.3 && here.X > level.GridOffset.X)
                        here.X -= EditorLevel.GRID_SNAP_SIZE;
                    if (joystick.Y > 0.3 && here.Y > level.GridOffset.Y)
                        here.Y -= EditorLevel.GRID_SNAP_SIZE;
                    if (joystick.Y < -0.3 && here.Y < level.GridDimensions.Y + level.GridOffset.Y)
                        here.Y += EditorLevel.GRID_SNAP_SIZE;
                    this.MoveTo(here);
                
                    this.previousState = this.state;

                    if (input.wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_B) && !input.wiiInput.IsKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_B))
                    {
                        this.DrawFrom(here);
                        this.state = CursorState.DRAGGING;
                    }
                    if (input.wiiInput.IsKeyHeld(WiimoteNumber.WII_1,WiiKeys.BUTTON_B))
                    {
                        this.state = CursorState.IDLE;
                    }
                    joystickTimer.Start();
                }
            }
            else
            {
                here = new Vector2((int)(input.CurrentMouseState.X - (input.CurrentMouseState.X % EditorLevel.GRID_SNAP_SIZE) - level.GridOffset.X % EditorLevel.GRID_SNAP_SIZE),
                    (int)(input.CurrentMouseState.Y - (input.CurrentMouseState.Y % EditorLevel.GRID_SNAP_SIZE) - level.GridOffset.Y % EditorLevel.GRID_SNAP_SIZE));
                this.MoveTo(here);
                this.previousState = this.state;

                if (input.CurrentMouseState.LeftButton == ButtonState.Pressed && input.LastMouseState.LeftButton == ButtonState.Released)
                {
                    this.DrawFrom(here);
                    this.state = CursorState.DRAGGING;
                }
                if (input.CurrentMouseState.LeftButton == ButtonState.Released && input.LastMouseState.LeftButton == ButtonState.Pressed)
                {
                    this.state = CursorState.IDLE;
                }
            }
        }

        public void SetTool(CursorTool newTool)
        {
            if (newTool != this.tool)
            {
                //removo a anterior                
                switch (this.tool)
                {
                    case CursorTool.BLACK_PLATFORM:
                        this.drawTool -= new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.GREEN_PLATFORM:
                        this.drawTool -= new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.YELLOW_PLATFORM:
                        this.drawTool -= new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.ELEVATOR:
                        this.drawTool -= new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.SAVE:
                        this.drawTool -= new DrawTool(DrawSaveWindow);
                        break;
                    /*case CursorTool.BALL_START:
                    case CursorTool.COLLECTIBLE:
                    case CursorTool.SQUARE_START:
                    case CursorTool.DELETE:
                    case CursorTool.MOVE:
                    case CursorTool.RESIZE:*/
                    default:
                        break;
                }

                //e adiciono a nova
                switch (newTool)
                {
                    case CursorTool.BLACK_PLATFORM:
                        this.drawTool += new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.GREEN_PLATFORM:
                        this.drawTool += new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.YELLOW_PLATFORM:
                        this.drawTool += new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.ELEVATOR:
                        this.drawTool += new DrawTool(DrawRectangle);
                        break;
                    case CursorTool.SAVE:
                        this.drawTool += new DrawTool(DrawSaveWindow);
                        break;
                    case CursorTool.CIRCLE_START:
                    case CursorTool.COLLECTIBLE:
                    case CursorTool.RECTANGLE_START:
                    case CursorTool.DELETE:
                    case CursorTool.MOVE:
                    case CursorTool.RESIZE:
                    default:
                        break;
                }

                this.tool = newTool;
            }
        }

        public CursorState CurrentState()
        {
            return this.state;
        }

        public CursorState PreviousState()
        {
            return this.previousState;
        }

        public System.Drawing.Rectangle GetRectangleInfo()
        {
            return lastRect;
        }
    }
}
