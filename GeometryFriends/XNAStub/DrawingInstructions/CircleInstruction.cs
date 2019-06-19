using FarseerGames.FarseerPhysics.Mathematics;

namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class CircleInstruction : DrawingInsctruction
    {
        public Vector2 Position { get; private set; }
        public int Radius { get; private set; }
        public float Diameter { get { return 2 * Radius; } }
        public int BorderThickness { get; private set; }
        public Color FillColor { get; private set; }
        public Color BorderColor { get; private set; }
        public float Rotation { get; private set; }
        public Vector2 RotationPoint { get; private set; }

        private const int DEFAULT_BORDER_THICKNESS = 1;
        private const int DEFAULT_ROTATION = 0;
        private static Vector2 DEFAULT_ROTATION_POINT = new Vector2(0,0);
        public const float DEFAULT_PRIORITY = 1f;

        public CircleInstruction()
        {
            Instruction = InstructionType.Circle;
        }

        public CircleInstruction(Vector2 position, int radius, int borderThickness, Color fillColor, Color borderColor, float rotation, Vector2 rotationPoint, float priority) 
            : this()
        {
            Position = position;
            Radius = radius;
            BorderThickness = borderThickness;
            FillColor = fillColor;
            BorderColor = borderColor;
            Rotation = rotation;
            RotationPoint = rotationPoint;
            Priority = priority;
        }

        public CircleInstruction(Vector2 position, int radius, Color fillColor)
            : this(position, radius, DEFAULT_BORDER_THICKNESS, fillColor, fillColor, DEFAULT_ROTATION, DEFAULT_ROTATION_POINT, DEFAULT_PRIORITY)
        {
        }

        public CircleInstruction(Vector2 position, int radius, Color fillColor, Color borderColor)
            : this(position, radius, DEFAULT_BORDER_THICKNESS, fillColor, borderColor, DEFAULT_ROTATION, DEFAULT_ROTATION_POINT, DEFAULT_PRIORITY)
        {
        }

        public CircleInstruction(Vector2 position, int radius, float rotation, Vector2 rotationPoint, Color fillColor, Color borderColor, int borderThickness)
            : this(position, radius, borderThickness, fillColor, borderColor, rotation, rotationPoint, DEFAULT_PRIORITY)
        {
        }
    }
}
