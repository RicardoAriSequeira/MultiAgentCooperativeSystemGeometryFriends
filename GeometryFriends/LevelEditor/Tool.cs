using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.LevelEditor
{
    internal enum ToolState
    {
        IDLE,  //waiting for the user to start using the tool     
        ACTIVE, //while on use (i.e dragging, moving, etc)
        DRAGGING,
        STANDBY //waiting for more input
    }
    abstract class Tool
    {
        protected String toolName, toolDescription;
        protected Texture2D iconTexture, cursorTexture;
        protected Vector2 position, iconPosition, dragOrigin;
        protected ContentManager contentManager;
        protected ToolState previousState, state;

        public abstract void LoadContent(ContentManager contentManager);
        public abstract void Unload();
        public abstract void Draw(DrawingInstructionsBatch instructionsBatch, GameTime gametime);
        public abstract void HandleInput(InputState input, Vector2 cursorPosition);
        
        public Texture2D GetIconTexture()
        {
            return this.iconTexture;
        }

        public Texture2D GetCursorTexture()
        {
            return this.cursorTexture;
        }

        public String GetName()
        {
            return this.toolName;
        }

        public String GetDescription()
        {
            return this.toolDescription;
        }

        public void SetIconPosition(Vector2 pos)
        {
            this.iconPosition = pos;
        }

        public Vector2 GetIconPosition()
        {
            return this.iconPosition;
        }

        public void Activate()
        {
            this.state = ToolState.STANDBY; //significa que está activa, mas adormecida. maneira de só começar a contar o input após um standby (ignorando o que tenha sido feito anteriormente)
        }

        public ToolState GetState()
        {
            return this.state;
        }
    }
}
