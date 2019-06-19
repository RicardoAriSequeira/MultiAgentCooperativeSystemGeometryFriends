
namespace GeometryFriends.XNAStub
{
    internal class InteractivePlatform
    {
        public event InteractivePlatformExitHandler InteractivePlatformExit;
        public delegate void InteractivePlatformExitHandler(object sender);

        IGraphicsDevice _graphicsDevice;
        public IGraphicsDevice GraphicsDevice {
            get { return _graphicsDevice; }
            protected set { _graphicsDevice = value;}
        }
        IInputDevice _inputDevice;
        public IInputDevice InputDevice
        {
            get { return _inputDevice; }
            protected set { _inputDevice = value; }
        }

        public InteractivePlatform(IGraphicsDevice graphicsDevice, IInputDevice inputDevice)
        {
            GraphicsDevice = graphicsDevice;
            InputDevice = inputDevice;
        }

        public void Exit()
        {
            _graphicsDevice.ViewerExit();
            _inputDevice.ViewerExit();
        }

        public void DoInteractivePlatformExit(object sender)
        {
            if(InteractivePlatformExit != null)
                InteractivePlatformExit(sender);
        }
    }
}
