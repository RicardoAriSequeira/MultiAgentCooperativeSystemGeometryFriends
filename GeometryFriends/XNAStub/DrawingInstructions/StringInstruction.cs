using FarseerGames.FarseerPhysics.Mathematics;

namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class StringInstruction : DrawingInsctruction
    {
        public SpriteFont Font { get; private set; }
        public string Text { get; private set; }
        public Vector2 Position { get; private set; }
        public Color TextColor { get; private set; }
        public float Scale { get; private set; }
        public float Rotation { get; private set; }
        private const float DEFAULT_ROTATION = 0f;
        private const float DEFAULT_SCALE = 1f;
        public const float DEFAULT_PRIORITY = 1;

        public StringInstruction()
        {
            Instruction = InstructionType.String;
        }

        public StringInstruction(SpriteFont font, string text, Vector2 position, Color textColor, float scale, float rotation, float priority)
            : this()
        {
            Font = font;
            Text = text;
            Position = position;
            TextColor = textColor;
            Scale = scale;
            Rotation = rotation;
            Priority = priority;
        }

        public StringInstruction(SpriteFont font, string text, Vector2 position, Color textColor) 
            : this (font, text, position, textColor, DEFAULT_SCALE, DEFAULT_ROTATION, DEFAULT_PRIORITY)
        {
        }

        public StringInstruction(SpriteFont font, string text, Vector2 position, float scale, Color textColor)
            : this(font, text, position, textColor, scale, DEFAULT_ROTATION, DEFAULT_PRIORITY)
        {
        }

        public StringInstruction(SpriteFont font, string text, Vector2 position, float scale, Color textColor, float rotation)
            : this(font, text, position, textColor, scale, rotation, DEFAULT_PRIORITY)
        {
        }
    }
}
