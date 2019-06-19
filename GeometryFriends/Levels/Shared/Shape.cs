using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Collisions;
using System.Xml;

namespace GeometryFriends.Levels.Shared
{
    public abstract class Shape : IXmlSerializable
    {
        protected Texture2D texture;
        protected Vector2 position;
        
        public float X
        {
            get
            {
                return body.Position.X;
            }

        }

        public float Y
        {
            get
            {
                return body.Position.Y;
            }

        }

        public float Width
        {
            get
            {
                return texture.Width;
            }
        }

        public float Height
        {
            get
            {
                return texture.Height;
            }
        }

        protected Body body;
        protected Geom geom;

        public abstract void Load(GraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator);
        public abstract void Draw(SpriteBatch spriteBatch);
        public abstract XmlNode toXml(XmlDocument doc);
    }
}
