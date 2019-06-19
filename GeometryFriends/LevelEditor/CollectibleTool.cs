using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{
    class CollectibleTool : PositionTool
    {
         public CollectibleTool()
        {
            
            toolName = "Diamond Tool";
            toolDescription = "Places a purple diamond in the level\n To use click where you want the diamond to appear";            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/CollectibleToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");            
        }
    }
}
