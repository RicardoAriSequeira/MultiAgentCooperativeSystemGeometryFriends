using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GeometryFriends.XNAStub
{
    internal class Game
    {
        public event GameExitHandler GameExit;
        public delegate void GameExitHandler(object sender, EventArgs e);

        //TODO:properly isolate the interactive platform so it is not static
        public static InteractivePlatform GamePlatform { get; set; }
        public IGraphicsDevice GraphicsDevice {
            get {
                if (GamePlatform == null)
                    return null;
                else
                    return GamePlatform.GraphicsDevice;
            }
        }
        public static IInputDevice InputDevice
        {
            get
            {
                if (GamePlatform == null)
                    return null;
                else
                    return GamePlatform.InputDevice;
            }
        }
        public GameComponentCollection Components { get; set; }
        private volatile bool _isActive;
        public bool IsActive {
            get { return _isActive; }
            set { _isActive = value; }
        }        

        private IOrderedEnumerable<DrawableGameComponent> orderedDrawableGameComponents;
        private IOrderedEnumerable<DrawableGameComponent> orderedUpdatableGameComponents;

        private bool _isFixedTimeStep = true;

        private DrawingInstructionsBatch _instructionsBatch;

        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667); // 60fps
        private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02);

        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

        private bool _suppressDraw;

        public bool ExitWhenViewerExits { get; set; }

        // A delegate type for hooking up change notifications.
        public delegate void RepaintDeviceHandler(object sender, List<DrawingInsctruction> instructions, HashSet<float> priorities);
        private event RepaintDeviceHandler RepaintGame;

        public Game()
        {
            GamePlatform = null;
            Components = new GameComponentCollection();
            IsActive = false;            

            orderedDrawableGameComponents = null;
            orderedUpdatableGameComponents = null;

            //listen for components changes and properly order them
            Components.ComponentAdded += Components_ComponentAdded;

            _suppressDraw = false;

            ExitWhenViewerExits = true;
        }

        public Game(InteractivePlatform gamePlatform) : this()
        {
            if (gamePlatform != null)
            {
                GamePlatform = gamePlatform;

                RepaintGame += GamePlatform.GraphicsDevice.RepaintDevice;
                
                //listen for game platform exit
                GamePlatform.InteractivePlatformExit += new InteractivePlatform.InteractivePlatformExitHandler(HandleGameViewerExit);
            }
            _instructionsBatch = new DrawingInstructionsBatch(GraphicsDevice);
        }

        public TimeSpan InactiveSleepTime
        {
            get { return _inactiveSleepTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The time must be positive.", default(Exception));

                _inactiveSleepTime = value;
            }
        }

        /// <summary>
        /// The maximum amount of time we will frameskip over and only perform Update calls with no Draw calls.
        /// MonoGame extension.
        /// </summary>
        public TimeSpan MaxElapsedTime
        {
            get { return _maxElapsedTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The time must be positive.", default(Exception));
                if (value < _targetElapsedTime)
                    throw new ArgumentOutOfRangeException("The time must be at least TargetElapsedTime", default(Exception));

                _maxElapsedTime = value;
            }
        }

        public TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(
                        "The time must be positive and non-zero.", default(Exception));

                if (value != _targetElapsedTime)
                {
                    _targetElapsedTime = value;
                }
            }
        }

        public bool IsFixedTimeStep
        {
            get { return _isFixedTimeStep; }
            set { _isFixedTimeStep = value; }
        }

        protected virtual void Initialize()
        {
            if (GraphicsDevice != null)
            {
                LoadContent();
            }
            foreach (DrawableGameComponent item in Components)
            {
                item.Initialize();
            }
            IsActive = true;
        }

        protected virtual void LoadContent()
        {   
        }

        protected virtual void UnloadContent()
        {
        }

        protected virtual void Update(GameTime gameTime)
        {
            if (orderedUpdatableGameComponents != null)
            {
                foreach (DrawableGameComponent toUpdate in orderedUpdatableGameComponents)
                {
                    toUpdate.Update(gameTime);
                }
            }
        }

        private void DoDraw(GameTime gameTime)
        {
            _instructionsBatch.Reset();
            Draw(gameTime, _instructionsBatch);
        }

        protected virtual void OnRepaintGame(List<DrawingInsctruction> instructions, HashSet<float> priorities)
        {
            if (RepaintGame != null)
                RepaintGame(this, instructions, priorities);
        }

        protected virtual void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            if (GraphicsDevice != null && !_suppressDraw)
            {
                if (orderedDrawableGameComponents != null)
                {
                    foreach (DrawableGameComponent toDraw in orderedDrawableGameComponents)
                    {
                        toDraw.Draw(gameTime, instructionsBatch);
                    }
                }
                //actually paint all the stuff
                //GraphicsDevice.RepaintDevice(instructionsBatch.Instructions);
                OnRepaintGame(instructionsBatch.Instructions, instructionsBatch.Priorities);
            }
        }

        private void Components_ComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            //properly order the components for drawing
            orderedDrawableGameComponents = Components.OrderBy(x => x.DrawOrder);
            //properly order the components for updating
            orderedUpdatableGameComponents = Components.OrderBy(x => x.UpdateOrder);
        }

        public void Exit()
        {
            IsActive = false;
            _suppressDraw = true; 
            if(GamePlatform != null)
                GamePlatform.Exit();
            if (GameExit != null)
                GameExit(this, new EventArgs());
        }

        public void HandleGameViewerExit(object sender)
        {
            if (ExitWhenViewerExits)
            {
                IsActive = false;
                _suppressDraw = true;
            }
        }

        public void ResetElapsedTime()
        {
            _gameTimer.Reset();
            _gameTimer.Start();
            _accumulatedElapsedTime = TimeSpan.Zero;
            _gameTime.ElapsedGameTime = TimeSpan.Zero;
            _previousTicks = 0L;
        }

        public void SuppressDraw()
        {
            _suppressDraw = true;
        }

        public void Run(/*we always run in sychronous mode*/)
        {
            if (!IsActive)
            {
                Initialize();                
            }

            _gameTimer = Stopwatch.StartNew();

            while (IsActive)
            {
                Tick();
            }
        }

        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();
        private Stopwatch _gameTimer;
        private long _previousTicks = 0;
        private int _updateFrameLag;

        public void Tick()
        {
        // NOTE: This code is very sensitive and can break very badly
        // with even what looks like a safe change.  Be sure to test 
        // any change fully in both the fixed and variable timestep 
        // modes across multiple devices and platforms.

        RetryTick:

            // Advance the accumulated elapsed time.
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            // If we're in the fixed timestep mode and not enough time has elapsed
            // to perform an update we sleep off the the remaining time to save battery
            // life and/or release CPU time to other threads and processes.
            if (IsFixedTimeStep && _accumulatedElapsedTime < TargetElapsedTime)
            {
                var sleepTime = (int)(TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;

                // NOTE: While sleep can be inaccurate in general it is 
                // accurate enough for frame limiting purposes if some
                // fluctuation is an acceptable result.

                System.Threading.Thread.Sleep(sleepTime);
                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            if (_accumulatedElapsedTime > _maxElapsedTime)
                _accumulatedElapsedTime = _maxElapsedTime;

            if (IsFixedTimeStep)
            {
                _gameTime.ElapsedGameTime = TargetElapsedTime;
                var stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (_accumulatedElapsedTime >= TargetElapsedTime)
                {
                    _gameTime.TotalGameTime += TargetElapsedTime;
                    _accumulatedElapsedTime -= TargetElapsedTime;
                    ++stepCount;

                    Update(_gameTime);
                }

                //Every update after the first accumulates lag
                _updateFrameLag += Math.Max(0, stepCount - 1);

                //If we think we are running slowly, wait until the lag clears before resetting it
                if (_gameTime.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                        _gameTime.IsRunningSlowly = false;
                }
                else if (_updateFrameLag >= 5)
                {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    _gameTime.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw, then we are not running slowly, so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {
                // Perform a single variable length update.
                _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
                _gameTime.TotalGameTime += _accumulatedElapsedTime;
                _accumulatedElapsedTime = TimeSpan.Zero;

                Update(_gameTime);
            }

            // Draw unless the update suppressed it.
            if (_suppressDraw)
                _suppressDraw = false;
            else
            {
                DoDraw(_gameTime);
            }
        }
    }
}
