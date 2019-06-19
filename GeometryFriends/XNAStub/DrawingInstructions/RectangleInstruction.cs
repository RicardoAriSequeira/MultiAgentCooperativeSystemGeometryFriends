using FarseerGames.FarseerPhysics.Mathematics;

namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class RectangleInstruction : DrawingInsctruction
    {
        public Vector2 Position { get; private set; }
        public int Width { get; private set; }
        public int Height { get; set; }
        public int BorderThickness { get; private set; }
        public Color FillColor { get; private set; }
        public Color BorderColor { get; private set; }
        public Color TintColor { get; private set; }
        //in radians!
        public float Rotation { get; private set; }

        private const int DEFAULT_BORDER_THICKNESS = 1;
        private const int DEFAULT_ROTATION = 0;
        private static Color DEFAULT_TINT = new Color(0, 0, 0, 0);
        public const float DEFAULT_PRIORITY = 1;

        public RectangleInstruction()
        {
            Instruction = InstructionType.Rectangle;
        }

        public RectangleInstruction(Vector2 position, int width, int height, Color fillColor, Color tintColor, int borderThickness, Color borderColor, float rotation, float priority)
            : this()
        {
            Position = position;
            Width = width;
            Height = height;
            FillColor = fillColor;
            BorderThickness = borderThickness;
            BorderColor = borderColor;
            TintColor = tintColor;
            Rotation = rotation;
            Priority = priority;
        }

        public RectangleInstruction(Vector2 position, int width, int height, Color fillColor)
            : this(position, width, height, fillColor, DEFAULT_TINT, DEFAULT_BORDER_THICKNESS, fillColor, DEFAULT_ROTATION, DEFAULT_PRIORITY)
        {
        }

        public RectangleInstruction(Vector2 position, int width, int height, Color fillColor, float priority)
            : this(position, width, height, fillColor, DEFAULT_TINT, DEFAULT_BORDER_THICKNESS, fillColor, DEFAULT_ROTATION, priority)
        {
        }

        public RectangleInstruction(Vector2 position, int width, int height, Color fillColor, Color tintColor)
            : this(position, width, height, fillColor, tintColor, DEFAULT_BORDER_THICKNESS, fillColor, DEFAULT_ROTATION, DEFAULT_PRIORITY)
        {
        }

        public RectangleInstruction(Vector2 position, int width, int height, Color fillColor, int borderThickness, Color borderColor)
            : this(position, width, height, fillColor, DEFAULT_TINT, borderThickness, borderColor, DEFAULT_ROTATION, DEFAULT_PRIORITY)
        {
        }

        public RectangleInstruction(Vector2 position, int width, int height, Color fillColor, int borderThickness, Color borderColor, float rotation)
            : this(position, width, height, fillColor, DEFAULT_TINT, borderThickness, borderColor, rotation, DEFAULT_PRIORITY)
        {
        }

        public RectangleInstruction(Vector2 position, int width, int height, Color fillColor, int borderThickness, Color borderColor, float rotation, float priority)
            : this(position, width, height, fillColor, DEFAULT_TINT, borderThickness, borderColor, rotation, priority)
        {
        }

    }
}
