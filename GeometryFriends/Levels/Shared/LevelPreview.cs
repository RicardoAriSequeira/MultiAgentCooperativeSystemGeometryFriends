using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeometryFriends.Levels.Shared;
using Microsoft.Xna.Framework;
using GeometryFriends.DrawingSystem;
using Microsoft.Xna.Framework.Graphics;
using GeometryFriends.ScreenSystem;
using GeometryFriends.Levels;
using System.Xml;
using Microsoft.Xna.Framework.Content;

namespace GeometryFriends.Levels.Shared
{
    class LevelPreview : XMLLevel
    {
        private const float PREVIEW_SCALE = 0.3f;
        private Matrix scale;        

        /// <summary>
        /// Creates an EditorLevel in order to edit the level passed as parameter. 
        /// Use the constructor with no params if you wish to create a new level.
        /// </summary>
        /// <param name="lvlNum"> number of the level to edit. must be a positive number.</param>
        public LevelPreview(int lvlNum) : base(lvlNum)
        {            
        }

       public void LoadContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;                       
            scale = Matrix.CreateScale(PREVIEW_SCALE);
            this.LoadLevelContent();
        }

       public override void LoadLevelContent()
       {
               base.LoadLevelContent();           
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

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, scale);            
            this.DrawLevel();           
            ScreenManager.SpriteBatch.End();
        }

        protected override void HandleBallControls(InputState input)
        {
            //Have to override abstract methods, but they are not supposed to be controlled in an editor level hence the empty method
        }

        protected override void HandleSquareControls(InputState input)
        {
            //Have to override abstract methods, but they are not supposed to be controlled in an editor level hence the empty method
        }

        public override void HandleInput(InputState input)
        {
        }

        public override void DrawLevel()
        {
            base.DrawLevel();       
        }
       
    }
}
