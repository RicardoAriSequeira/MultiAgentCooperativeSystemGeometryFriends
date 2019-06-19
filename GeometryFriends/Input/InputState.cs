#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using GeometryFriends.XNAStub;
#endregion

namespace GeometryFriends.Input
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    internal class InputState
    {
        #region Fields

        public KeyboardState CurrentKeyboardState;
        //public GamePadState CurrentGamePadState;

        public MouseState CurrentMouseState;
        public WiimoteHandler wiiInput;

        public KeyboardState LastKeyboardState;
        //public GamePadState LastGamePadState;

        public MouseState LastMouseState;

        #endregion

        #region Properties

        public InputState()
        {            
            //CurrentKeyboardState = new KeyboardState();
            //CurrentGamePadState = new GamePadState();
            //CurrentMouseState = new MouseState();
            //temporário
            
            wiiInput = new WiimoteHandler();            

            //wiiInput.initialize();


            //                LastKeyboardState = new KeyboardState();
            //                LastGamePadState = new GamePadState();
            //#if !XBOX
            //                LastMouseState = new MouseState();
            //#endif
        }


        /// <summary>
        /// Checks for a "menu up" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuUp
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                       /*(CurrentGamePadState.DPad.Up == ButtonState.Pressed &&
                        LastGamePadState.DPad.Up == ButtonState.Released) ||
                       (CurrentGamePadState.ThumbSticks.Left.Y > 0 &&
                        LastGamePadState.ThumbSticks.Left.Y <= 0) ||*/
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_UP) ||
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_2, WiiKeys.BUTTON_UP);
            }
        }


        /// <summary>
        /// Checks for a "menu down" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuDown
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                       /*(CurrentGamePadState.DPad.Down == ButtonState.Pressed &&
                        LastGamePadState.DPad.Down == ButtonState.Released) ||
                       (CurrentGamePadState.ThumbSticks.Left.Y < 0 &&
                        LastGamePadState.ThumbSticks.Left.Y >= 0) ||*/
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_DOWN) ||
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_2, WiiKeys.BUTTON_DOWN);
            }
        }

        /// <summary>
        /// Checks for a "menu down" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuLeft
        {
            get
            {
                return IsNewKeyPress(Keys.Left) ||
                       /*(CurrentGamePadState.DPad.Left == ButtonState.Pressed &&
                        LastGamePadState.DPad.Left == ButtonState.Released) ||
                       (CurrentGamePadState.ThumbSticks.Left.X < 0 &&
                        LastGamePadState.ThumbSticks.Left.X >= 0) ||*/
                       wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_LEFT) ||
                       wiiInput.IsKeyPressed(WiimoteNumber.WII_2, WiiKeys.BUTTON_LEFT);
            }
        }

        /// <summary>
        /// Checks for a "menu down" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuRight
        {
            get
            {
                return IsNewKeyPress(Keys.Right) ||
                       /*(CurrentGamePadState.DPad.Right == ButtonState.Pressed &&
                        LastGamePadState.DPad.Right == ButtonState.Released) ||
                       (CurrentGamePadState.ThumbSticks.Left.X > 0 &&
                        LastGamePadState.ThumbSticks.Left.X <= 0) ||*/
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_RIGHT) ||
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_2, WiiKeys.BUTTON_RIGHT);
            }
        }
        /// <summary>
        /// Checks for a "menu select" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewKeyPress(Keys.Enter) ||
                       /*(CurrentGamePadState.Buttons.A == ButtonState.Pressed &&
                        LastGamePadState.Buttons.A == ButtonState.Released) ||
                       (CurrentGamePadState.Buttons.Start == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Start == ButtonState.Released) ||*/
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_A) ||
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_2, WiiKeys.BUTTON_A);
            }
        }


        /// <summary>
        /// Checks for a "menu cancel" input action (on either keyboard or gamepad).
        /// </summary>
        public bool MenuCancel
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       /*(CurrentGamePadState.Buttons.B == ButtonState.Pressed &&
                        LastGamePadState.Buttons.B == ButtonState.Released) ||
                       (CurrentGamePadState.Buttons.Back == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Back == ButtonState.Released) ||*/
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_HOME) ||
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_2, WiiKeys.BUTTON_HOME);
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action (on either keyboard or gamepad).
        /// </summary>
        public bool PauseGame
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       /*(CurrentGamePadState.Buttons.Back == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Back == ButtonState.Released) ||
                       (CurrentGamePadState.Buttons.Start == ButtonState.Pressed &&
                        LastGamePadState.Buttons.Start == ButtonState.Released) ||*/
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_HOME) ||
                        wiiInput.IsKeyPressed(WiimoteNumber.WII_2, WiiKeys.BUTTON_HOME);
            }
        }


        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            LastKeyboardState = CurrentKeyboardState;
            //LastGamePadState = CurrentGamePadState;

            LastMouseState = CurrentMouseState;

            wiiInput.UpdateState();

            //wiiInput.updateAccelList();

            CurrentKeyboardState = Keyboard.GetState();
            //CurrentGamePadState = GamePad.GetState(PlayerIndex.One);

            CurrentMouseState = Mouse.GetState();
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update.
        /// </summary>
        bool IsNewKeyPress(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key));
        }
        #endregion
    }
}
