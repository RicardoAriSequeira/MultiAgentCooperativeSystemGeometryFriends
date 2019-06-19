using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Xml;

namespace GeometryFriends.Levels.Shared
{
    internal class InfoArea : IXmlSerializable
    {
        private CircleBrush brush;

        private int radios;

        private string message;
        private double messageTTL;
        private string tip;
        private Vector2 position;        
        InGameMessageLayer messageLayer;

        public InfoArea(string message, string tip, Vector2 position, int radius)
        {
            this.message = message;
            this.tip = tip;
            this.position = position;
            this.radios = radius;
            this.messageTTL = 0;
        }

        public void Load(InGameMessageLayer messageLayer)
        {
            brush = new CircleBrush(radios, Color.Black, Color.Green);          

            this.messageLayer = messageLayer;
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch, Vector2 circlePosition, Vector2 rectanglePosition, SpriteFont spriteFont)
        {            
            float squared = this.radios * this.radios;
            //brush.Draw(spriteBatch, this.position);
            if ((circlePosition - this.position).LengthSquared() < squared ||
                (rectanglePosition - this.position).LengthSquared() < squared)
            {
                instructionsBatch.DrawString(spriteFont, this.tip, new Vector2(this.position.X - (instructionsBatch.MeasureString(this.tip, spriteFont).X / 2), this.position.Y - 100), Color.White);
                if (messageTTL == 0)
                {
                    messageTTL = messageLayer.AddMessage(message);
                }
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
             XmlElement node;
            node = doc.CreateElement("Tip");
            node.SetAttribute("message", this.message);
            node.SetAttribute("tip", this.tip);
            node.SetAttribute("X", (this.position.X).ToString());
            node.SetAttribute("Y", (this.position.Y).ToString());
            node.SetAttribute("radius", this.radios.ToString());  //TODO: Check out this attribute
            return node; 
        }

        internal void Update(double elapsedTime)
        {
            if (messageTTL > 0)
            {
                messageTTL -= elapsedTime;
                if (messageTTL < 0)
                {
                    messageTTL = 0;
                }
            }
        }
    }
}
