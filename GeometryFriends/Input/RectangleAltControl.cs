using FarseerGames.FarseerPhysics.Mathematics;


namespace GeometryFriends.Input
{
    class RectangleAltControl : RectangleWiiInputSchema
    {
        public override bool MorphDown(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                Vector3 accels = input.wiiInput.GetAccelerations(this.ControllerNumber);
                if (accels.Y > 0.5)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool MorphUp(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                Vector3 accels = input.wiiInput.GetAccelerations(this.ControllerNumber);
                if (accels.Y < -0.5)
                {
                    return true;
                }
            }
            return false;
        }
        
        public override bool MoveLeft(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                if (input.wiiInput.IsNunchukOn(this.ControllerNumber))
                {
                    Vector3 accels = input.wiiInput.GetNunchukAccelerations(this.ControllerNumber);
                    if (accels.X < 0.5)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool MoveRight(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                if (input.wiiInput.IsNunchukOn(this.ControllerNumber))
                {
                    Vector3 accels = input.wiiInput.GetNunchukAccelerations(this.ControllerNumber);
                    if (accels.X > -0.5)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
