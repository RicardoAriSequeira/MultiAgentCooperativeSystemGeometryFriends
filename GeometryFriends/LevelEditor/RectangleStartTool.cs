using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{
    class RectangleStartTool : PositionTool
    {
        public RectangleStartTool()
        {
            toolName = "Rectangle Position Tool";
            toolDescription = "Sets the place where the Rectangle player starts \n To use click where you want the Rectangle player to start";            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/SquareStartToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");            
        }
    }
}
