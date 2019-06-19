using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System.Xml;

namespace GeometryFriends.Levels.Shared
{
    internal class CircleCharacter : GeoShape
    {
        private int radius;
        public int Radius
        {
            get
            {
                return this.radius;
            }
        }

        public bool DebugDraw { get; set; }
        public SpriteFont debugFont { get; set; }

        Vector2 centroidDebug;
        Vector2[] verticesDebug;

        private int mass;
        public bool isBig;
        public bool isSmall;
        protected double jumpingDelayInterval;
        public bool collisionState;
        PhysicsSimulator physicsSimulator;

        public const int BORDER_THICKNESS = 2;
        public const int BORDER_THICKNESS_LITTLE = 1;
        public const int INITAL_RADIUS = 40;
        public const int LITTLE_CIRCLE_RADIUS = 10;
        public const int LITTLE_CIRCLE_Y_SHIFT = 30;
        public const int MAX_RADIUS = 60;
        public const int MIN_RADIUS = 40;
        public const int GROWTHFACTOR = 1;
        public const int INITAL_MASS = 3;
        const int NUMBER_OF_EDGES = 50;
        public const int JUMP_VELOCITY = 17500;
        public const float RESTITUTION_COEFFICIENT = 0.5f;
        public const float FRICTION_COEFFICIENT = .8f;//.2f;
        const int COLLISION_GROUP = 0;
        public const int TORQUE_AMOUNT = 25000; //20000;
        public const int MAX_ANGULAR_VELOCITY = 5;//25;
        public const int JUMPING_DELAY_MAX = 200;
        public const string CIRCLE_CHARACTER_ID = "Ball";

        Enums.CollisionCategories collisionCategory = Enums.CollisionCategories.All;
        Enums.CollisionCategories collidesWith = Enums.CollisionCategories.All;

        // My FLAGS PHIL
        bool detect_max_growth = false;

        public enum Spin
        {
            Both,
            Left,
            Right,
            None
        }
        protected Spin spin = Spin.None;
        public Spin CurrentSpin { 
            get { 
                return spin; 
            }
            set
            {
                spin = value;
            }
        }

        private CircleCharacter() { }

        public CircleCharacter(Vector2 position)
        {
            this.DebugDraw = false;
            this.initialPosition = position;
            this.position = position;
            this.isBig = false;
     
            this.jumpingDelayInterval = JUMPING_DELAY_MAX;
        }

        /// <summary>
        /// Enable the creation of an identical circle character that can be used to coordinate paralel simulations (e.g. action prediction)
        /// </summary>
        /// <param name="clonedBody">The physics body of the cloned circle.</param>
        /// <param name="clonedGeom">The geometrical representation of the cloned circle.</param>
        /// <returns>The cloned circle character.</returns>
        internal CircleCharacter Clone(PhysicsSimulator clonedSimulator, Body clonedBody, Geom clonedGeom)
        {
            CircleCharacter clone = new CircleCharacter();
            clone.position = this.position;
            clone.initialPosition = this.initialPosition;
            clone.body = clonedBody;
            clone.geom = clonedGeom;
            clone.radius = this.radius;
            clone.DebugDraw = this.DebugDraw;
            clone.debugFont = this.debugFont;
            clone.centroidDebug = this.centroidDebug;
            clone.verticesDebug = this.verticesDebug;
            clone.mass = this.mass;
            clone.isBig = this.isBig;
            clone.isSmall = this.isSmall;
            clone.jumpingDelayInterval = this.jumpingDelayInterval;
            clone.collisionState = this.collisionState;
            clone.physicsSimulator = clonedSimulator;
            clone.spin = this.spin;

            //properly bind the cloned geometry
            clone.geom.Collision += clone.HandleCollision;

            return clone;
        }

        public Vector2 RotationPoint { 
            get {
                return new Vector2(X, Y);
            } 
        }

        public void SetCollisionState(bool collisionState)
        {
            this.collisionState = collisionState;
        }

        public bool GetCollisionState()
        {
            return this.collisionState;
        }

        public Body Body
        {
            get { return body; }
        }

        public double JumpingDelayInterval
        {
            get { return jumpingDelayInterval; }
            set { jumpingDelayInterval = value; }
        }

        public void ApplyForce(Vector2 force)
        {
            body.ApplyForce(force);
        }
     
        public Enums.CollisionCategories CollisionCategory
        {
            get { return collisionCategory; }
            set { collisionCategory = value; }
        }

        public Enums.CollisionCategories CollidesWith
        {
            get { return collidesWith; }
            set { collidesWith = value; }
        }

        private bool HandleCollision(Geom g1, Geom g2, ContactList contactList)
        {
            if ((string)g2.Tag == "LightGreenPlatform")
            {
                return false;
            }

            Contact[] contacts = contactList.ToArray();

            int side_collisions = 0;

            if ((string)g2.Tag != TriangleCollectible.COLLECTIBLE_ID)
            {
                for (int i = 0; i < contacts.Length; i++)
                {
                    if (contacts[i].Position.Y > (this.geom.WorldVertices.GetCentroid().Y + this.radius / 4))
                    {
                        this.collisionState = true;
                        jumpingDelayInterval = 0;
                        return true;
                    }
                    if (contacts[i].Position.X < (this.geom.WorldVertices.GetCentroid().X + this.radius / 4))
                    {
                        side_collisions++;
                    }
                    if (contacts[i].Position.X > (this.geom.WorldVertices.GetCentroid().X + this.radius / 4))
                    {
                        side_collisions++;
                    }
                }
                if (side_collisions >= 2)
                    detect_max_growth = true;
            }
            return true;
        }

        private bool PredictionHandleCollision(Geom g1, Geom g2, ContactList contactList)
        {
            if ((string)g2.Tag == "LightGreenPlatform")
            {
                return false;
            }

            Contact[] contacts = contactList.ToArray();

            if ((string)g2.Tag == TriangleCollectible.COLLECTIBLE_ID)
            {
                return false;
            }
            return true;
        }


        public void Debug()
        {
            verticesDebug = this.geom.WorldVertices.ToArray();
            centroidDebug = this.geom.WorldVertices.GetCentroid();
        }

        private void CreateNewCircleBodyAndGeometry(int radius, int mass, Vector2 position, Vector2 linearVelocity, float angularVelocity )
        {
            if (this.geom != null)
            {
                this.geom.Dispose();
            }
            if (this.body != null)
            {
                this.body.Dispose();
            }

            this.initialPosition = position;

            //BODY
            this.body = BodyFactory.Instance.CreateCircleBody(this.physicsSimulator, radius, mass);
            this.body.Position = position;
            this.body.linearVelocity = linearVelocity;
            this.body.AngularVelocity = angularVelocity;
            
            //GEOMETRY
            this.geom = GeomFactory.Instance.CreateCircleGeom(this.physicsSimulator, body, radius, NUMBER_OF_EDGES);
            this.geom.RestitutionCoefficient = RESTITUTION_COEFFICIENT;
            this.geom.FrictionCoefficient = FRICTION_COEFFICIENT;
            this.geom.CollisionGroup = COLLISION_GROUP;
            this.geom.CollisionCategories = collisionCategory;
            this.geom.CollidesWith = collidesWith;
            this.geom.Tag = CIRCLE_CHARACTER_ID;
            this.geom.Collision += HandleCollision;
        }

        public override void Load(PhysicsSimulator physicsSimulator)
        {
            this.physicsSimulator = physicsSimulator;
            this.collisionState = false;

            this.radius = INITAL_RADIUS;
            this.mass = INITAL_MASS;

            //texture = DrawingHelper.CreateCircleTexture(graphicsDevice, INITAL_RADIUS, Color.Gold, Color.Black);
            //littleCircle = DrawingHelper.CreateCircleTexture(graphicsDevice, 10, Color.Black);
            //origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            CreateNewCircleBodyAndGeometry(INITAL_RADIUS, INITAL_MASS, position, new Vector2(0f, 0f), 0f);
        }

        public void SpinLeft()
        {
            if (spin == Spin.Right || spin == Spin.Both)
                spin = Spin.Both;
            else
                spin = Spin.Left;
            
            if (body.AngularVelocity > (-MAX_ANGULAR_VELOCITY))
            {
                 body.ApplyTorque(-TORQUE_AMOUNT);
            }
        }

        public void SpinRight()
        {
            if (spin == Spin.Left || spin == Spin.Both)
                spin = Spin.Both;
            else
                spin = Spin.Right;

            if (body.AngularVelocity < MAX_ANGULAR_VELOCITY)
            {
                body.ApplyTorque(TORQUE_AMOUNT);
            }
        }
     
        public void Jump(float intensity)
        {
            if (GetCollisionState() == true)
            {                
                body.linearVelocity.Y = -JUMP_VELOCITY / this.radius * FarseerGames.FarseerPhysics.Mathematics.MathHelper.Min((intensity / 2), 1);
            }
            else if (jumpingDelayInterval < JUMPING_DELAY_MAX)
            {
                body.linearVelocity.Y = -JUMP_VELOCITY / this.radius;
                jumpingDelayInterval = JUMPING_DELAY_MAX * FarseerGames.FarseerPhysics.Mathematics.MathHelper.Min((intensity / 2), 2);
            }
        }

        public void Grow()
        {
            if ((this.radius + GROWTHFACTOR) <= MAX_RADIUS && !detect_max_growth)
            {
                this.isBig = false;
                this.radius += GROWTHFACTOR;
                this.mass += ((int)(GROWTHFACTOR / 1));

                CreateNewCircleBodyAndGeometry(this.radius, this.mass, geom.Position, body.linearVelocity, body.AngularVelocity);
            }
            else
            {
                this.isBig = true;
            }
        }

        public void Shrink()
        {
            if ((this.radius - GROWTHFACTOR) >= MIN_RADIUS)
            {
                detect_max_growth = false;
                this.radius -= GROWTHFACTOR;
                this.mass -= ((int)(GROWTHFACTOR / 1));

                //texture = DrawingHelper.CreateCircleTexture(graphicsDevice, this.radius, Color.Gold, Color.Black);
                //origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                this.isSmall = false;
                CreateNewCircleBodyAndGeometry(this.radius, this.mass, geom.Position, body.linearVelocity, body.AngularVelocity);
            }
            else
            {
                detect_max_growth = false;
                this.isSmall = true;
            }
        }

        public override void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            if (geom.Position.X > 0 &&
                geom.Position.Y > 0 &&
                geom.Position.X < instructionsBatch.DWidth &&
                geom.Position.Y < instructionsBatch.DHeight)
            {
                instructionsBatch.DrawCircle(new Vector2(geom.Position.X - radius, geom.Position.Y - radius), radius, geom.Rotation, RotationPoint, GameColors.CIRCLE_FILL_COLOR, GameColors.CIRCLE_BORDER_COLOR, BORDER_THICKNESS);
                instructionsBatch.DrawCircle(new Vector2(geom.Position.X - radius + LITTLE_CIRCLE_RADIUS, geom.Position.Y - radius + LITTLE_CIRCLE_Y_SHIFT), LITTLE_CIRCLE_RADIUS, geom.Rotation, RotationPoint, GameColors.CIRCLE_LITTLE_FILL_COLOR, GameColors.CIRCLE_LITTLE_BORDER_COLOR, BORDER_THICKNESS_LITTLE);
            }
            else
            {
                this.ResetCirclePosition();
            }

            //debug shape composition and position
            if (DebugDraw)
            {
                Debug();       
            }
        }

        public void SetPosition(Vector2 position)
        {
            this.body.Position = position;
        }

        public void ResetCirclePosition()
        {
            this.body.Position = base.ResetPosition();
        }
       
        public override XmlNode ToXml(XmlDocument doc)
        {
            XmlElement node;
            node = doc.CreateElement("BallStartingPosition");
            node.SetAttribute("X", this.body.Position.X.ToString());
            node.SetAttribute("Y", this.body.Position.Y.ToString());
            return node;
        }
    }
}
