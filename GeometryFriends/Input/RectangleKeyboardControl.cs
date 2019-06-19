using GeometryFriends.XNAStub;


namespace GeometryFriends.Input
{
    internal class RectangleKeyboardControl : RectangleWiiInputSchema
    {
        public override bool MorphDown(InputState input)
        {            
            if (input.CurrentKeyboardState.IsKeyDown(Keys.K))
                return true;
            return false;                    
        }

        public override bool MorphUp(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.I))
                return true;
            return false;
        }
        
        public override bool MoveLeft(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.J))
                return true;
            return false;
        }

        public override bool MoveRight(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.L))
                return true;
            return false;
        }
    }
}
