using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{
    class GreenPlatformTool : PlatformTool
    {
         public GreenPlatformTool()
        {
            
            toolName = "Green Platform Tool";
            toolDescription = "Creates green platforms that collide only with the Circle shape Player\n To use simply click where you want the platform to begin then drag";            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/GreenPlatformToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");
            this.rectangleTexture = contentManager.Load<Texture2D>("Content/Common/test.png");
        }
    }
}
