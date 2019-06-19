
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;

namespace GeometryFriends.DrawingSystem {
    internal class LineBrush {
        private Vector2 origin;
        private Vector2 scale;
        private float rotation;
        Vector2 xVector = new Vector2(1, 0);
        private Texture2D lineTexture;

        public LineBrush() {
            origin = new Vector2(0, thickness / 2f + 1);
        }

        public LineBrush(int thickness, Color color) {
            this.color = color;
            this.thickness = thickness;
            origin = new Vector2(0, thickness / 2f + 1);
        }

        private int thickness = 1;
        public int Thickness {
            get { return thickness; }
            set { thickness = value; }
        }

        private Color color = Color.Black;
        public Color Color {
            get { return color; }
            set { color = value; }
        }

        Vector2 difference;
        public void Draw(DrawingInstructionsBatch instructionsBatch, Vector2 startPoint, Vector2 endPoint)
        {
            Vector2.Subtract(ref endPoint, ref startPoint, out difference);
            instructionsBatch.DrawLine(startPoint, endPoint, thickness, Color);
        }
    }
}
