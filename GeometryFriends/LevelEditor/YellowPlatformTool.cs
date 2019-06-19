using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{
    class YellowPlatformTool : PlatformTool
    {
        public YellowPlatformTool()
        {
            toolName = "Yellow Platform Tool";
            toolDescription = "Creates Yellow platforms that collide only with the Rectangle shape Player\n To use simply click where you want the platform to begin then drag";            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/YellowPlatformToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");
            this.rectangleTexture = contentManager.Load<Texture2D>("Content/Common/test.png");
        }
    }
}
