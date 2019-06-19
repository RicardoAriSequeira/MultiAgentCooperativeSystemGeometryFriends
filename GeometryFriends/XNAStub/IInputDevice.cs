using System.Collections.Generic;

namespace GeometryFriends.XNAStub
{
    /// <summary>
    /// Abstracts the input devices connection so that the game can be connected to different platforms with different ways to process the input.
    /// </summary>
    internal interface IInputDevice
    {
        //start/close methods
        void ViewerExit();
        List<Keys> GetPressedKeys();

        //update methods
        MouseState GetMouseState();
    }
}
