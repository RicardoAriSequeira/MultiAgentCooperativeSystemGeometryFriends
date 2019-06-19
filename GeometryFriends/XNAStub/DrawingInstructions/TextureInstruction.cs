using FarseerGames.FarseerPhysics.Mathematics;

namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class TextureInstruction : DrawingInsctruction
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Position { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Color TintColor { get; private set; }

        private static Color DEFAULT_TINT = new Color(0, 0, 0, 0);
        public const float DEFAULT_PRIORITY = 1f;

        public TextureInstruction()
        {
            Instruction = InstructionType.Texture;
        }

        public TextureInstruction(Texture2D texture, Vector2 position, int width, int height, Color tint, float priority)
            : this()
        {
            Texture = texture;
            Position = position;
            Width = width;
            Height = height;
            TintColor = tint;
            Priority = priority;
        }

        public TextureInstruction(Texture2D texture, Vector2 position, int width, int height)
            : this(texture, position, width, height, DEFAULT_TINT, DEFAULT_PRIORITY)
        {
        }

        public TextureInstruction(Texture2D texture, Vector2 position, int width, int height, Color tintColor) 
            : this(texture, position, width, height, DEFAULT_TINT, DEFAULT_PRIORITY)
        {
        }
    }
}
