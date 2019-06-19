using FarseerGames.FarseerPhysics.Mathematics;


namespace GeometryFriends.Input
{
    class CircleStandardControl : CircleWiiInputSchema
    {
        public override bool Grow(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                if (input.wiiInput.IsKeyHeld(this.ControllerNumber, WiiKeys.BUTTON_B))
                {
                    return true;
                }                                    
            }
            return false;
        }

        public override float Jump(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                if (input.wiiInput.GetDeltas(this.ControllerNumber).Y > 1)
                    return input.wiiInput.GetDeltas(this.ControllerNumber).Y;
            }
            return 0;
        }

        public override bool MoveLeft(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                Vector3 accels = input.wiiInput.GetAccelerations(this.ControllerNumber);

                if (accels.X < 0.5)
                    return true;
            }
            return false;
        }

        public override bool MoveRight(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                Vector3 accels = input.wiiInput.GetAccelerations(this.ControllerNumber);

                if (accels.X > -0.5)
                    return true;
            }
            return false;
        }
    }
}
