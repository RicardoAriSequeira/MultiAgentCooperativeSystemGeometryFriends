
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;

namespace GeometryFriends.Levels.Shared
{
    internal class Border
    {
        Vector2[] borderDimensions;
        Color borderColor;
        int borderThickness;

        Body borderBody;
        Geom[] borderGeom;

        int width;
        int height;
        int borderWidth;
        Vector2 position;

        private const float DEFAULT_DRAWING_PRIORITY = -1;

        public Border(int width, int height, int borderWidth, Vector2 position)
        {
            this.width = width;
            this.height = height;
            this.borderWidth = borderWidth;
            this.position = position;

            borderColor = GameColors.LIMITS_COLOR;
            borderThickness = 2;
        }

        public void Load(PhysicsSimulator physicsSimulator)
        {
            borderDimensions = new Vector2[4];

            borderDimensions[0] = new Vector2(borderWidth, height);
            borderDimensions[1] = new Vector2(borderWidth, height);
            borderDimensions[2] = new Vector2(width, borderWidth);
            borderDimensions[3] = new Vector2(width, borderWidth);

            //use the body factory to create the physics body
            borderBody = BodyFactory.Instance.CreateRectangleBody(physicsSimulator, width, height, 1);

            borderBody.IsStatic = true;
            borderBody.Position = position;

            LoadBorderGeom(physicsSimulator);
        }

        public void LoadBorderGeom(PhysicsSimulator physicsSimulator)
        {
            Vector2 geometryOffset = Vector2.Zero;

            borderGeom = new Geom[4];
            //left border
            geometryOffset = new Vector2(-(width * .5f - borderWidth * .5f), 0);
            borderGeom[0] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, borderWidth, height, geometryOffset, 0);
            borderGeom[0].RestitutionCoefficient = .2f;
            borderGeom[0].FrictionCoefficient = .5f;
            borderGeom[0].CollisionGroup = 100;
            borderGeom[0].Tag = "Left";

            //right border (clone left border since geometry is same size)
            geometryOffset = new Vector2(width * .5f - borderWidth * .5f, 0);
            borderGeom[1] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[0], geometryOffset, 0);
            borderGeom[1].Tag = "Right";

            //top border
            geometryOffset = new Vector2(0, -(height * .5f - borderWidth * .5f));
            borderGeom[2] = GeomFactory.Instance.CreateRectangleGeom(physicsSimulator, borderBody, width, borderWidth, geometryOffset, 0);
            borderGeom[2].RestitutionCoefficient = .2f;
            borderGeom[2].FrictionCoefficient = .2f;
            borderGeom[2].CollisionGroup = 100;
            borderGeom[2].CollisonGridCellSize = 20;
            borderGeom[2].ComputeCollisonGrid();
            borderGeom[2].Tag = "Ceiling";

            //bottom border (clone top border since geometry is same size)
            geometryOffset = new Vector2(0, height * .5f - borderWidth * .5f);
            borderGeom[3] = GeomFactory.Instance.CreateGeom(physicsSimulator, borderBody, borderGeom[2], geometryOffset, 0);
            borderGeom[3].Tag = "Bottom";
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            for (int i = 0; i < 4; i++)
            {
                instructionsBatch.DrawRectangle(new Vector2(borderGeom[i].Position.X - borderDimensions[i].X / 2, borderGeom[i].Position.Y - borderDimensions[i].Y / 2), (int)borderDimensions[i].X, (int)borderDimensions[i].Y, borderColor, borderThickness, borderColor, borderGeom[i].Rotation, DEFAULT_DRAWING_PRIORITY);
            }
        }
    }
}
