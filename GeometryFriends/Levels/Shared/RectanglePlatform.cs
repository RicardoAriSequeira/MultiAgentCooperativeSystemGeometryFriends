
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.AI.Perceptions.Information;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Xml;

namespace GeometryFriends.Levels.Shared
{
    internal class RectanglePlatform : GeoShape
    {
        int width;
        int height;

        Color color;
        Color borderColor;
        int collisionGroup;

        private const float DEFAULT_DRAWING_PRIORITY = -2;

        public Body getBody()
        {
            return body;
        }

        public Color getColor()
        {
            return color;
        }

        public RectanglePlatform(int width, int height, Vector2 position, bool centered, Color color, Color borderColor, int collisionGroup)
        {
            this.width = width;
            this.height = height;
            if (centered)
            {
                this.position = position;
            }
            else
            {
                this.position.X = position.X + (width / 2);
                this.position.Y = position.Y + (height / 2);
            }
            this.color = color;
            this.borderColor = borderColor;
            this.collisionGroup = collisionGroup;
        }

        public override void Load(PhysicsSimulator physicsSimulator)
        {   
            //use the body factory to create the physics body
            body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);
            body.IsStatic = true;
            body.Position = position;

            geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, width, height);
            geom.CollisionGroup = collisionGroup;
            geom.FrictionCoefficient = .2f;
            geom.RestitutionCoefficient = .2f;
            if (color == GameColors.YELLOW_OBSTACLE_COLOR)
            {
                geom.Tag = "LightGreenPlatform";
            }
            else if (color == GameColors.GREEN_OBSTACLE_COLOR)
            {
                geom.Tag = "GoldPlatform";
            }
            else
            {
                geom.Tag = "Platform-" + position.ToString();
            }
        }

        public override void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.DrawRectangle(new Vector2(geom.Position.X - width / 2, geom.Position.Y - height / 2), width, height, color, DEFAULT_BORDER_SIZE, borderColor, geom.Rotation, DEFAULT_DRAWING_PRIORITY);
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            XmlElement node;
            node = doc.CreateElement("Obstacle");
            node.SetAttribute("width", this.width.ToString());
            node.SetAttribute("height", this.height.ToString());
            node.SetAttribute("X", (this.position.X - (this.width / 2)).ToString());
            node.SetAttribute("Y", (this.position.Y - (this.height / 2)).ToString());
            node.SetAttribute("centered", "false");  //TODO: Check out this attribute
            return node; 
        }

        public ObstacleRepresentation GetRepresentation()
        {
            return new ObstacleRepresentation(this.position.X, this.position.Y, this.width, this.height);
        }
    }
}