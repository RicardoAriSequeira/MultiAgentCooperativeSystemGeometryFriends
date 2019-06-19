using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Levels.Shared;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends
{
    internal class AgentDebugPane : DrawableGameComponent
    {
        ContentManager content;
        SpriteFont spriteFont;
        
        public bool AgentVisible
        {
            get;
            set;
        }
        
        private RectangleCharacter rectangle;

        private Color objectiveColor = Color.White;
        private string currentObjective;
        public String CurrentObjective
        {
            get
            {
                return this.currentObjective;
            }
            set
            {
                this.currentObjective = value;
                objectiveColor = Color.Black;
            }

        }

        private Color taskColor = Color.White;
        private string currentTask;        
        public String CurrentTask
        {
            get
            {
                return this.currentTask;
            }
            set
            {
                this.currentTask = value;
                taskColor = Color.Black;
            }
        }

        private Color actionColor = Color.White;
        private string currentAction;        
        public String CurrentAction
        {
            get
            {
                return this.currentAction;
            }
            set
            {
                this.currentAction = value;
                actionColor = Color.Black;
            }
        }

        private Color durationColor;
        private string currentActionTime;
        public String CurrentActionTime
        {
            get
            {
                return this.currentActionTime;
            }
            set
            {
                this.currentActionTime = value;
                durationColor = Color.Black;
            }
        }

        private string graphInfo = "Press tab to toggle graph info";
        public String GraphInfo
        {
            get
            {
                return this.graphInfo;
            }
            set
            {
                this.graphInfo = value;
            }
        }

        public void setRectangle(RectangleCharacter rectangle)
        {
            this.rectangle = rectangle;
        }

        public AgentDebugPane(IGraphicsDevice drawingDevice)
            : base(drawingDevice)
        {
            content = new ContentManager();
            this.AgentVisible = false;
        }

        private const int TIME_TO_FADE = 500;
        protected override void LoadContent()
        {
            spriteFont = content.Load<SpriteFont>(@"Content\Fonts\diagnosticFont.spritefont");
        }

        protected override void UnloadContent()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {        
            double elapsedMillis = gameTime.ElapsedGameTime.TotalMilliseconds;
            byte colorToAdd = (byte)(elapsedMillis / TIME_TO_FADE * 255);

            objectiveColor.R += (objectiveColor.R + colorToAdd > 255 ? (byte)0 : colorToAdd);
            objectiveColor.G += (objectiveColor.G + colorToAdd > 255 ? (byte)0 : colorToAdd);
            objectiveColor.B += (objectiveColor.B + colorToAdd > 255 ? (byte)0 : colorToAdd);
            taskColor.R += (taskColor.R + colorToAdd > 255 ? (byte)0 : colorToAdd);
            taskColor.G += (taskColor.G + colorToAdd > 255 ? (byte)0 : colorToAdd);
            taskColor.B += (taskColor.B + colorToAdd > 255 ? (byte)0 : colorToAdd);
            actionColor.R += (actionColor.R + colorToAdd > 255 ? (byte)0 : colorToAdd);
            actionColor.G += (actionColor.G + colorToAdd > 255 ? (byte)0 : colorToAdd);
            actionColor.B += (actionColor.B + colorToAdd > 255 ? (byte)0 : colorToAdd);
            durationColor.B += (durationColor.B + colorToAdd > 255 ? (byte)0 : colorToAdd);
            durationColor.R += (durationColor.R + colorToAdd > 255 ? (byte)0 : colorToAdd);
            durationColor.G += (durationColor.G + colorToAdd > 255 ? (byte)0 : colorToAdd);            
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            if (AgentVisible)
            {
                string obj = "obj: " + CurrentObjective;
                string tsk = "task: " + CurrentTask;
                string action = "action: " + CurrentAction;
                string duration = "dur: " + CurrentActionTime;
                string sXVel = "X Vel: ";
                string sYVel = "Y Vel: ";
                if (rectangle != null)
                {
                    sXVel += rectangle.Body.LinearVelocity.X;
                    sYVel += rectangle.Body.LinearVelocity.Y;
                }

                instructionsBatch.DrawRectangle(new Vector2(90, 45), 300, 500, new Color(255, 0, 0, 100));

                instructionsBatch.DrawString(spriteFont, obj, new Vector2(100, 50), objectiveColor);
                instructionsBatch.DrawString(spriteFont, tsk, new Vector2(100, 80), taskColor);
                instructionsBatch.DrawString(spriteFont, action, new Vector2(100, 110), actionColor);
                instructionsBatch.DrawString(spriteFont, duration, new Vector2(100, 140), durationColor);

                instructionsBatch.DrawString(spriteFont, sXVel, new Vector2(100, 200), Color.White);
                instructionsBatch.DrawString(spriteFont, sYVel, new Vector2(100, 230), Color.White);

                instructionsBatch.DrawString(spriteFont, graphInfo, new Vector2(100, 280), Color.White);
            }
        }
    }
}
