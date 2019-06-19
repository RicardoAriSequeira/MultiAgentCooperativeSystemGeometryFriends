using GeometryFriends.XNAStub;

namespace GeometryFriends.Input
{
    internal class CircleKeyboardControl : CircleWiiInputSchema
    {
        public override bool Grow(InputState input)
        {                                   
            if (input.CurrentKeyboardState.IsKeyDown(Keys.S))
            {
                return true;
            }
            return false;
        }

        public override float Jump(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.W))
            {
                return 2;
            }
            return 0;
        }

        public override bool MoveLeft(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.A))
                return true;
            return false;
        }

        public override bool MoveRight(InputState input)
        {
            if (input.CurrentKeyboardState.IsKeyDown(Keys.D))
                return true;
            return false;
        }
    }
}
