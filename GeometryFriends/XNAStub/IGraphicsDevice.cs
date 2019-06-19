using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Collections.Generic;

namespace GeometryFriends.XNAStub
{
    /// <summary>
    /// Interface to abstract the graphics device that is actually used to represent the game environment.
    /// </summary>
    internal interface IGraphicsDevice
    {        
        //drawing methods
        void RepaintDevice(object sender, List<DrawingInsctruction> instructions, HashSet<float> priorities);
        Vector2 MeasureString(string toMeasure, SpriteFont font);

        //start/close methods
        void ViewerExit();

        //extended XNAStub method in order to allow acess to the gamePanelBackBuffer and obtain a screenshot
        System.Drawing.Bitmap getScreenshot();
    }
}
