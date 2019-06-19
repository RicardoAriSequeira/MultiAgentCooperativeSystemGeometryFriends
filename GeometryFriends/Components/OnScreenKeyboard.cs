#region Using Statements
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
#endregion

namespace GeometryFriends.Components
{
    internal class OnScreenKeyboard : DrawableGameComponent
    {
        public delegate void StringEnteredHandler(String s, Boolean success);
        public event StringEnteredHandler KeyboardStringEntered;

        ContentManager content;
        SpriteFont spriteFont;

        int panelWidth = 640;
        int panelHeight = 512;
        Color panelColor = new Color(100, 100, 100, 200);               

        private String[,] keys = new String[5, 10] {
                            {"1",   "2",    "3",        "4",    "5",    "6",    "7",    "8",        "9",    "0"},
                            {"q",   "w",    "e",        "r",    "t",    "y",    "u",    "i",        "o",    "p"},
                            {"a",   "s",    "d",        "f",    "g",    "h",    "j",    "k",        "l",    ""},
                            {"z",   "x",    "c",        "v",    "b",    "n",    "m",    "Space",    "",     ""},
                            {"Done","",     "Cancel",   "",     "",     "Bksp", "",     "Caps",     "",     ""}};

        private Vector2 currentKey;
        private int stringMaxSize;
        private String keyboardMessage;
        private String currentString;
        private bool firstInputKey = true;
        private bool toggleCaps;
        private bool visible;

        Vector2 keyboardPosition;
        private const int KEY_SIZE = 56;
        private Color KEYBOARD_COLOR = Color.White;
        private Color KEYBOARD_ACTIVE_KEY_COLOR = Color.YellowGreen;

        private static string DEFAULT_PLAYER_NAME = "Team";
        
        public new bool Visible
        {
            get
            {
                return this.visible;
            }
        }

        public OnScreenKeyboard(IGraphicsDevice drawingDevice)
            : base(drawingDevice)
        {
            content = new ContentManager();
            currentKey = new Vector2(4, 0);
            toggleCaps = false;
            visible = false;
            currentString = "";
        }

        protected override void LoadContent()
        {
            spriteFont = content.Load<SpriteFont>(@"Content\Fonts\gameFont.spritefont");
            keyboardPosition = new Vector2(
                (GameAreaInformation.DRAWING_WIDTH / 2) - (KEY_SIZE * (keys.GetLength(1) - 0.5f) / 2),
                (GameAreaInformation.DRAWING_HEIGHT / 2) - (KEY_SIZE * ((keys.GetLength(0) + 2.5f) / 2)));
            panelWidth = KEY_SIZE * (keys.GetLength(1) + 2);
            panelHeight = KEY_SIZE * (keys.GetLength(0) + 4);
        }

        protected override void UnloadContent()
        {
            content.Unload();
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            if (visible)
            {
                Vector2 panelOrigin = new Vector2(GameAreaInformation.DRAWING_WIDTH / 2 - panelWidth / 2, GameAreaInformation.DRAWING_HEIGHT / 2 - panelHeight / 2);

                instructionsBatch.DrawRectangle(panelOrigin, panelWidth, panelHeight, panelColor);

                instructionsBatch.DrawString(spriteFont, keyboardMessage, panelOrigin - new Vector2(0, 200), KEYBOARD_COLOR);

                for (int i = 0; i < keys.GetLength(0); ++i)
                {
                    for (int j = 0; j < keys.GetLength(1); ++j)
                    {
                        if (new Vector2(i, j) == currentKey)
                            instructionsBatch.DrawString(
                                spriteFont,
                                ((toggleCaps && keys[i, j].Length == 1) ? keys[i, j].ToUpper() : keys[i, j]),
                                new Vector2(keyboardPosition.X + (j * KEY_SIZE), keyboardPosition.Y + (i * KEY_SIZE)),
                                1.5f,
                                KEYBOARD_ACTIVE_KEY_COLOR);
                        else
                            instructionsBatch.DrawString(
                                spriteFont,
                                ((toggleCaps && keys[i, j].Length == 1) ? keys[i, j].ToUpper() : keys[i, j]),
                                new Vector2(keyboardPosition.X + (j * KEY_SIZE), keyboardPosition.Y + (i * KEY_SIZE)),
                                KEYBOARD_COLOR);
                    }
                }

                //draw current string
                Vector2 position = new Vector2(keyboardPosition.X, keyboardPosition.Y + (6 * KEY_SIZE));
                String extra;
                if (gameTime.TotalGameTime.Seconds % 2 == 0)
                    extra = "";
                else
                    extra = "_";

                instructionsBatch.DrawString(spriteFont, currentString + extra, position, 1.5f, KEYBOARD_ACTIVE_KEY_COLOR);
            }
        }

        public override void Update(GameTime gameTime)
        {
        }

        public void HandleInput(InputState input)
        {
            if (visible)
            {
                WiimoteHandler wiiput = input.wiiInput;
               //if (wiiput.isWiimoteOn(WiimoteNumber.WII_1))
               // {                   
                if (wiiput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_DOWN) &&
                    !wiiput.IsKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_DOWN) ||
                    (input.CurrentKeyboardState.IsKeyDown(Keys.Down) &&
                    input.LastKeyboardState.IsKeyUp(Keys.Down)))
                {

                    if (currentKey.X >= keys.GetLength(0) - 1)
                    {
                        currentKey.X = 0;
                    }
                    else
                    {
                        currentKey.X += 1;
                    }
                    SetValidKey(true);
                    return;
                }
                if (wiiput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_UP) &&
                    !wiiput.IsKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_UP) ||
                    (input.CurrentKeyboardState.IsKeyDown(Keys.Up) &&
                    input.LastKeyboardState.IsKeyUp(Keys.Up)))
                {
                    if (currentKey.X <= 0)
                    {
                        currentKey.X = keys.GetLength(0) - 1;
                    }
                    else
                    {
                        currentKey.X -= 1;
                    }
                    SetValidKey(true);
                    return;
                }

                if (wiiput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_LEFT) &&
                    !wiiput.IsKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_LEFT) ||
                    (input.CurrentKeyboardState.IsKeyDown(Keys.Left) &&
                    input.LastKeyboardState.IsKeyUp(Keys.Left)))
                {
                    if (currentKey.Y == 0)
                    {
                        currentKey.Y = keys.GetLength(1) - 1;
                    }
                    else
                    {
                        currentKey.Y -= 1;
                    }
                    SetValidKey(true);
                    return;
                }
                if (wiiput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_RIGHT) &&
                    !wiiput.IsKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_RIGHT) ||
                    (input.CurrentKeyboardState.IsKeyDown(Keys.Right) &&
                    input.LastKeyboardState.IsKeyUp(Keys.Right)))
                {
                    if (currentKey.Y >= keys.GetLength(1)-1)
                    {
                        currentKey.Y = 0;
                    }
                    else
                    {
                        currentKey.Y += 1;
                    }
                    SetValidKey(false);
                    return;
                }
                if (wiiput.IsKeyPressed(WiimoteNumber.WII_1, WiiKeys.BUTTON_A) &&
                    !wiiput.IsKeyHeld(WiimoteNumber.WII_1, WiiKeys.BUTTON_A) ||
                    input.CurrentKeyboardState.IsKeyDown(Keys.Enter) &&
                    input.LastKeyboardState.IsKeyUp(Keys.Enter))
                {
                    UpdateString();
                    return;
                }
                //}
                //else
                /*{
                    if (input.CurrentMouseState.X < keyboardPosition.X + keys.GetLength(1) * KEY_SIZE &&
                        input.CurrentMouseState.Y < keyboardPosition.Y + keys.GetLength(0) * KEY_SIZE)
                    {
                        currentKey.Y = (int)(input.CurrentMouseState.X - keyboardPosition.X) / KEY_SIZE;
                        currentKey.X = (int)(input.CurrentMouseState.Y - keyboardPosition.Y) / KEY_SIZE;
                    }
                    if (currentKey.X >= 0 && currentKey.X < keys.GetLength(0) && currentKey.Y >= 0 && currentKey.Y < keys.GetLength(1))
                    {
                        setValidKey();
                    }

                    if (input.CurrentMouseState.LeftButton == ButtonState.Pressed &&
                        input.LastMouseState.LeftButton == ButtonState.Released)
                    {
                        updateString();
                    }
                }*/
            }
        }

        /// <summary>
        /// Makes sure only keys with a value are selected. useful for keys longer than a single character
        /// </summary>
        private void SetValidKey(bool checkLeft)
        {
            while (keys[(int)currentKey.X, (int)currentKey.Y] == "")
            {
                if (checkLeft)
                {
                    if (currentKey.Y <= 0)
                    {
                        currentKey.Y = keys.GetLength(1) - 1;
                    }
                    else
                    {
                        currentKey.Y -= 1;
                    }
                }
                else
                {
                    if (currentKey.Y >= keys.GetLength(1) - 1)
                    {
                        currentKey.Y = 0;
                    }
                    else
                    {
                        currentKey.Y += 1;
                    }
                }
            }
        }

        public void Enable(StringEnteredHandler s, String title, int maxSize, String defaultString)
        {
            this.keyboardMessage = title;
            this.stringMaxSize = maxSize;
            if (defaultString.Equals("player") && !string.IsNullOrEmpty(DEFAULT_PLAYER_NAME))
                this.currentString = DEFAULT_PLAYER_NAME;
            else
                this.currentString = defaultString;
            this.currentKey = new Vector2(4, 0);
            this.KeyboardStringEntered += s;
            //s(currentString, false);
            this.toggleCaps = true;
            this.firstInputKey = true;
            this.visible = true;
        }

        protected void UpdateString()
        {
            String currentKeyValue = keys[(int)currentKey.X, (int)currentKey.Y];

            if (currentString.Length < stringMaxSize)
            {
                switch (currentKeyValue)
                {
                    case "":
                        break;
                    case "Cancel":
                        this.KeyboardStringEntered(currentString, false);
                        this.visible = false;
                        this.KeyboardStringEntered = null;                        
                        break;
                    case "Done":
                        this.KeyboardStringEntered(currentString, true);
                        this.visible = false;
                        this.KeyboardStringEntered = null;
                        break;
                    case "Bksp":
                        if (this.firstInputKey)
                            this.currentString = "";
                        else
                        {
                            if (currentString.Length > 0)
                            {
                                this.currentString = this.currentString.Remove(currentString.Length - 1);
                            }
                            else
                            {
                                this.currentString = "";
                            }

                        }
                        break;
                    case "Space":
                        if (currentString.Length > 1)
                        {
                            this.currentString += " ";
                            this.toggleCaps = true;
                        }
                        break;
                    case "Caps":
                        this.toggleCaps = !toggleCaps;
                        break;
                    default:
                        if (toggleCaps)
                        {
                            this.currentString += currentKeyValue.ToUpper();
                            if (currentString.Length == 1 || (currentString.Length > 2 && currentString[currentString.Length-2].Equals(' ')))
                            {
                                this.toggleCaps = false;
                            }
                        }
                        else
                        {
                            this.currentString += currentKeyValue;
                        }
                        break;
                }
            }
            else
            {
                switch (currentKeyValue)
                {
                    case "Cancel":
                        this.KeyboardStringEntered(currentString, false);
                        this.visible = false;
                        this.KeyboardStringEntered = null;
                        break;
                    case "Done":
                        this.KeyboardStringEntered(currentString, true);
                        this.visible = false;
                        this.KeyboardStringEntered = null;
                        break;
                    case "Bksp":
                        this.currentString = this.currentString.Remove(currentString.Length - 1);
                        break;
                    case "Caps":
                        this.toggleCaps = !toggleCaps;
                        break;
                }
            }
            this.firstInputKey = false;
        }

        // Look at this!!!
        public static void SetDefaultName(string name)
        {
            DEFAULT_PLAYER_NAME = name;
        }

        internal static string GetDefaultName()
        {
            return DEFAULT_PLAYER_NAME;
        }
    }
}
