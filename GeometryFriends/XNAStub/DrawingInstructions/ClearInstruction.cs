
namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class ClearInstruction : DrawingInsctruction
    {
        public Color ClearColor { get; private set; }

        public ClearInstruction(Color clearColor)
        {
            Instruction = InstructionType.Clear;
            ClearColor = clearColor;
        }
    }
}
