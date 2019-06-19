using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.DrawingSystem;
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;

namespace GeometryFriends
{
    class PhysicsSimulatorView
    {
        private PhysicsSimulator physicsSimulator;
        private SpriteFont spriteFont;

        //aabb
        private Color aabbColor = new Color(0, 0, 0, 150);// Color.Gainsboro;
        private int aabbLineThickness = 1;
        private LineBrush aabbLineBrush;
        private bool enableAABBView = true;

        //vertices
        private int verticeRadius = 3;
        private Color verticeColor = new Color(0, 50, 0, 150);
        private CircleBrush verticeCircleBrush;
        private bool enableVerticeView = true;

        //edges
        private int edgeLineThickness = 1;
        private Color edgeColor = new Color(0, 0, 0, 150);
        private LineBrush edgeLineBrush;
        private bool enableEdgeView = false;

        //grid
        private int gridRadius = 1;
        private Color gridColor = new Color(0, 0, 0, 150);
        private CircleBrush gridCircleBrush;
        private bool enableGridView = false;

        //coordinate axis
        private Color coordinateAxisColor = new Color(0, 0, 0, 150);
        private int coordinateAxisLineThickness = 1;
        private int coordinateAxisLineLength = 20;
        private LineBrush coordinateAxisLineBrush;
        private bool enableCoordinateAxisView = true;

        //contacts
        private Color contactColor = new Color(255, 0, 0, 150);
        private int contactRadius = 4;
        private CircleBrush contactCircleBrush;
        private bool enableContactView = true;

        //springs
        private Color springLineColor = new Color(0, 0, 0, 150);
        private int springLineThickness = 1;
        private LineBrush springLineBrush;
        private CircleBrush springCircleBrush;
        private bool enableSpringView = true;

        //revolute joint
        private Color revoluteJointColor = new Color(0, 0, 0, 200);
        private int revoluteJointLineThickness = 1;
        private LineBrush revoluteJointLineBrush;
        private RectangleBrush revoluteJointRectangleBrush;
        private bool enableRevoluteJointView = true;

        //pin joint
        private Color pinJointColor = new Color(0, 0, 0, 200);
        private int pinJointLineThickness = 1;
        private LineBrush pinJointLineBrush;
        private RectangleBrush pinJointRectangleBrush;
        private bool enablePinJointView = true;

        //slider joint
        private Color sliderJointColor = new Color(0, 0, 0, 200);
        private int sliderJointLineThickness = 1;
        private LineBrush sliderJointLineBrush;
        private RectangleBrush sliderJointRectangleBrush;
        private bool enableSliderJointView = true;

        //performance panel
        private Color performancePanelColor = new Color(0, 0, 0, 155);
        private Color performancePanelTextColor = new Color(0, 0, 0, 255);
        private bool enablePerformancePanelView = true;
        private Vector2 performancePanelPosition = new Vector2(100, 110);
        private int performancePanelWidth = 280;
        private int performancePanelHeight = 120;
        private string updateTotal = "Update Total: {0}";
        private string cleanUp = "Clean Up: {0}";
        private string broadPhaseCollision = "Broad Phase Collsion: {0}";
        private string narrowPhaseCollision = "Narrow Phase Collsion: {0}";
        private string applyForces = "Apply Forces: {0}";
        private string applyImpulses = "Apply Impulses: {0}";
        private string updatePosition = "Update Positions: {0}";

        public PhysicsSimulatorView(PhysicsSimulator physicsSimulator)
        {
            this.physicsSimulator = physicsSimulator;
        }

        //aabb
        public Color AABBColor
        {
            get { return aabbColor; }
            set { aabbColor = value; }
        }

        public int AABBLineThickness
        {
            get { return aabbLineThickness; }
            set { aabbLineThickness = value; }
        }

        public bool EnableAABBView
        {
            get { return enableAABBView; }
            set { enableAABBView = value; }
        }

        //vertices
        public int VerticeRadius
        {
            get { return verticeRadius; }
            set { verticeRadius = value; }
        }

        public Color VerticeColor
        {
            get { return verticeColor; }
            set { verticeColor = value; }
        }

        public bool EnableVerticeView
        {
            get { return enableVerticeView; }
            set { enableVerticeView = value; }
        }

        //edges
        public int EdgeLineThickness
        {
            get { return edgeLineThickness; }
            set { edgeLineThickness = value; }
        }

        public Color EdgeColor
        {
            get { return edgeColor; }
            set { edgeColor = value; }
        }

        public bool EnableEdgeView
        {
            get { return enableEdgeView; }
            set { enableEdgeView = value; }
        }

        //grid
        public int GridRadius
        {
            get { return gridRadius; }
            set { gridRadius = value; }
        }

        public Color GridColor
        {
            get { return gridColor; }
            set { gridColor = value; }
        }

        public bool EnableGridView
        {
            get { return enableGridView; }
            set { enableGridView = value; }
        }

        //coordinate axis
        public int CoordinateAxisLineThickness
        {
            get { return coordinateAxisLineThickness; }
            set { coordinateAxisLineThickness = value; }
        }

        public Color CoordinateAxisColor
        {
            get { return coordinateAxisColor; }
            set { coordinateAxisColor = value; }
        }

        public int CoordinateAxisLineLength
        {
            get { return coordinateAxisLineLength; }
            set { coordinateAxisLineLength = value; }
        }

        public bool EnableCoordinateAxisView
        {
            get { return enableCoordinateAxisView; }
            set { enableCoordinateAxisView = value; }
        }

        //contacts
        public int ContactRadius
        {
            get { return contactRadius; }
            set { contactRadius = value; }
        }

        public Color ContactColor
        {
            get { return contactColor; }
            set { contactColor = value; }
        }

        public bool EnableContactView
        {
            get { return enableContactView; }
            set { enableContactView = value; }
        }

        //springs
        public Color SpringLineColor
        {
            get { return springLineColor; }
            set { springLineColor = value; }
        }

        public int SpringLineThickness
        {
            get { return springLineThickness; }
            set { springLineThickness = value; }
        }

        public bool EnableSpingView
        {
            get { return enableSpringView; }
            set { enableSpringView = value; }
        }

        //revolute joint
        public Color RevoluteJointLineColor
        {
            get { return revoluteJointColor; }
            set { revoluteJointColor = value; }
        }

        public int RevoluteJointLineThickness
        {
            get { return revoluteJointLineThickness; }
            set { revoluteJointLineThickness = value; }
        }

        public bool EnableRevoluteJointView
        {
            get { return enableRevoluteJointView; }
            set { enableRevoluteJointView = value; }
        }

        //pin joint
        public Color PinJointLineColor
        {
            get { return pinJointColor; }
            set { pinJointColor = value; }
        }

        public int PinJointLineThickness
        {
            get { return pinJointLineThickness; }
            set { pinJointLineThickness = value; }
        }

        public bool EnablePinJointView
        {
            get { return enablePinJointView; }
            set { enablePinJointView = value; }
        }

        //slider joint
        public Color SliderJointLineColor
        {
            get { return sliderJointColor; }
            set { sliderJointColor = value; }
        }

        public int SliderJointLineThickness
        {
            get { return sliderJointLineThickness; }
            set { sliderJointLineThickness = value; }
        }

        public bool EnableSliderJointView
        {
            get { return enableSliderJointView; }
            set { enableSliderJointView = value; }
        }

        //performance panel
        public Color PerformancePanelColor
        {
            get { return performancePanelColor; }
            set { performancePanelColor = value; }
        }

        public Color PerformancePanelTextColor
        {
            get { return performancePanelTextColor; }
            set { performancePanelTextColor = value; }
        }

        public bool EnablePerformancePanelView
        {
            get { return enablePerformancePanelView; }
            set { enablePerformancePanelView = value; }
        }

        public virtual void LoadContent(IGraphicsDevice graphicsDevice, ContentManager content)
        {
            LoadVerticeContent(graphicsDevice);
            LoadEdgeContent(graphicsDevice);
            LoadGridContent(graphicsDevice);
            LoadAABBContent(graphicsDevice);
            LoadCoordinateAxisContent(graphicsDevice);
            LoadContactContent(graphicsDevice);
            LoadPerformancePanelContent(graphicsDevice, content);
            LoadSpringContent(graphicsDevice);
            LoadRevoluteJointContent(graphicsDevice);
            LoadPinJointContent(graphicsDevice);
            LoadSliderJointContent(graphicsDevice);
        }

        public virtual void UnloadContent(IGraphicsDevice graphicsDevice, ContentManager content)
        {
            LoadVerticeContent(graphicsDevice);
            LoadEdgeContent(graphicsDevice);
            LoadGridContent(graphicsDevice);
            LoadAABBContent(graphicsDevice);
            LoadCoordinateAxisContent(graphicsDevice);
            LoadContactContent(graphicsDevice);
            LoadPerformancePanelContent(graphicsDevice, content);
            LoadSpringContent(graphicsDevice);
            LoadRevoluteJointContent(graphicsDevice);
            LoadPinJointContent(graphicsDevice);
            LoadSliderJointContent(graphicsDevice);
        }

        private void LoadPerformancePanelContent(IGraphicsDevice graphicsDevice, ContentManager content)
        {
            spriteFont = content.Load<SpriteFont>(@"Content\Fonts\diagnosticFont.spritefont");
        }

        private void LoadContactContent(IGraphicsDevice graphicsDevice)
        {
            contactCircleBrush = new CircleBrush(contactRadius, contactColor, contactColor);
        }

        private void LoadVerticeContent(IGraphicsDevice graphicsDevice)
        {
            verticeCircleBrush = new CircleBrush(verticeRadius, verticeColor, verticeColor);
        }

        private void LoadEdgeContent(IGraphicsDevice graphicsDevice)
        {
            edgeLineBrush = new LineBrush(edgeLineThickness, edgeColor);
        }

        private void LoadGridContent(IGraphicsDevice graphicsDevice)
        {
            gridCircleBrush = new CircleBrush(gridRadius, gridColor, gridColor);
        }

        private void LoadAABBContent(IGraphicsDevice graphicsDevice)
        {
            //load aabb texture
            aabbLineBrush = new LineBrush(aabbLineThickness, aabbColor);
        }

        private void LoadCoordinateAxisContent(IGraphicsDevice graphicsDevice)
        {
            coordinateAxisLineBrush = new LineBrush(coordinateAxisLineThickness, coordinateAxisColor);
        }

        private void LoadSpringContent(IGraphicsDevice graphicsDevice)
        {
            springLineBrush = new LineBrush(springLineThickness, springLineColor);
            springCircleBrush = new CircleBrush(2, springLineColor, springLineColor);
        }

        private void LoadRevoluteJointContent(IGraphicsDevice graphicsDevice)
        {
            revoluteJointLineBrush = new LineBrush(revoluteJointLineThickness, revoluteJointColor);
            revoluteJointRectangleBrush = new RectangleBrush(10, 10, revoluteJointColor, revoluteJointColor);
        }

        private void LoadPinJointContent(IGraphicsDevice graphicsDevice)
        {
            pinJointLineBrush = new LineBrush(pinJointLineThickness, pinJointColor);
            pinJointRectangleBrush = new RectangleBrush(10, 10, pinJointColor, pinJointColor);
        }

        private void LoadSliderJointContent(IGraphicsDevice graphicsDevice)
        {
            sliderJointLineBrush = new LineBrush(sliderJointLineThickness, sliderJointColor);
            sliderJointRectangleBrush = new RectangleBrush(10, 10, sliderJointColor, sliderJointColor);
        }

        public void Draw(DrawingInstructionsBatch instructionsBatch)
        {
            if (enableVerticeView || enableEdgeView) { DrawVerticesAndEdges(instructionsBatch); }
            if (enableGridView) { DrawGrid(instructionsBatch); }
            if (enableAABBView) { DrawAABB(instructionsBatch); }
            if (enableCoordinateAxisView) { DrawCoordinateAxis(instructionsBatch); }
            if (enableContactView) { DrawContacts(instructionsBatch); }
            if (enablePerformancePanelView) { DrawPerformancePanel(instructionsBatch); }
            if (EnableSpingView) { DrawSprings(instructionsBatch); }
            if (EnableRevoluteJointView) { DrawRevoluteJoints(instructionsBatch); }
            if (EnablePinJointView) { DrawPinJoints(instructionsBatch); }
            if (EnableSliderJointView) { DrawSliderJoints(instructionsBatch); }
        }

        private void DrawPerformancePanel(DrawingInstructionsBatch instructionsBatch)
        {
            instructionsBatch.DrawRectangle(performancePanelPosition, performancePanelWidth, performancePanelHeight, performancePanelColor);

            instructionsBatch.DrawString(spriteFont, String.Format(updateTotal, physicsSimulator.UpdateTime.ToString("0.00")), new Vector2(110, 110), Color.White);
            instructionsBatch.DrawString(spriteFont, String.Format(cleanUp, physicsSimulator.CleanUpTime.ToString("0.00")), new Vector2(120, 125), Color.White);
            instructionsBatch.DrawString(spriteFont, String.Format(broadPhaseCollision, physicsSimulator.BroadPhaseCollisionTime.ToString("0.00")), new Vector2(120, 140), Color.White);
            instructionsBatch.DrawString(spriteFont, String.Format(narrowPhaseCollision, physicsSimulator.NarrowPhaseCollisionTime.ToString("0.00")), new Vector2(120, 155), Color.White);
            instructionsBatch.DrawString(spriteFont, String.Format(applyForces, physicsSimulator.ApplyForcesTime.ToString("0.00")), new Vector2(120, 170), Color.White);
            instructionsBatch.DrawString(spriteFont, String.Format(applyImpulses, physicsSimulator.ApplyImpulsesTime.ToString("0.00")), new Vector2(120, 185), Color.White);
            instructionsBatch.DrawString(spriteFont, String.Format(updatePosition, physicsSimulator.UpdatePositionsTime.ToString("0.00")), new Vector2(120, 200), Color.White);
            //spriteBatch.DrawString(spriteFont, String.Format("Broadphase Pairs: {0}",this.physicsSimulator.sweepAndPrune.collisionPairs.Keys.Count), new Vector2(120, 215), Color.White);
        }

        private void DrawContacts(DrawingInstructionsBatch instructionsBatch)
        {
            //draw contact textures
            for (int i = 0; i < physicsSimulator.ArbiterList.Count; i++)
            {
                for (int j = 0; j < physicsSimulator.ArbiterList[i].ContactList.Count; j++)
                {
                    contactCircleBrush.Draw(instructionsBatch, physicsSimulator.ArbiterList[i].ContactList[j].Position);
                }
            }
        }

        private void DrawVerticesAndEdges(DrawingInstructionsBatch instructionsBatch)
        {
            //draw vertice texture
            int verticeCount;
            for (int i = 0; i < physicsSimulator.GeomList.Count; i++)
            {
                verticeCount = physicsSimulator.GeomList[i].LocalVertices.Count;
                for (int j = 0; j < verticeCount; j++)
                {
                    if (enableEdgeView)
                    {
                        if (j < verticeCount - 1)
                        {
                            edgeLineBrush.Draw(instructionsBatch, physicsSimulator.GeomList[i].WorldVertices[j], physicsSimulator.GeomList[i].WorldVertices[j + 1]);
                        }
                        else
                        {
                            edgeLineBrush.Draw(instructionsBatch, physicsSimulator.GeomList[i].WorldVertices[j], physicsSimulator.GeomList[i].WorldVertices[0]);
                        }
                    }
                    if (enableVerticeView)
                    {
                        verticeCircleBrush.Draw(instructionsBatch, physicsSimulator.GeomList[i].WorldVertices[j]);
                    }
                }
            }
        }

        private void DrawAABB(DrawingInstructionsBatch instructionsBatch)
        {
            //draw aabb
            Vector2 min;
            Vector2 max;

            Vector2 topRight;
            Vector2 bottomLeft;

            for (int i = 0; i < physicsSimulator.GeomList.Count; i++)
            {
                min = physicsSimulator.GeomList[i].AABB.Min;
                max = physicsSimulator.GeomList[i].AABB.Max;

                topRight = new Vector2(max.X, min.Y);
                bottomLeft = new Vector2(min.X, max.Y);
                aabbLineBrush.Draw(instructionsBatch, min, topRight);
                aabbLineBrush.Draw(instructionsBatch, topRight, max);
                aabbLineBrush.Draw(instructionsBatch, max, bottomLeft);
                aabbLineBrush.Draw(instructionsBatch, bottomLeft, min);
            }
        }

        private void DrawGrid(DrawingInstructionsBatch instructionsBatch)
        {
            //draw grid
            Vector2 point;
            int count;
            for (int i = 0; i < physicsSimulator.GeomList.Count; i++)
            {
                if (physicsSimulator.GeomList[i].Grid == null) { continue; }
                count = physicsSimulator.GeomList[i].Grid.Points.Length;
                for (int j = 0; j < count; j++)
                {
                    point = physicsSimulator.GeomList[i].GetWorldPosition(physicsSimulator.GeomList[i].Grid.Points[j]);
                    gridCircleBrush.Draw(instructionsBatch, point);
                }
            }
        }

        private void DrawCoordinateAxis(DrawingInstructionsBatch instructionsBatch)
        {
            for (int i = 0; i < physicsSimulator.BodyList.Count; i++)
            {
                Vector2 startX = physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(-coordinateAxisLineLength / 2f, 0));
                Vector2 endX = physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(coordinateAxisLineLength / 2f, 0));
                Vector2 startY = physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(0, -coordinateAxisLineLength / 2f));
                Vector2 endY = physicsSimulator.BodyList[i].GetWorldPosition(new Vector2(0, coordinateAxisLineLength / 2f));

                coordinateAxisLineBrush.Draw(instructionsBatch, startX, endX);
                coordinateAxisLineBrush.Draw(instructionsBatch, startY, endY);
            }
        }

        private Vector2 body1AttachPointInWorldCoordinates;
        private Vector2 body2AttachPointInWorldCoordinates;
        private Vector2 vectorTemp1;
        private Vector2 worldAttachPoint;
        private Vector2 attachPoint1;
        private Vector2 attachPoint2;
        private void DrawSprings(DrawingInstructionsBatch instructionsBatch)
        {
            for (int i = 0; i < physicsSimulator.ControllerList.Count; i++)
            {
                if (physicsSimulator.ControllerList[i] is FixedLinearSpring)
                {
                    FixedLinearSpring fixedLinearSpring = (FixedLinearSpring)physicsSimulator.ControllerList[i];
                    worldAttachPoint = fixedLinearSpring.WorldAttachPoint;
                    body1AttachPointInWorldCoordinates = fixedLinearSpring.Body.GetWorldPosition(fixedLinearSpring.BodyAttachPoint);
                    springCircleBrush.Draw(instructionsBatch, body1AttachPointInWorldCoordinates);
                    springCircleBrush.Draw(instructionsBatch, worldAttachPoint);

                    Vector2.Lerp(ref worldAttachPoint, ref body1AttachPointInWorldCoordinates, .25f, out vectorTemp1);
                    springCircleBrush.Draw(instructionsBatch, vectorTemp1);

                    Vector2.Lerp(ref worldAttachPoint, ref body1AttachPointInWorldCoordinates, .50f, out vectorTemp1);
                    springCircleBrush.Draw(instructionsBatch, vectorTemp1);

                    Vector2.Lerp(ref worldAttachPoint, ref body1AttachPointInWorldCoordinates, .75f, out vectorTemp1);
                    springCircleBrush.Draw(instructionsBatch, vectorTemp1);

                    springLineBrush.Draw(instructionsBatch, body1AttachPointInWorldCoordinates, fixedLinearSpring.WorldAttachPoint);
                }
            }

            for (int i = 0; i < physicsSimulator.ControllerList.Count; i++)
            {
                if (physicsSimulator.ControllerList[i] is LinearSpring)
                {
                    LinearSpring linearSpring = (LinearSpring)physicsSimulator.ControllerList[i];
                    attachPoint1 = linearSpring.AttachPoint1;
                    attachPoint2 = linearSpring.AttachPoint2;
                    linearSpring.Body1.GetWorldPosition(ref attachPoint1, out body1AttachPointInWorldCoordinates);
                    linearSpring.Body2.GetWorldPosition(ref attachPoint2, out body2AttachPointInWorldCoordinates);
                    springCircleBrush.Draw(instructionsBatch, body1AttachPointInWorldCoordinates);
                    springCircleBrush.Draw(instructionsBatch, body2AttachPointInWorldCoordinates);

                    Vector2.Lerp(ref body1AttachPointInWorldCoordinates, ref body2AttachPointInWorldCoordinates, .25f, out vectorTemp1);
                    springCircleBrush.Draw(instructionsBatch, vectorTemp1);

                    Vector2.Lerp(ref body1AttachPointInWorldCoordinates, ref body2AttachPointInWorldCoordinates, .50f, out vectorTemp1);
                    springCircleBrush.Draw(instructionsBatch, vectorTemp1);

                    Vector2.Lerp(ref body1AttachPointInWorldCoordinates, ref body2AttachPointInWorldCoordinates, .75f, out vectorTemp1);
                    springCircleBrush.Draw(instructionsBatch, vectorTemp1);

                    springLineBrush.Draw(instructionsBatch, body1AttachPointInWorldCoordinates, body2AttachPointInWorldCoordinates);
                }
            }
        }

        private void DrawRevoluteJoints(DrawingInstructionsBatch instructionsBatch)
        {
            for (int i = 0; i < physicsSimulator.JointList.Count; i++)
            {
                if (physicsSimulator.JointList[i] is FixedRevoluteJoint)
                {
                    FixedRevoluteJoint fixedRevoluteJoint = (FixedRevoluteJoint)physicsSimulator.JointList[i];
                    revoluteJointRectangleBrush.Draw(instructionsBatch, fixedRevoluteJoint.Anchor);
                }

                if (physicsSimulator.JointList[i] is RevoluteJoint)
                {
                    RevoluteJoint revoluteJoint = (RevoluteJoint)physicsSimulator.JointList[i];
                    revoluteJointRectangleBrush.Draw(instructionsBatch, revoluteJoint.CurrentAnchor);
                    revoluteJointLineBrush.Draw(instructionsBatch, revoluteJoint.CurrentAnchor, revoluteJoint.Body1.Position);
                    revoluteJointLineBrush.Draw(instructionsBatch, revoluteJoint.CurrentAnchor, revoluteJoint.Body2.Position);
                }
            }
        }

        private void DrawPinJoints(DrawingInstructionsBatch instructionsBatch)
        {
            for (int i = 0; i < physicsSimulator.JointList.Count; i++)
            {
                if (physicsSimulator.JointList[i] is PinJoint)
                {
                    PinJoint pinJoint = (PinJoint)physicsSimulator.JointList[i];
                    pinJointRectangleBrush.Draw(instructionsBatch, pinJoint.WorldAnchor1);
                    pinJointRectangleBrush.Draw(instructionsBatch, pinJoint.WorldAnchor2);
                    pinJointLineBrush.Draw(instructionsBatch, pinJoint.WorldAnchor1, pinJoint.WorldAnchor2);
                }
            }
        }


        private void DrawSliderJoints(DrawingInstructionsBatch instructionsBatch)
        {
            for (int i = 0; i < physicsSimulator.JointList.Count; i++)
            {
                if (physicsSimulator.JointList[i] is SliderJoint)
                {
                    SliderJoint sliderJoint = (SliderJoint)physicsSimulator.JointList[i];
                    sliderJointRectangleBrush.Draw(instructionsBatch, sliderJoint.WorldAnchor1);
                    sliderJointRectangleBrush.Draw(instructionsBatch, sliderJoint.WorldAnchor2);
                    sliderJointLineBrush.Draw(instructionsBatch, sliderJoint.WorldAnchor1, sliderJoint.WorldAnchor2);
                }
            }
        }
    }
}
