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
    internal class RectangleCharacter : GeoShape
    {
        PhysicsSimulator physicsSimulator;
        float width;
        public override float Width
        {
            get
            {
                //SwapWidthAndHeightAccordingToOrientation();
                return this.width;
            }
        }

        float height;
        public override float Height
        {
            get
            {
                //SwapWidthAndHeightAccordingToOrientation();
                return this.height;
            }
        }
        bool canStretch;
        float changeOrientationAngle;
        float straightErrorAngle;

        public bool DebugDraw { get; set; }
        public SpriteFont debugFont { get; set; }

        Vector2 vertice1Debug;
        Vector2 vertice2Debug;
        Vector2 vertice3Debug;
        Vector2 vertice4Debug;

        public const float FORCE_AMOUNT = 1000;
        public const int BORDER_THICKNESS = 2;
        public const int INITIAL_WIDTH = 100;
        public const int MINIMUM_WIDTH = INITIAL_WIDTH / 2;
        public const int MAXIMUM_WIDTH = INITIAL_WIDTH * 2;
        public const int INITIAL_HEIGHT = 100;
        public const int MINIMUM_HEIGHT = INITIAL_HEIGHT / 2;
        public const int MAXIMUM_HEIGHT = INITIAL_HEIGHT * 2;
        public const int GROWTH_FACTOR = 3;
        public const int MASS = 5;
        public const float RESTITUTION_COEFFICIENT = 0.5f;
        public const float FRICTION_COEFFICIENT = .2f;
        public const int COLLISION_GROUP = 0;
        public const string RECTANGLE_CHARACTER_ID = "Rectangle";

        Enums.CollisionCategories collisionCategory = Enums.CollisionCategories.All;
        Enums.CollisionCategories collidesWith = Enums.CollisionCategories.All;

        //control for jitter auxiliary variables
        Vector2 drawPosition = new Vector2(0, 0);
        float drawRotation = 0;

        public enum Slide
        {
            Both,
            Left,
            Right,
            None
        }
        protected Slide slide = Slide.None;
        public Slide CurrentSlide
        {
            get
            {
                return slide;
            }
            set
            {
                slide = value;
            }
        }

        private RectangleCharacter() { }

        public RectangleCharacter(Vector2 position)
        {
            this.DebugDraw = false;
            this.position = position;
            this.initialPosition = new Vector2(position.X,position.Y);
            this.canStretch = true;
            SetChangeOrientationAngleDegrees(5);
            SetStraightErrorAngleDegrees(4);
        }

        /// <summary>
        /// Enable the creation of an identical rectangle character that can be used to coordinate paralel simulations (e.g. action prediction)
        /// </summary>
        /// <param name="clonedBody">The physics body of the cloned rectangle.</param>
        /// <param name="clonedGeom">The geometrical representation of the cloned rectangle.</param>
        /// <returns>The cloned rectangle character.</returns>
        internal RectangleCharacter Clone(PhysicsSimulator clonedSimulator, Body clonedBody, Geom clonedGeom)
        {
            RectangleCharacter clone = new RectangleCharacter();
            clone.position = this.position;
            clone.initialPosition = this.initialPosition;
            clone.body = clonedBody;
            clone.geom = clonedGeom;
            clone.physicsSimulator = clonedSimulator;
            clone.DebugDraw = this.DebugDraw;
            clone.debugFont = this.debugFont;
            clone.width = this.width;
            clone.height = this.height;
            clone.canStretch = this.canStretch;
            clone.changeOrientationAngle = this.changeOrientationAngle;
            clone.straightErrorAngle = this.straightErrorAngle;
            clone.vertice1Debug = this.vertice1Debug;
            clone.vertice2Debug = this.vertice2Debug;
            clone.vertice3Debug = this.vertice3Debug;
            clone.vertice4Debug = this.vertice4Debug;
            clone.collisionCategory = this.collisionCategory;
            clone.collidesWith = this.collidesWith;
            clone.drawPosition = this.drawPosition;
            clone.drawRotation = this.drawRotation;
            clone.slide = this.slide;

            //properly bind the cloned geometry
            clone.geom.Collision += clone.HandleCollision;

            return clone;
        }

        public bool GetCanStretch()
        {
            return canStretch;
        }

        public void SetCanStretch(bool flag){
            this.canStretch = flag;
        }

        private bool HandleCollision(Geom g1, Geom g2, ContactList contactList)
        {
            if (g2.Tag != null && g2.Tag.Equals("GoldPlatform"))
            {
                return false;
            }

            if ((string)g2.Tag != CircleCharacter.CIRCLE_CHARACTER_ID)
            {
                Contact[] contacts = contactList.ToArray();

                for (int i = 0; i < contacts.Length; i++)
                {
                    if (contacts[i].Position.Y < this.geom.WorldVertices.GetCentroid().Y &&
                        ((contacts[i].Position.X > this.geom.GetRectangleCorner(1).X) ||
                        (contacts[i].Position.X < this.geom.GetRectangleCorner(2).X)))
                    {
                        canStretch = false;                        
                    }
                }
            }

            return true;
        }

        public Body Body
        {
            get { return body; }
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

        private void CreateNewRectangleBodyGeometryTexture(float width, float height, Vector2 position, Vector2 linearVelocity)
        {
            if (this.geom != null)
            {
                this.geom.Dispose();
            }
            if (this.body != null)
            {
                this.body.Dispose();
            }
            //BODY
            body = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, MASS);
            this.body.Position = position;
            this.body.linearVelocity = linearVelocity;

            //GEOMETRY
            geom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, body, width, height);
            this.geom.RestitutionCoefficient = RESTITUTION_COEFFICIENT;
            this.geom.FrictionCoefficient = FRICTION_COEFFICIENT;
            this.geom.CollisionGroup = COLLISION_GROUP;
            this.geom.Tag = RECTANGLE_CHARACTER_ID;
            this.geom.CollisionCategories = collisionCategory;
            this.geom.CollidesWith = collidesWith;
            this.geom.Collision += HandleCollision;
        }

        public void Debug()
        {
            vertice1Debug = this.geom.GetRectangleCorner(1);
            vertice2Debug = this.geom.GetRectangleCorner(2);
            vertice3Debug = this.geom.GetRectangleCorner(3);
            vertice4Debug = this.geom.GetRectangleCorner(4);            
        }

        public override void Load(PhysicsSimulator physicsSimulator)
        {
            this.physicsSimulator = physicsSimulator;

            this.width = INITIAL_WIDTH;
            this.height = INITIAL_HEIGHT;

            CreateNewRectangleBodyGeometryTexture(INITIAL_WIDTH, INITIAL_HEIGHT, position, new Vector2(0f, 0f));
        }

        public override void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            //full resolution drawing
            instructionsBatch.DrawRectangle(new Vector2(geom.Position.X - width / 2, geom.Position.Y - height / 2), (int)width, (int)height, GameColors.RECTANGLE_FILL_COLOR, BORDER_THICKNESS, GameColors.RECTANGLE_BORDER_COLOR, geom.Rotation);

            //debug individual vertices order
            if (DebugDraw)
            {
                Debug();//load vertices position
                //draw vertices string
                instructionsBatch.DrawString(debugFont, "1", vertice1Debug, GameColors.DEBUG_VERTICES);
                instructionsBatch.DrawString(debugFont, "2", vertice2Debug, GameColors.DEBUG_VERTICES);
                instructionsBatch.DrawString(debugFont, "3", vertice3Debug, GameColors.DEBUG_VERTICES);
                instructionsBatch.DrawString(debugFont, "4", vertice4Debug, GameColors.DEBUG_VERTICES);
            }
        }

        public void ResetInitialPosition()
        {
            CreateNewRectangleBodyGeometryTexture(INITIAL_WIDTH, INITIAL_HEIGHT, initialPosition, new Vector2(0f, 0f));
            this.width = INITIAL_WIDTH;
            this.height = INITIAL_HEIGHT;
        }
        
        public void SlideLeft()
        {
            SwapWidthAndHeightAccordingToOrientation();

            if (slide == Slide.Right || slide == Slide.Both)
                slide = Slide.Both;
            else
                slide = Slide.Left;

            Vector2 force = Vector2.Zero;
            force += new Vector2(-FORCE_AMOUNT, 0);
            body.ApplyForce(force);
        }

        public void SlideRight()
        {
            SwapWidthAndHeightAccordingToOrientation();

            if (slide == Slide.Left || slide == Slide.Both)
                slide = Slide.Both;
            else
                slide = Slide.Right;
            
            Vector2 force = Vector2.Zero;
            force += new Vector2(FORCE_AMOUNT, 0);
            body.ApplyForce(force);
        }

        private float CalculateCompensationGrowth(float dimensionToReduceSize, float dimensionToCompensateSize)
        {
            return (GROWTH_FACTOR * dimensionToCompensateSize) / (dimensionToReduceSize - GROWTH_FACTOR);
        }
        
        //this method swaps width and height when the rectangle falls over
        private void SwapWidthAndHeightAccordingToOrientation()
        {
            Vector2 aux = new Vector2(1, 0);
            Vector2 rectangleTopEdge = geom.GetRectangleCorner(1) - geom.GetRectangleCorner(3);

            double angle = Math.Atan2(aux.Y - rectangleTopEdge.Y, aux.X - rectangleTopEdge.X);

            if (Math.Abs(angle) < changeOrientationAngle || Math.Abs(Math.PI - Math.Abs(angle)) < changeOrientationAngle || Math.Abs((Math.PI / 2) + angle) < changeOrientationAngle) // swap width and height            
            {
                float temp = height;
                height = width;
                width = temp;
                //reinstatiate the geometry so that the vertices are correct before any morph can take place
                CreateNewRectangleBodyGeometryTexture(width, height, new Vector2(geom.Position.X, geom.Position.Y), body.linearVelocity);                
            }
        }

        private bool IsStraight()
        {
            Vector2 aux = new Vector2(1, 0);
            Vector2 rectangleLeftEdge = geom.GetRectangleCorner(1) - geom.GetRectangleCorner(2);

            double angle = Math.Atan2(aux.Y - rectangleLeftEdge.Y, aux.X - rectangleLeftEdge.X);
            
            //when checking here should always have the correct vertex orientation
            if (Math.Abs(angle) < straightErrorAngle)
                return true;
            return false;
        }

        public void StretchVerticalUp()
        {
            SwapWidthAndHeightAccordingToOrientation();
            if (this.width - GROWTH_FACTOR > MINIMUM_WIDTH && IsStraight() && canStretch)
            {
                float compensationGrowth = CalculateCompensationGrowth(width, height);
                width -= GROWTH_FACTOR;
                height += compensationGrowth;

                CreateNewRectangleBodyGeometryTexture(width, height, new Vector2(geom.Position.X, geom.Position.Y - compensationGrowth / 2), body.linearVelocity);
            }    
        }

        public void StretchVerticalDown()
        {
            SwapWidthAndHeightAccordingToOrientation();
            if (this.height - GROWTH_FACTOR > MINIMUM_HEIGHT && IsStraight())
            {
                float compensationGrowth = CalculateCompensationGrowth(height, width);
                this.height -= GROWTH_FACTOR;
                this.width += compensationGrowth;

                CreateNewRectangleBodyGeometryTexture(width, height, new Vector2(geom.Position.X, geom.Position.Y + GROWTH_FACTOR), body.linearVelocity);
            }
        }

        public void SetPosition(Vector2 position)
        {
            this.body.Position = position;
        }

        public override XmlNode ToXml(XmlDocument doc)
        {
            XmlElement node;
            node = doc.CreateElement("SquareStartingPosition");
            node.SetAttribute("X", this.body.Position.X.ToString());
            node.SetAttribute("Y", this.body.Position.Y.ToString());
            return node;
        }

        public void SetChangeOrientationAngleRadians(float angleRadians)
        {
            changeOrientationAngle = angleRadians;
        }

        public void SetChangeOrientationAngleDegrees(float angleDegrees)
        {
            changeOrientationAngle = DegreesToRadians(angleDegrees);
        }

        public void SetStraightErrorAngleRadians(float angleRadians)
        {
            straightErrorAngle = angleRadians;
        }

        public void SetStraightErrorAngleDegrees(float angleDegrees)
        {
            straightErrorAngle = DegreesToRadians(angleDegrees);
        }

        private float DegreesToRadians(float degrees)
        {
            return (float)(Math.PI / 180) * degrees;
        }
    }
}
