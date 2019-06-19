using FarseerGames.FarseerPhysics.Mathematics;
using System.Drawing;

namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class LineInstruction : DrawingInsctruction
    {
        public Vector2 StartPoint { get; private set; }
        public Vector2 EndPoint { get; private set; }
        public int Thickness { get; private set; }
        public Color LineColor { get; private set; }

        public const float DEFAULT_PRIORITY = 1f;

        public LineInstruction(Vector2 startPoint, Vector2 endPoint, int thickness, Color lineColor, float priority)
        {
            Instruction = InstructionType.Line;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Thickness = thickness;
            LineColor = lineColor;
            Priority = priority;
        }

        public LineInstruction(Vector2 startPoint, Vector2 endPoint, int thickness, Color lineColor) 
            : this(startPoint, endPoint, thickness, lineColor, DEFAULT_PRIORITY)
        {
        }

        public Point GetStartPoint()
        {
            return new Point((int)StartPoint.X, (int)StartPoint.Y);
        }

        public Point GetEndPoint()
        {
            return new Point((int)EndPoint.X, (int)EndPoint.Y);
        }
    }
}
