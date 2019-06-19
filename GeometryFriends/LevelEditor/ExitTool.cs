using GeometryFriends.XNAStub;

namespace GeometryFriends.LevelEditor
{
    class ExitTool : DialogTool
    {
         public ExitTool()
        {
             toolName = "Exit Tool";
             toolDescription = "Do you wish to save before leaving? (esc key cancels exit)";
             option1 = "Yes";
             option2 = "No";
             dialogText = "Do you wish to save?";
        }
     
        public override void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            this.dialogSpriteFont = contentManager.Load<SpriteFont>("Content/Fonts/detailsFont.spritefont");
            this.iconTexture = contentManager.Load<Texture2D>("Content/LevelEditor/ExitToolIcon.png");
            this.cursorTexture = contentManager.Load<Texture2D>("Content/LevelEditor/BlackPlatformToolCursor.png");

            this.dialogTexture = contentManager.Load<Texture2D>("Content/LevelEditor/DialogBg.png");            
        }
    }
}
