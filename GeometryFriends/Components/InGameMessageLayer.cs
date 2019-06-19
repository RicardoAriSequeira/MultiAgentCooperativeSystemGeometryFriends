using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Collections.Generic;

namespace GeometryFriends.Levels.Shared
{
    internal class InGameMessageLayer
    {
        private double defaultTime = 5000;
        //private Vector2 TIME_TEXT_POSITION = new Vector2(1100, 40);
        private Color MESSAGE_COLOR = GameColors.GAME_MESSAGE_COLOR;  
        private const double FADING_TIME = 2000;
        private Dictionary<string, double> messages = new Dictionary<string, double>();
        SpriteFont spriteFont;

        public void Load(ContentManager contentManager)
        {
            spriteFont = contentManager.Load<SpriteFont>("Content/Fonts/detailsFont.spritefont");
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch, double elapsedTime)
        {            
            //TIME
            //spriteBatch.DrawString(spriteFont, "Time: " + (int)elapsedTime, TIME_TEXT_POSITION, Color.White);

            //MESSAGE LIST
            int cont = 0;
            foreach (string key in messages.Keys)
            {
                double timeToDisapear = messages[key] - elapsedTime;
                if (timeToDisapear > 0)
                {                    
                    instructionsBatch.DrawString(spriteFont, key, new Vector2(45, 40 + cont++ * 30), timeToDisapear > FADING_TIME ? MESSAGE_COLOR: MESSAGE_COLOR * (float)(timeToDisapear / FADING_TIME));
                }                
            }            
        }

        internal void Update(double elapsedTime)
        {
            List<string> keysToRemove = new List<string>();
            List<string> keysToUpdate = new List<string>();
            foreach (string key in messages.Keys)
            {
                if (messages[key] - elapsedTime > 0)
                {
                    keysToUpdate.Add(key);
                }
                else
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (string key in keysToUpdate)
            {
                messages[key] -= elapsedTime;
            }
            foreach (string key in keysToRemove)
            {
                messages.Remove(key);
            }
        }

        public double AddMessage(string message)
        {
            return AddMessage(message, defaultTime);
        }

        public double AddMessage(string message, double time)
        {
            if (message != "")                            
                messages.Add(message, time);
            return time;
        }
    }
}
