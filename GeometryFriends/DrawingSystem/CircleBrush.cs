using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;

namespace GeometryFriends.DrawingSystem {
    internal class CircleBrush {

        public CircleBrush() { }

        public CircleBrush(int radius, Color color, Color borderColor) {
            this.color = color;
            this.borderColor = borderColor;
            this.radius = radius;
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

        private Vector2 positionCorrection = new Vector2(2, 2);
        private int radius = 4;
        public int Radius {
            get { return radius; }
            set {
                positionCorrection = new Vector2(value / 2, value / 2);
                radius = value; 
            }
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch, Vector2 position)
        {
            instructionsBatch.DrawCircle(position - positionCorrection, Radius, Color, BorderColor);
        }
    }
}