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
    internal class TriangleCollectible : GeoShape
    {
        //Body body;
        //Geom geom;
        internal delegate void CollectibleCaughtHandler();
        internal event CollectibleCaughtHandler collectedEvent;

        const int WIDTH = 50;
        const int HEIGHT = 50;
        const int ROTATION = 95;
        Color COLOR = GameColors.COLLECTIBLE_FILL_COLOR;
        Color BORDER_COLOR = GameColors.COLLECTIBLE_BORDER_COLOR;
        const int COLLISION_GROUP = 0;
        bool collected;
        public const string COLLECTIBLE_ID = "Collectible";
        PhysicsSimulator simulator;

        public TriangleCollectible(Vector2 position)
        {
            this.position = position;
            this.collected = false;
            simulator = null;
        }

        private bool HandleCollision(Geom g1, Geom g2, ContactList contactList)
        {
            if (!collected)
            {
                if (g1.Tag.Equals(CircleCharacter.CIRCLE_CHARACTER_ID) ||
                    g1.Tag.Equals(RectangleCharacter.RECTANGLE_CHARACTER_ID) ||
                    g2.Tag.Equals(CircleCharacter.CIRCLE_CHARACTER_ID) ||
                    g2.Tag.Equals(RectangleCharacter.RECTANGLE_CHARACTER_ID))
                {
                    this.collected = true;
                    //remove the collectible if caught
                    simulator.Remove(this.geom);
                    simulator.Remove(this.body);
                    this.collectedEvent();
                }
            }
            return false;
        }

        public bool IsCollected()
        {
            return this.collected;
        }

        public override void Load(PhysicsSimulator physicsSimulator)
        {
            //keep track of the physics simulator
            simulator = physicsSimulator;

            body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, WIDTH, HEIGHT, 1);
            body.IsStatic = true;
            body.Position = position;
            body.Rotation = ROTATION;

            geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, WIDTH, HEIGHT);
            geom.CollisionGroup = 100;
            geom.Tag = COLLECTIBLE_ID;
            geom.CollisionGroup = COLLISION_GROUP;
            geom.Collision += HandleCollision;
        }

        public override void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            if (!this.collected)
            {
                instructionsBatch.DrawRectangle(new Vector2(geom.Position.X - WIDTH / 2, geom.Position.Y - HEIGHT / 2), WIDTH, HEIGHT, COLOR, DEFAULT_BORDER_SIZE, BORDER_COLOR, geom.Rotation);
            }
        }

        public Vector2 GetPosition()
        {
            return body.Position;
        }
        
        public override XmlNode ToXml(XmlDocument doc)
        {
            XmlElement node;
            node = doc.CreateElement(COLLECTIBLE_ID);
            node.SetAttribute("X", this.body.Position.X.ToString());
            node.SetAttribute("Y", this.body.Position.Y.ToString());
            return node;
        }

        public CollectibleRepresentation GetRepresentation()
        {
            return new CollectibleRepresentation(this.position.X, this.position.Y);
        }
    }
}