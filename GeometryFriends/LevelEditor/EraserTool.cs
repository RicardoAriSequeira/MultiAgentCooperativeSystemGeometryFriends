using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{
    class EraserTool : PositionTool
    {
        public EraserTool()
        {
            
            toolName = "Eraser Tool";
            toolDescription = "Deletes a Shape from the level - To use click on the Shape you wish to delete";            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/EraserToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");            
        }
    }
}
