using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
using System.Xml;

namespace GeometryFriends.Levels.Shared
{
    internal class Elevator : IXmlSerializable
    {
        private Color COLL_ICON_COLOR = GameColors.COLLECTIBLE_FILL_COLOR;
        private const int COLL_ICON_WIDTH = 20;
        private const int COLL_ICON_HEIGHT = 20;
        SpriteFont collSpriteFont;

        Body platformBody;
        Geom platformGeom;

        int width;
        int height;
        int collectiblesNeeded;
        int collectiblesCollected = 0;
        bool repeatMovement;
        Vector2 position;
        Vector2 startPosition;
        Vector2 endPosition;
        Color color;
        Color borderColor;
        int collisionGroup;
        Vector2 platformOrigin;
        String direction;

        public Body GetBody()
        {
            return platformBody;
        }

        public Color GetColor()
        {
            return color;
        }

        private bool HandleCollision(Geom g1, Geom g2, ContactList contactList)
        {
            if(g1.Tag.Equals("LightGreenPlatform")){
                if (g2.Tag != null && g2.Tag.Equals(RectangleCharacter.RECTANGLE_CHARACTER_ID))
                {
                    return true;
                }
            }
            if(g1.Tag.Equals("GoldPlatform")){
                if (g2.Tag != null && g2.Tag.Equals(CircleCharacter.CIRCLE_CHARACTER_ID))
                {
                    return true;
                }
            }
            return false;
        }

        public Elevator(int width, int height, Vector2 startPosition, Vector2 endPosition, string direction, int collectiblesNeeded, bool repeatMovement,  Color color, Color borderColor, int collisionGroup)
        {
            this.width = width;
            this.height = height;          

            //POSITIONS ARE NOT CENTERED SO WE CENTER THEM:
            this.startPosition.X = (startPosition.X * 2 + width) / 2;
            this.startPosition.Y = (startPosition.Y * 2 + height) / 2;
            this.endPosition.X = (endPosition.X * 2 + width) / 2;
            this.endPosition.Y = (endPosition.Y * 2 + height) / 2;
            
            this.position = new Vector2(this.startPosition.X,this.startPosition.Y);

            this.collectiblesNeeded = collectiblesNeeded;
            this.repeatMovement = repeatMovement;
            this.direction = direction;
            this.color = color;
            this.borderColor = borderColor;
            this.collisionGroup = collisionGroup;
        }

        public void Update(int collectiblesCollected)
        {
            platformBody.ClearForce();
            this.collectiblesCollected = collectiblesCollected;
            if (collectiblesNeeded <= collectiblesCollected)
            {
                if (this.direction.Equals("Vertical-Down"))
                {
                    //platformBody.Position = new Vector2(platformBody.Position.X, platformBody.Position.Y+1);

                    platformBody.linearVelocity = (new Vector2(0, 50));

                    //If chegou a posicao final Then inverte tudo
                    if (repeatMovement && System.Math.Abs(endPosition.Y - platformBody.Position.Y) < 10)
                    {
                        this.direction = "Vertical-Up";

                        Vector2 switchAux = new Vector2(startPosition.X, startPosition.Y);
                        startPosition = new Vector2(endPosition.X, endPosition.Y);
                        endPosition = new Vector2(switchAux.X, switchAux.Y);
                    }
                }

                if (this.direction.Equals("Vertical-Up"))
                {
                    // platformBody.Position = new Vector2(platformBody.Position.X, platformBody.Position.Y-1);
                    platformBody.linearVelocity = (new Vector2(0, -50));
                    //If chegou a posicao final Then inverte tudo
                    if (repeatMovement && System.Math.Abs(endPosition.Y - platformBody.Position.Y) < 10)
                    {
                        this.direction = "Vertical-Down";

                        Vector2 switchAux = new Vector2(startPosition.X, startPosition.Y);
                        startPosition = new Vector2(endPosition.X, endPosition.Y);
                        endPosition = new Vector2(switchAux.X, switchAux.Y);
                    }
                }

                if (this.direction.Equals("Horizontal-Left"))
                {
                    // platformBody.Position = new Vector2(platformBody.Position.X, platformBody.Position.Y-1);
                    platformBody.linearVelocity = (new Vector2(-50, 0));
                    //If chegou a posicao final Then inverte tudo
                    if (repeatMovement && System.Math.Abs(endPosition.X - platformBody.Position.X) < 10)
                    {
                        this.direction = "Horizontal-Right";

                        Vector2 switchAux = new Vector2(startPosition.X, startPosition.Y);
                        startPosition = new Vector2(endPosition.X, endPosition.Y);
                        endPosition = new Vector2(switchAux.X, switchAux.Y);
                    }
                }

                if (this.direction.Equals("Horizontal-Right"))
                {
                    // platformBody.Position = new Vector2(platformBody.Position.X, platformBody.Position.Y-1);
                    platformBody.linearVelocity = (new Vector2(50, 0));
                    //If chegou a posicao final Then inverte tudo
                    if (repeatMovement && System.Math.Abs(endPosition.X - platformBody.Position.X) < 10)
                    {
                        this.direction = "Horizontal-Left";

                        Vector2 switchAux = new Vector2(startPosition.X, startPosition.Y);
                        startPosition = new Vector2(endPosition.X, endPosition.Y);
                        endPosition = new Vector2(switchAux.X, switchAux.Y);
                    }
                }
            }
        }

        public void Load(PhysicsSimulator physicsSimulator, SpriteFont collSpriteFont)
        {
            this.collSpriteFont = collSpriteFont;

            //use the body factory to create the physics body
            platformBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1000000);
            //platformBody.IsStatic = true;
            platformBody.IgnoreGravity = true;

            platformBody.Position = position;

            platformGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, platformBody, width, height);
            platformGeom.CollisionGroup = collisionGroup;
            platformGeom.FrictionCoefficient = .2f;
            platformGeom.RestitutionCoefficient = 0f;
            if (color == GameColors.GREEN_ELEVATOR_COLOR)
            {
                platformGeom.Tag = "LightGreenPlatform";
            }
            else if (color == GameColors.YELLOW_ELEVATOR_COLOR)
            {
                platformGeom.Tag = "GoldPlatform";
            }

            this.platformGeom.Collision += HandleCollision;
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.DrawRectangle(new Vector2(platformGeom.Position.X - width / 2, platformGeom.Position.Y - height / 2), width, height, color, 2, borderColor, platformGeom.Rotation);
            if (collectiblesNeeded - collectiblesCollected > 0)
            {
                Vector2 collPosition = new Vector2(this.platformGeom.Position.X - COLL_ICON_WIDTH / 2, this.platformGeom.Position.Y - COLL_ICON_HEIGHT);

                instructionsBatch.DrawRectangle(collPosition, COLL_ICON_WIDTH, COLL_ICON_HEIGHT, COLL_ICON_COLOR, 1, GameColors.COLLECTIBLE_BORDER_COLOR, (float)(platformGeom.Rotation + (Math.PI / 4)));
                instructionsBatch.DrawString(collSpriteFont, "     x " + (this.collectiblesNeeded - this.collectiblesCollected), collPosition, 1, Color.Black, platformGeom.Rotation);
            }
        }

        public XmlNode ToXml(XmlDocument doc)
        {
            XmlElement node;
            node = doc.CreateElement("Elevator");
            node.SetAttribute("width", this.width.ToString());
            node.SetAttribute("height", this.height.ToString());
            node.SetAttribute("startX", ((this.startPosition.X * 2 - width) / 2).ToString());
            node.SetAttribute("startY", ((this.startPosition.Y * 2 - height) / 2).ToString());
            node.SetAttribute("endX", ((this.endPosition.X * 2 - width) / 2).ToString());
            node.SetAttribute("endY", ((this.endPosition.Y * 2 - height) / 2).ToString());
            node.SetAttribute("collNeeded", this.collectiblesNeeded.ToString());
            node.SetAttribute("repeatMov", this.repeatMovement.ToString());
            node.SetAttribute("direction", this.direction);

            return node; 
        }
    }
}