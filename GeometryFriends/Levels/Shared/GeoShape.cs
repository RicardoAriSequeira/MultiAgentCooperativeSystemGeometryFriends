using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Drawing;
using System.Xml;

namespace GeometryFriends.Levels.Shared
{
    internal abstract class GeoShape : IXmlSerializable
    {
        protected const int DEFAULT_BORDER_SIZE = 2;
        protected Vector2 position;
        protected Vector2 initialPosition;
        /// <summary>
        /// Returns a rectangle representing the area occupied by this shape
        /// </summary>
        public Rectangle Area
        {
            get
            {
                return new Rectangle((int)(this.position.X - this.Width / 2), (int)(this.position.Y - this.Height/2), (int)this.Width, (int)this.Height); 
            }
        }

        /// <summary>
        /// returns the X coordinate of this shape's center
        /// </summary>
        public float X
        {
            get
            {
                return body.Position.X;
            }
        }

        /// <summary>
        /// returns the Y coordinate of this shape's center
        /// </summary>
        public float Y
        {
            get
            {
                return body.Position.Y;
            }
        }

        public virtual float Width
        {
            get
            {
                return geom.AABB.Width;
            }
        }

        public virtual float Height
        {
            get
            {
                return geom.AABB.Height;
            }
        }

        public Vector2 ResetPosition()
        {
            position = initialPosition;
            return initialPosition;
        }

        protected Body body;
        protected Geom geom;

        public abstract void Load(PhysicsSimulator physicsSimulator);
        public abstract void Draw(DrawingInstructionsBatch instructionsBatch);
        public abstract XmlNode ToXml(XmlDocument doc);
    }
}
