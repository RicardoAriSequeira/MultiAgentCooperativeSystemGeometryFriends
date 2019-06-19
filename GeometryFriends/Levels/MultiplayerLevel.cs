using GeometryFriends.Input;
using GeometryFriends.XNAStub;

namespace GeometryFriends.Levels
{
    internal class MultiplayerLevel : XMLLevel
    {
        protected CircleWiiInputSchema wiiBallControls;
        protected CircleKeyboardControl keyBallControls;

        protected RectangleWiiInputSchema wiiSquareControls;
        protected RectangleKeyboardControl keySquareControls;
        protected bool upSoundPlayedOnce = false;
        protected bool downSoundPlayedOnce = false;
        protected bool altControl = false;
        private double timesincelastlog;

        public MultiplayerLevel(int levelNumber)
            : base(levelNumber)
        {
            //MojoLog.Instance().WriteLine("Level " + levelNumber + " - World " + XMLLevelParser.getWorldName());

            Level.IsMulti(true);

            if (GeometryFriends.Properties.Settings.Default.controls)
            {
                this.wiiBallControls = new CircleStandardControl();
                this.wiiSquareControls = new RectangleStandardControl();
            }
            else
            {
                this.wiiBallControls = new CircleAltControl();
                this.wiiSquareControls = new RectangleAltControl();
            }

            this.keyBallControls = new CircleKeyboardControl();
            this.keySquareControls = new RectangleKeyboardControl();            
        }

        public MultiplayerLevel(int levelNumber, int worldNumber)
            : base(levelNumber, worldNumber)
        {
            Level.IsMulti(true);

            if (GeometryFriends.Properties.Settings.Default.controls)
            {
                this.wiiBallControls = new CircleStandardControl();
                this.wiiSquareControls = new RectangleStandardControl();
            }
            else
            {
                this.wiiBallControls = new CircleAltControl();
                this.wiiSquareControls = new RectangleAltControl();
            }

            this.keyBallControls = new CircleKeyboardControl();
            this.keySquareControls = new RectangleKeyboardControl();
        }

        protected override void HandleCircleControls(InputState input)
        {
            CircleWiiInputSchema ballControls;

            if (!input.wiiInput.isWiimoteOn(wiiBallControls.ControllerNumber))
                ballControls = keyBallControls;
            else
                ballControls = wiiBallControls;


            if (ballControls.Grow(input))
            {
                this.contractSoundInstance.Stop();
                circle.Grow();


                if (Engine.loggerStartFlag)
                    Engine.logger_circle.WriteLine(GetElapsedGameTime() + " - Grow");

                if (!this.growSoundInstance.IsPlaying && !circle.isBig)
                {
                    this.growSoundInstance.Play();
                    this.contractSoundInstance.Stop();
                }
            }
            else
            {
                this.growSoundInstance.Stop();
                circle.Shrink();
                if (!this.contractSoundInstance.IsPlaying && !circle.isSmall)
                {
                    this.growSoundInstance.Stop();
                    this.contractSoundInstance.Play();
                }
            }
            if (ballControls.MoveRight(input))
            {
                circle.SpinRight();
                if (Engine.loggerStartFlag)
                    Engine.logger_circle.WriteLine(GetElapsedGameTime() + " - Roll Right");
            }
            if (ballControls.MoveLeft(input))
            {
                circle.SpinLeft();
                if (Engine.loggerStartFlag)
                    Engine.logger_circle.WriteLine(GetElapsedGameTime() + " - Roll Left");
            }

                float jumpValue = ballControls.Jump(input);
                if ( jumpValue != 0 && circle.GetCollisionState() == true)
                {
                    circle.Jump(jumpValue);
                    circle.SetCollisionState(false);

                    if (Engine.loggerStartFlag)
                        Engine.logger_circle.WriteLine(GetElapsedGameTime() + " - Jump");

                    if(!this.jumpSoundInstance.IsPlaying)
                        this.jumpSoundInstance.Play();
                }                
        }

        protected override void HandleRectangleControls(InputState input)
        {
            RectangleWiiInputSchema squareControls;
            if (!input.wiiInput.isWiimoteOn(wiiSquareControls.ControllerNumber))
                squareControls = keySquareControls;
            else
                squareControls = wiiSquareControls;

            if (squareControls.MorphUp(input))
            {
                rectangle.StretchVerticalUp();


                if (Engine.loggerStartFlag)
                    Engine.logger_rect.WriteLine(GetElapsedGameTime() + " - Morph Up");

                if (!this.morphUpSoundInstance.IsPlaying && rectangle.GetCanStretch() && !upSoundPlayedOnce)
                {
                    this.morphUpSoundInstance.Play();
                    upSoundPlayedOnce = true;
                }
            }
            else
            {
                this.morphUpSoundInstance.Stop();
                upSoundPlayedOnce = false;
            }

            if (squareControls.MorphDown(input))
            {
                rectangle.StretchVerticalDown();

                if (Engine.loggerStartFlag)
                    Engine.logger_rect.WriteLine(GetElapsedGameTime() + " - Morph Down");

                if (!this.morphDownSoundInstance.IsPlaying && rectangle.GetCanStretch() && !downSoundPlayedOnce)
                {
                    this.morphDownSoundInstance.Play();
                    downSoundPlayedOnce = true;
                }
            }
            else
            {
                downSoundPlayedOnce = false;
                this.morphDownSoundInstance.Stop();
            }

            if (squareControls.MoveLeft(input))
            {
                rectangle.SlideLeft();

                if (Engine.loggerStartFlag)
                    Engine.logger_rect.WriteLine(GetElapsedGameTime() + " - Slide Left");
            }
            if (squareControls.MoveRight(input))
            {
                rectangle.SlideRight();

                if (Engine.loggerStartFlag)
                    Engine.logger_rect.WriteLine(GetElapsedGameTime() + " - Slide Right");
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            timesincelastlog += gameTime.ElapsedGameTime.TotalMilliseconds;
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        protected override void EndGame() {
            // empty
        }
    }
}
