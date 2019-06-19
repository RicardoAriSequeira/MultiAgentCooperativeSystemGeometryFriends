using System.Drawing;

namespace GeometryFriends.XNAStub
{
    internal class Texture2D
    {   
        public int Width { get; private set; }
        public int Height { get; private set; }
        public string Filepath { get; private set; }

        public Texture2D(string underlyingImagePath)            
        {
            Filepath = underlyingImagePath;
            Image tmp = Image.FromFile(underlyingImagePath);
            Width = tmp.Width;
            Height = tmp.Height;

            tmp.Dispose();
        }

    }
}
