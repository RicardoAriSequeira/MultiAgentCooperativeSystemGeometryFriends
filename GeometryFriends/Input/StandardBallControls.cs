using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GeometryFriends.Input
{
    class StandardBallControls : BallWiiInputSchema
    {
        public override bool grow(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                if (input.wiiInput.isKeyHeld(this.ControllerNumber, WiiKeys.BUTTON_B))
                {
                    return true;
                }                                    
            }
            return false;
        }

        public override float jump(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                if (input.wiiInput.getDeltas(this.ControllerNumber).Y > 1)
                    return input.wiiInput.getDeltas(this.ControllerNumber).Y;
            }
            return 0;
        }

        public override bool moveLeft(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                Vector3 accels = input.wiiInput.getAccelerations(this.ControllerNumber);

                if (accels.X > 0.5)
                    return true;
            }
            return false;
            
        }

        public override bool moveRight(InputState input)
        {
            if (input.wiiInput.isWiimoteOn(this.ControllerNumber))
            {
                Vector3 accels = input.wiiInput.getAccelerations(this.ControllerNumber);

                if (accels.X < -0.5)
                    return true;
            }
            return false;
        }
    }
}
