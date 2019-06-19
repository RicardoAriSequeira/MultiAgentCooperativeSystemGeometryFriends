#region Using Statements


using FarseerGames.FarseerPhysics.Mathematics;
using System;
using WiiInput;
#endregion

namespace GeometryFriends.Input
{   
    ///<Summary>
    ///Wiimote keys listed
    ///</summary>
    internal enum WiiKeys
    {
        BUTTON_UP,
        BUTTON_DOWN,
        BUTTON_LEFT,
        BUTTON_RIGHT,
        BUTTON_HOME,
        BUTTON_PLUS,
        BUTTON_MINUS,
        BUTTON_A,
        BUTTON_B,
        BUTTON_Z,
        BUTTON_1,
        BUTTON_2
    }
    ///<Summary>
    ///used to determine which wiimote to get input from.
    ///WiimoteLib only supports 2 wiimotes as of now.
    ///</summary>
    internal enum WiimoteNumber
    {
        WII_1,
        WII_2
    }

    internal class WiimoteHandler
    {
        private bool[] wiimoteOn = {false, false, false, false};
        private WiimoteInput wiimote;

        public bool isWiimoteOn(WiimoteNumber num)
        {
            return this.wiimoteOn[(int)num];
        }

        public WiimoteHandler()
        {
            ConnectWiimote();
        }

        //method responsible for exception handling relative to the wiimote connection
        public void ConnectWiimote()
        {
            try
            {
                wiimote = new WiimoteInput();
                this.wiimoteOn[0] = true;
                for (int i = 1; i < 4; ++i )
                {
                    try
                    {
                        this.wiimote.getWiimotePlayer(i);
                        this.wiimoteOn[i] = true;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        this.wiimoteOn[i] = false;
                    }
                }              
            }
            catch (Exception)
            {
                this.wiimoteOn[0] = false;
                Log.LogRaw("WiimoteInput not found.");
            }
        }

        public void UpdateState()
        {
            if (this.wiimoteOn[0])
            {
                try
                {
                    wiimote.updateState();
                }
                catch (NullReferenceException)
                {
                    ConnectWiimote();
                }

                // write log
                /*
                // get values for WIImote1. It does not include the Nunchuk.
                Vector3 v = getAccelerations(WiimoteNumber.WII_1);
                String str = v.X + ";" + v.Y + ";" + v.Z;
            
                v = getDeltas(WiimoteNumber.WII_1);
                str += ";" + v.X + ";" + v.Y + ";" + v.Z;

                if(isKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_B))
                    str += ";1";
                else
                    str += ";0";
                if (this.wiimoteOn[1])
                {
                    // get values for WIImote2. It includes the Nunchuk.
                    v = getAccelerations(WiimoteNumber.WII_2);
                    str += ";" + v.X + ";" + v.Y + ";" + v.Z;

                    v = getDeltas(WiimoteNumber.WII_2);
                    str += ";" + v.X + ";" + v.Y + ";" + v.Z;

                    v = getNunchukAccelerations(WiimoteNumber.WII_2);
                    str += ";" + v.X + ";" + v.Y + ";" + v.Z;
                }
                LogFile.Instance().WriteLine(str);
                */
            }
        }
       
        public bool IsKeyPressed(WiimoteNumber num, WiiKeys key)
        {
            if (wiimoteOn[0])
            {
                if (num == WiimoteNumber.WII_1) 
                    return CheckKeyPressed(WiimoteInput.player1, key, false);
            }
            if (wiimoteOn[1])
            {
                if (num == WiimoteNumber.WII_2) 
                    return CheckKeyPressed(WiimoteInput.player2, key, false);
            }
            return false;
        }

        public bool IsKeyHeld(WiimoteNumber num, WiiKeys key)
        {
            bool res = false;
            if (wiimoteOn[0])
            {

                if (num == WiimoteNumber.WII_1)
                    res = CheckKeyPressed(WiimoteInput.player1, key, true);
            }
            if (wiimoteOn[1])
            {
                if (num == WiimoteNumber.WII_2)
                    res = CheckKeyPressed(WiimoteInput.player2, key, true);
            }

            return res;
        }

        private bool CheckKeyPressed(int player, WiiKeys key, bool isHolding)
        {
            bool result = false;
            switch (key)
            {
                case WiiKeys.BUTTON_1:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.One == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.One == isHolding);
                    break;
                case WiiKeys.BUTTON_2:
                        result = (wiimote.getCurrentWiimoteState(player).ButtonState.Two == true) &&
                            (wiimote.getPreviousWiimoteState(player).ButtonState.Two == isHolding);
                    break;
                case WiiKeys.BUTTON_A:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.A == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.A == isHolding);
                    break;
                case WiiKeys.BUTTON_B:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.B == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.B == isHolding);
                    break;
                case WiiKeys.BUTTON_DOWN:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.Down == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.Down == isHolding);
                    break;
                case WiiKeys.BUTTON_HOME:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.Home == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.Home == isHolding);
                    break;
                case WiiKeys.BUTTON_LEFT:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.Left == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.Left == isHolding);
                    break;
                case WiiKeys.BUTTON_MINUS:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.Minus == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.Minus == isHolding);
                    break;
                case WiiKeys.BUTTON_PLUS:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.Plus == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.Plus == isHolding);
                    break;
                case WiiKeys.BUTTON_RIGHT:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.Right == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.Right == isHolding);
                    break;
                case WiiKeys.BUTTON_UP:
                    result = (wiimote.getCurrentWiimoteState(player).ButtonState.Up == true) &&
                        (wiimote.getPreviousWiimoteState(player).ButtonState.Up == isHolding);
                    break;                
            }

            return result;
        }

        public bool IsNunchukOn(WiimoteNumber num)
        {
            bool res = false;
            if (wiimoteOn[(int)num])
            {
                if (num == WiimoteNumber.WII_1)
                    res = (wiimote.getCurrentWiimoteState(WiimoteInput.player1).Extension &&
                        wiimote.getCurrentWiimoteState(WiimoteInput.player1).ExtensionType == WiimoteLib.ExtensionType.Nunchuk);
                if (num == WiimoteNumber.WII_2)
                    res = (wiimote.getCurrentWiimoteState(WiimoteInput.player2).Extension &&
                        wiimote.getCurrentWiimoteState(WiimoteInput.player2).ExtensionType == WiimoteLib.ExtensionType.Nunchuk);
            }

            return res;
        }

        public Vector2 GetNunchukJoystickPosition(WiimoteNumber num)
        {
            Vector2 result = Vector2.Zero;
            if (num == WiimoteNumber.WII_1)
                result = new Vector2(wiimote.getCurrentWiimoteState(WiimoteInput.player1).NunchukState.Joystick.X,
                    wiimote.getCurrentWiimoteState(WiimoteInput.player1).NunchukState.Joystick.Y);
            if (num == WiimoteNumber.WII_2)
                result = new Vector2(wiimote.getCurrentWiimoteState(WiimoteInput.player2).NunchukState.Joystick.X,
                    wiimote.getCurrentWiimoteState(WiimoteInput.player2).NunchukState.Joystick.Y);
            return result;
        }

        public Vector3 GetAccelerations(WiimoteNumber num)
        {
            Vector3 accels = new Vector3(0,0,0);
            if (wiimoteOn[(int)num])
            {
                accels.X = wiimote.getCurrentWiimoteState((int)num).AccelState.Values.X;
                accels.Y = wiimote.getCurrentWiimoteState((int)num).AccelState.Values.Y;
                accels.Z = wiimote.getCurrentWiimoteState((int)num).AccelState.Values.Z;
            }
            return accels;
        }

        public Vector3 GetNunchukAccelerations(WiimoteNumber num)
        {
            Vector3 accels = new Vector3(0, 0, 0);
            if (wiimoteOn[(int)num] && IsNunchukOn(num))
            {
                accels.X = wiimote.getCurrentWiimoteState((int)num).NunchukState.AccelState.Values.X;
                accels.Y = wiimote.getCurrentWiimoteState((int)num).NunchukState.AccelState.Values.Y;
                accels.Z = wiimote.getCurrentWiimoteState((int)num).NunchukState.AccelState.Values.Z;
            }
            return accels;
        }

        public Vector3 GetDeltas(WiimoteNumber num)
        {
            Vector3 deltas = new Vector3(0, 0, 0);
            if (wiimoteOn[(int)num] && isWiimoteOn(num))
            {
                deltas.X = wiimote.getWiimotePlayer((int)num).deltaX();
                deltas.Y = wiimote.getWiimotePlayer((int)num).deltaY();
                deltas.Z = wiimote.getWiimotePlayer((int)num).deltaZ();
            }
            return deltas;
        }
    }
}
