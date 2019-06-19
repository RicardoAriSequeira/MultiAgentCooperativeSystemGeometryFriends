using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.Input;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends.ScreenSystem
{
    internal class SafeAreaScreen : GameScreen
    {
        int width; // Viewport width
        int height; // Viewport height
        int dx; // 5% of width
        int dy; // 5% of height
        Color notActionSafeColor = new Color(255, 255, 255, 50); // Red, 50% opacity
        Color notTitleSafeColor = new Color(255, 255, 255, 25); // Yellow, 50% opacity
        bool enabled = true;

        public SafeAreaScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            width = GameAreaInformation.DRAWING_WIDTH;
            height = GameAreaInformation.DRAWING_HEIGHT;
            dx = (int)(width * 0.05);
            dy = (int)(height * 0.05);
        }

        public override void HandleInput(InputState input)
        {
            //enabled = input.CurrentKeyboardState.IsKeyDown(Keys.F1);
            base.HandleInput(input);
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch)
        {
            if (!enabled) return;
            // Tint the non-action-safe area red
            instructionsBatch.DrawRectangle(new Vector2(0, 0), width, dy, notActionSafeColor);
            instructionsBatch.DrawRectangle(new Vector2(0, height - dy), width, dy, notActionSafeColor);
            instructionsBatch.DrawRectangle(new Vector2(0, dy), dx, height - 2 * dy, notActionSafeColor);
            instructionsBatch.DrawRectangle(new Vector2(width - dx, dy), dx, height - 2 * dy, notActionSafeColor);

            // Tint the non-title-safe area yellow
            instructionsBatch.DrawRectangle(new Vector2(dx, dy), width - 2 * dx, dy, notTitleSafeColor);
            instructionsBatch.DrawRectangle(new Vector2(dx, height - 2 * dy), width - 2 * dx, dy, notTitleSafeColor);
            instructionsBatch.DrawRectangle(new Vector2(dx, 2 * dy), dx, height - 4 * dy, notTitleSafeColor);
            instructionsBatch.DrawRectangle(new Vector2(width - 2 * dx, 2 * dy), dx, height - 4 * dy, notTitleSafeColor);
        }
    }
}


