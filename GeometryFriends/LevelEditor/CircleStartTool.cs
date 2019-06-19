using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{
    class CircleStartTool : PositionTool
    {
        public CircleStartTool()
        {
            
            toolName = "Circle Position Tool";
            toolDescription = "Sets the place where the Circle player starts \n To use click where you want the circle player to start";            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/CircleStartToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");            
        }
    }
}
