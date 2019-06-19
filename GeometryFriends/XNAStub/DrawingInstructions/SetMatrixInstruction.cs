
namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class SetMatrixInstruction : DrawingInsctruction
    {
        public float TranslateX { get; set; }
        public float TranslateY { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }

        public SetMatrixInstruction(float translateX, float translateY, float rotation, float scale) 
        {
            Instruction = InstructionType.SetMatrix;
            TranslateX = translateX;
            TranslateY = translateY;
            Rotation = rotation;
            Scale = scale;
        }
    }
}
