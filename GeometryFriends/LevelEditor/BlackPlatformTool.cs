using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{   
    class BlackPlatformTool : PlatformTool
    {
        //public delegate bool BlackPlatformToolHandler(Vector2 dim, Vector2 pos);
        //public event BlackPlatformToolHandler ToolFinished;
        
        public BlackPlatformTool()
        {
            
            toolName = "Black Platform Tool";
            toolDescription = "Creates black platforms that collide with both players.    To use simply click where you want the platform to begin then drag";            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");
            this.rectangleTexture = contentManager.Load<Texture2D>("Content/Common/test.png");
        }
    }
}
