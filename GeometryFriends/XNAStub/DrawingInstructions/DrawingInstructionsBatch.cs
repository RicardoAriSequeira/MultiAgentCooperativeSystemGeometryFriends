using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using System.Collections.Generic;

namespace GeometryFriends.XNAStub.DrawingInstructions
{
    internal class DrawingInstructionsBatch
    {
        public IGraphicsDevice GraphicsDevice { get; set; }
        public List<DrawingInsctruction> Instructions { get; set; }
        public HashSet<float> Priorities { get; private set; }

        public int DWidth {
            get { return GameAreaInformation.DRAWING_WIDTH; }
            //set { GameAreaInformation.DRAWING_WIDTH = value; } 
        }
        public int DHeight {
            get { return GameAreaInformation.DRAWING_HEIGHT; } 
            //set { GameAreaInformation.DRAWING_HEIGHT = value; } 
        }

        public DrawingInstructionsBatch (IGraphicsDevice graphicsDevice)
	    {
            Instructions = new List<DrawingInsctruction>();
            GraphicsDevice = graphicsDevice;
	    }

        public void Reset()
        {
            Instructions = new List<DrawingInsctruction>();
            Priorities = new HashSet<float>();
        }

        public void SetTransformMatrix(float translateX, float translateY, float rotation, float scale)
        {
            Instructions.Add(new SetMatrixInstruction(translateX, translateY, rotation, scale));
        }

        public void ResetTransformMatrix()
        {
            Instructions.Add(new ResetMatrixInstruction());
        }

        public void Clear(Color clearColor)
        {
            Instructions.Add(new ClearInstruction(clearColor));
        }

        public Vector2 MeasureString(string toMeasure, SpriteFont font)
        {
            return GraphicsDevice.MeasureString(toMeasure, font);
        }

        public void DrawString(SpriteFont font, string text, Vector2 position, Color textColor)
        {
            Priorities.Add(StringInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new StringInstruction(font, text, position, textColor));
        }

        public void DrawString(SpriteFont font, string text, Vector2 position, float scale, Color textColor)
        {
            Priorities.Add(StringInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new StringInstruction(font, text, position, scale, textColor));
        }

        public void DrawString(SpriteFont font, string text, Vector2 position, float scale, Color textColor, float rotation)
        {
            Priorities.Add(StringInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new StringInstruction(font, text, position, scale, textColor, rotation));
        }

        public void DrawRectangle(Vector2 position, int width, int height, Color rectangleColor)
        {
            Priorities.Add(RectangleInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new RectangleInstruction(position, width, height, rectangleColor));
        }

        public void DrawRectangle(Vector2 position, int width, int height, Color rectangleColor, float priority)
        {
            Priorities.Add(priority);
            Instructions.Add(new RectangleInstruction(position, width, height, rectangleColor, priority));
        }

        public void DrawRectangle(Vector2 position, int width, int height, Color rectangleColor, Color tintColor)
        {
            Priorities.Add(RectangleInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new RectangleInstruction(position, width, height, rectangleColor, tintColor));
        }
        public void DrawRectangle(Vector2 position, int width, int height, Color rectangleColor, int borderSize, Color borderColor)
        {
            Priorities.Add(RectangleInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new RectangleInstruction(position, width, height, rectangleColor, borderSize, borderColor));
        }

        public void DrawRectangle(Vector2 position, int width, int height, Color rectangleColor, int borderSize, Color borderColor, float rotation)
        {
            Priorities.Add(RectangleInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new RectangleInstruction(position, width, height, rectangleColor, borderSize, borderColor, rotation));
        }

        public void DrawRectangle(Vector2 position, int width, int height, Color rectangleColor, int borderSize, Color borderColor, float rotation, float priority)
        {
            Priorities.Add(priority);
            Instructions.Add(new RectangleInstruction(position, width, height, rectangleColor, borderSize, borderColor, rotation, priority));
        }

        public void DrawCircle(Vector2 position, int radius, Color fillColor)
        {
            Priorities.Add(CircleInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new CircleInstruction(position, radius, fillColor));
        }

        public void DrawCircle(Vector2 position, int radius, Color fillColor, Color borderColor)
        {
            Priorities.Add(CircleInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new CircleInstruction(position, radius, fillColor, borderColor));
        }
        public void DrawCircle(Vector2 position, int radius, float rotation, Vector2 rotationPoint, Color fillColor, Color borderColor, int borderThickness)
        {
            Priorities.Add(CircleInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new CircleInstruction(position, radius, rotation, rotationPoint, fillColor, borderColor, borderThickness));
        }

        public void DrawTexture(Texture2D texture, Vector2 position, int width, int height)
        {
            Priorities.Add(TextureInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new TextureInstruction(texture, position, width, height));
        }

        public void DrawTexture(Texture2D texture, Vector2 position, int width, int height, Color tintColor)
        {
            Priorities.Add(TextureInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new TextureInstruction(texture, position, width, height, tintColor));
        }

        public void DrawLine(Vector2 startPoint, Vector2 endPoint, int thickness, Color lineColor)
        {
            Priorities.Add(LineInstruction.DEFAULT_PRIORITY);
            Instructions.Add(new LineInstruction(startPoint, endPoint, thickness, lineColor));
        }
    }
}
