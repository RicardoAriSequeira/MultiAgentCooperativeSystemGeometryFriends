using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;

namespace GeometryFriends.DrawingSystem {
    internal class RectangleBrush {

        public RectangleBrush() { }

        public RectangleBrush(int width, int height, Color color, Color borderColor) {
            this.color = color;
            this.borderColor = borderColor;
            this.width = width;
            this.height = height;
        }

        private Color color = Color.Black;
        public Color Color {
            get { return color; }
            set { color = value; }
        }

        private Color borderColor;

        public Color BorderColor {
            get { return borderColor; }
            set { borderColor = value; }
        }

        private int borderThickness = 1;
        public int BorderThickness
        {
            get { return borderThickness; }
            set { borderThickness = value; }
        }

        private int width = 5;
        public int Width {
            get { return width; }
            set { width = value; }
        }

        private int height;

        public int Height {
            get { return height; }
            set { height = value; }
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch, Vector2 position)
        {
            instructionsBatch.DrawRectangle(position, Width, Height, Color, BorderThickness, BorderColor);
            //new Vector2(radius, radius)
        }
    }
}