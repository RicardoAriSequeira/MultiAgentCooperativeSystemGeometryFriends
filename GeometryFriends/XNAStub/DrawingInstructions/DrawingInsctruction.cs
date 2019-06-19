
namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal abstract class DrawingInsctruction
    {
        public InstructionType Instruction { get; set; }
        
        private const float NO_PRIORITY = float.NaN;
        private float _priority = NO_PRIORITY;
        public float Priority { 
            get {
                return _priority;
            } 
            set { 
                _priority = value;
            } 
        }

        internal enum InstructionType
        {
            Rectangle,
            Circle,
            Line,
            Texture,
            String,
            Clear,
            SetMatrix,
            ResetMatrix
        }

        public bool HasPriority()
        {
            return !float.IsNaN(Priority);
        }
    }
}
