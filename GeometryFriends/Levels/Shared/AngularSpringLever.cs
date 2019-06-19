
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;

namespace GeometryFriends.Levels.Shared
{
    internal class AngularSpringLever
    {
        int radius;

        int attachPoint = 0; //0=left, 1=top, 2=right,3=bottom
        int rectangleWidth = 100;
        int rectangleHeight = 20;
        Vector2 position;

        float springConstant = 1;
        float dampningConstant = 1;

        Body angleSpringleverBody;
        Geom circleGeom;
        Geom rectangleGeom;

        FixedRevoluteJoint revoluteJoint;
        FixedAngleSpring fixedAngleSpring;

        int collisionGroup = 0;

        public AngularSpringLever()
        {
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        public int AttachPoint
        {
            get { return attachPoint; }
            set { attachPoint = value; }
        }
        public int RectangleWidth
        {
            get { return rectangleWidth; }
            set { rectangleWidth = value; }
        }

        public int RectangleHeight
        {
            get { return rectangleHeight; }
            set { rectangleHeight = value; }
        }

        public float SpringConstant
        {
            get { return springConstant; }
            set { springConstant = value; }
        }

        public float DampningConstant
        {
            get { return dampningConstant; }
            set { dampningConstant = value; }
        }

        public int CollisionGroup
        {
            get { return collisionGroup; }
            set { collisionGroup = value; }
        }

        public Body Body
        {
            get { return angleSpringleverBody; }
        }

        public void Load(IGraphicsDevice graphicsDevice, PhysicsSimulator physicsSimulator)
        {
            int radius;
            if (attachPoint == 0 | attachPoint == 2)
            {
                radius = rectangleHeight;
            }
            else
            {
                radius = rectangleWidth;
            }
            //body is created as rectangle so that it has the moment of inertia closer to the final shape of the object.
            angleSpringleverBody = BodyFactory.Instance.CreateBody(physicsSimulator, 1, BodyFactory.MOIForRectangle(rectangleWidth, rectangleHeight, 1f));

            rectangleGeom = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, angleSpringleverBody, rectangleWidth, rectangleHeight);
            rectangleGeom.FrictionCoefficient = .5f;
            rectangleGeom.CollisionGroup = collisionGroup;
            
            Vector2 offset = Vector2.Zero;
            switch (attachPoint)
            {
                case 0:
                    {
                        offset = new Vector2(-rectangleWidth / 2, 0); //offset to rectangle to left
                        break;
                    }
                case 1:
                    {
                        offset = new Vector2(0, -rectangleHeight / 2); //offset to rectangle to top
                        break;
                    }
                case 2:
                    {
                        offset = new Vector2(rectangleWidth / 2, 0); //offset to rectangle to right
                        break;
                    }
                case 3:
                    {
                        offset = new Vector2(0, rectangleHeight / 2); //offset to rectangle to bottom
                        break;
                    }
            }

            angleSpringleverBody.Position = position - offset;

            circleGeom = GeomFactory.Instance.CreateCircleGeom(physicsSimulator, angleSpringleverBody, radius, 20, offset, 0);
            circleGeom.FrictionCoefficient = .5f;
            circleGeom.CollisionGroup = collisionGroup;
            circleGeom.Tag = "AngleSpring-" + Position.ToString();

            revoluteJoint = JointFactory.Instance.CreateFixedRevoluteJoint(physicsSimulator, angleSpringleverBody, position);
            physicsSimulator.Add(revoluteJoint);
            fixedAngleSpring = ControllerFactory.Instance.CreateFixedAngleSpring(physicsSimulator, angleSpringleverBody, springConstant, dampningConstant);
            //fixedAngleSpring.MaxTorque = 200000;
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.DrawRectangle(new Vector2(rectangleGeom.Position.X - rectangleWidth / 2, rectangleGeom.Position.Y - rectangleHeight / 2), rectangleWidth, rectangleHeight, GameColors.SPRING_FILL_COLOR, 1, GameColors.SPRING_BORDER_COLOR, rectangleGeom.Rotation);
            instructionsBatch.DrawCircle(new Vector2(circleGeom.Position.X - radius, circleGeom.Position.Y - radius), radius, circleGeom.Rotation, new Vector2(circleGeom.Position.X, circleGeom.Position.Y), GameColors.SPRING_CIRCLE_FILL_COLOR, GameColors.SPRING_CIRCLE_BORDER_COLOR, 1);
        }
    }
}
