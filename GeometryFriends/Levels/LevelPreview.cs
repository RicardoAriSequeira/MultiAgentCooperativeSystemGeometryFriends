
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;

namespace GeometryFriends.Levels.Shared
{
    class LevelPreview : XMLLevel
    {
        private const float PREVIEW_BACKGROUND_PRIORITY = -3f;
        private const float PREVIEW_SCALE = 0.2f;
        private Vector2 position;
        private float transformScale;
        public bool IsSelected
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a Level Preview used in level selection screens.         
        /// </summary>
        /// <param name="lvlNum"> number of level to preview. must be a positive number.</param>
        public LevelPreview(int lvlNum, IGraphicsDevice graphicsDevice, Vector2 position) : base(lvlNum)
        {
            this.position = position;
            this.graphicsDevice = graphicsDevice;
            IsSelected = true;
        }

       public void LoadContent(ContentManager contentManager)
       {
           this.contentManager = contentManager;
           transformScale = PREVIEW_SCALE;
           this.LoadLevelContent();
       }

       protected override void LoadBorders()
       {       
       }

       public override void LoadLevelContent()
       {
           base.LoadLevelContent();           
       }

       public Vector2 GetPosition()
       {
           return this.position;
       }

        /// <summary>
        /// Unload content from memory.
        /// </summary>
        public override void UnloadContent()
        {            
            base.UnloadContent();
        }

        //em principio fazer override ao update resolve o meu problema de ter o physics simulator a funcionar no modo de edição
        //(assim como toda a logica de jogo)
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {            
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            int thickness = (int)(GameAreaInformation.DRAWING_HEIGHT * 0.05f);
            instructionsBatch.SetTransformMatrix(position.X, position.Y, 0, transformScale);
            instructionsBatch.DrawRectangle(new Vector2(0, 0), GameAreaInformation.DRAWING_WIDTH, GameAreaInformation.DRAWING_HEIGHT, Color.SteelBlue, PREVIEW_BACKGROUND_PRIORITY);
            this.DrawLevel(instructionsBatch);
            //draw borders
            if (this.IsSelected)
            {
                instructionsBatch.DrawRectangle(new Vector2(0, 0), GameAreaInformation.DRAWING_WIDTH, thickness, Color.LightCoral);
                instructionsBatch.DrawRectangle(new Vector2(0, GameAreaInformation.DRAWING_HEIGHT - thickness), GameAreaInformation.DRAWING_WIDTH, thickness, Color.LightCoral);
                instructionsBatch.DrawRectangle(new Vector2(0, 0), thickness, GameAreaInformation.DRAWING_HEIGHT, Color.LightCoral);
                instructionsBatch.DrawRectangle(new Vector2(GameAreaInformation.DRAWING_WIDTH - thickness, 0), thickness, GameAreaInformation.DRAWING_HEIGHT, Color.LightCoral);                
            }
            else
            {
                instructionsBatch.DrawRectangle(new Vector2(0, 0), GameAreaInformation.DRAWING_WIDTH, thickness, Color.White);
                instructionsBatch.DrawRectangle(new Vector2(0, GameAreaInformation.DRAWING_HEIGHT - thickness), GameAreaInformation.DRAWING_WIDTH, thickness, Color.White);
                instructionsBatch.DrawRectangle(new Vector2(0, 0), thickness, GameAreaInformation.DRAWING_HEIGHT, Color.White);
                instructionsBatch.DrawRectangle(new Vector2(GameAreaInformation.DRAWING_WIDTH - thickness, 0), thickness, GameAreaInformation.DRAWING_HEIGHT, Color.White);                
            }
            instructionsBatch.ResetTransformMatrix();
        }

        public override void DrawLevel(DrawingInstructionsBatch instructionsBatch)
        {
            foreach (TriangleCollectible col in collectibles)
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

            if (hasHangingTexture)
            {
                instructionsBatch.DrawRectangle(new Vector2(hangingGeom.Position.X - hangingTextureWidth / 2, hangingGeom.Position.Y - hangingTextureHeight / 2), hangingTextureWidth, hangingTextureHeight, hangingTextureColor, hangingTextureBorderThickness, hangingTextureBorderColor, hangingGeom.Rotation);
            }
            if(circle.X >= 0 && circle.Y >= 0)
                circle.Draw(instructionsBatch);
            if(rectangle.X>=0 && rectangle.Y>=0)
                rectangle.Draw(instructionsBatch);
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
        }

        protected override void EndGame() {
            // empty
        }
    }
}
