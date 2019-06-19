using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Levels.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace GeometryFriends.AI.Perceptions.Navigation
{
    /// <summary>
    /// Navigational graph used by the agent in order to figure out a path
    /// </summary>
    public class NavGraph
    {
        public const float MAX_Y_ACCEL_BY_CIRCLE = -400;
        public const float MAX_Y_ACCEL_BY_RECTANGLE = 0;
        public const float MAX_X_ACCEL_BY_CIRCLE = 200;
        public const float MAX_X_ACCEL_BY_RECTANGLE = 200;
        public const int MIN_GAP_X_SIZE = 60;
        public const int MIN_GAP_Y_SIZE = 60;        
        public const int NODE_RADIUS = 115;
        public const int PATH_GENERATION_RADIUS = 230;
        public const int NUMBER_OF_NODES = 4;

        private object graphLock = new object();

        //Sensors Information
        private int[] numbersInfo;
        private float[] rectangleInfo;
        private float[] circleInfo;
        private float[] obstaclesInfo;
        private float[] rectanglePlatformsInfo;
        private float[] circlePlatformsInfo;
        private float[] collectiblesInfo;
        private Rectangle area;

        private NavNode circleStartNode, tcNode;
        public NavNode CircleStartingNode
        {
            get
            {
                return this.circleStartNode;
            }
        }

        private NavNode rectangleStartNode;
        public NavNode RectangleStartingNode
        {
            get
            {
                return this.rectangleStartNode;
            }
        }

        private LinkedList<NavNode> allNodes;
        public LinkedList<NavNode> GraphNodes
        {
            get
            {
                return this.allNodes;
            }
        }

        private bool complete = false;
        public bool GraphCalculated
        {
            get
            {
                return this.complete;
            }
        }        
        /// <summary>
        /// Creates a NavGraph instance for a certain level
        /// </summary>
        /// <param name="l">SinglePlayerLevel to which the NavGraph will be built</param>
        public NavGraph(int[] nI, float[] sI, float[] cI, float[] oI, float[] sPI, float[] cPI, float[] colI, Rectangle a)
        {
            this.area = a;
            int temp;

            // numbersInfo[] Description
            //
            // Index - Information
            //
            //   0   - Number of Obstacles
            //   1   - Number of Rectangle Platforms
            //   2   - Number of Circle Platforms
            //   3   - Number of Collectibles

            numbersInfo = new int[4];
            int i;
            for (i = 0; i < nI.Length; i++)
            {
                numbersInfo[i] = nI[i];

            }

            // rectangleInfo[] Description
            //
            // Index - Information
            //
            //   0   - Rectangle X Position
            //   1   - Rectangle Y Position
            //   2   - Rectangle X Velocity
            //   3   - Rectangle Y Velocity
            //   4   - Rectangle Height

            rectangleInfo = new float[5];

            rectangleInfo[0] = sI[0];
            rectangleInfo[1] = sI[1];
            rectangleInfo[2] = sI[2];
            rectangleInfo[3] = sI[3];
            rectangleInfo[4] = sI[4];

            // circleInfo[] Description
            //
            // Index - Information
            //
            //   0  - Circle X Position
            //   1  - Circle Y Position
            //   2  - Circle X Velocity
            //   3  - Circle Y Velocity
            //   4  - Circle Radius

            circleInfo = new float[5];

            circleInfo[0] = cI[0];
            circleInfo[1] = cI[1];
            circleInfo[2] = cI[2];
            circleInfo[3] = cI[3];
            circleInfo[4] = cI[4];


            // Obstacles and Platforms Info Description
            //
            //  X = Center X Coordinate
            //  Y = Center Y Coordinate
            //
            //  H = Platform Height
            //  W = Platform Width
            //
            //  Position (X=0,Y=0) = Left Superior Corner

            // obstaclesInfo[] Description
            //
            // Index - Information
            //
            // If (Number of Obstacles > 0)
            //  [0 ; (NumObstacles * 4) - 1]      - Obstacles' info [X,Y,H,W]
            // Else
            //   0                                - 0
            //   1                                - 0
            //   2                                - 0
            //   3                                - 0

            if (numbersInfo[0] > 0)
                obstaclesInfo = new float[numbersInfo[0] * 4];
            else obstaclesInfo = new float[4];

            temp = 1;
            if (nI[0] > 0)
            {
                while (temp <= nI[0])
                {
                    obstaclesInfo[(temp * 4) - 4] = oI[(temp * 4) - 4];
                    obstaclesInfo[(temp * 4) - 3] = oI[(temp * 4) - 3];
                    obstaclesInfo[(temp * 4) - 2] = oI[(temp * 4) - 2];
                    obstaclesInfo[(temp * 4) - 1] = oI[(temp * 4) - 1];
                    temp++;
                }
            }
            else
            {
                obstaclesInfo[0] = oI[0];
                obstaclesInfo[1] = oI[1];
                obstaclesInfo[2] = oI[2];
                obstaclesInfo[3] = oI[3];
            }

            // rectanglePlatformsInfo[] Description
            //
            // Index - Information
            //
            // If (Number of Rectangle Platforms > 0)
            //  [0; (numRectanglePlatforms * 4) - 1]   - Rectangle Platforms' info [X,Y,H,W]
            // Else
            //   0                                  - 0
            //   1                                  - 0
            //   2                                  - 0
            //   3                                  - 0


            if (numbersInfo[1] > 0)
                rectanglePlatformsInfo = new float[numbersInfo[1] * 4];
            else
                rectanglePlatformsInfo = new float[4];

            temp = 1;
            if (nI[1] > 0)
            {
                while (temp <= nI[1])
                {
                    rectanglePlatformsInfo[(temp * 4) - 4] = sPI[(temp * 4) - 4];
                    rectanglePlatformsInfo[(temp * 4) - 3] = sPI[(temp * 4) - 3];
                    rectanglePlatformsInfo[(temp * 4) - 2] = sPI[(temp * 4) - 2];
                    rectanglePlatformsInfo[(temp * 4) - 1] = sPI[(temp * 4) - 1];
                    temp++;
                }
            }
            else
            {
                rectanglePlatformsInfo[0] = sPI[0];
                rectanglePlatformsInfo[1] = sPI[1];
                rectanglePlatformsInfo[2] = sPI[2];
                rectanglePlatformsInfo[3] = sPI[3];
            }

            // circlePlatformsInfo[] Description
            //
            // Index - Information
            //
            // If (Number of Circle Platforms > 0)
            //  [0; (numCirclePlatforms * 4) - 1]   - Circle Platforms' info [X,Y,H,W]
            // Else
            //   0                                  - 0
            //   1                                  - 0
            //   2                                  - 0
            //   3                                  - 0

            if (numbersInfo[2] > 0)
                circlePlatformsInfo = new float[numbersInfo[2] * 4];
            else
                circlePlatformsInfo = new float[4];

            temp = 1;
            if (nI[2] > 0)
            {
                while (temp <= nI[2])
                {
                    circlePlatformsInfo[(temp * 4) - 4] = cPI[(temp * 4) - 4];
                    circlePlatformsInfo[(temp * 4) - 3] = cPI[(temp * 4) - 3];
                    circlePlatformsInfo[(temp * 4) - 2] = cPI[(temp * 4) - 2];
                    circlePlatformsInfo[(temp * 4) - 1] = cPI[(temp * 4) - 1];
                    temp++;
                }
            }
            else
            {
                circlePlatformsInfo[0] = cPI[0];
                circlePlatformsInfo[1] = cPI[1];
                circlePlatformsInfo[2] = cPI[2];
                circlePlatformsInfo[3] = cPI[3];
            }

            //Collectibles' To Catch Coordinates (X,Y)
            //
            //  [0; (numCollectibles * 2) - 1]   - Collectibles' Coordinates (X,Y)

            collectiblesInfo = new float[numbersInfo[3] * 2];

            temp = 1;
            while (temp <= nI[3])
            {

                collectiblesInfo[(temp * 2) - 2] = colI[(temp * 2) - 2];
                collectiblesInfo[(temp * 2) - 1] = colI[(temp * 2) - 1];

                temp++;
            }
            

            this.allNodes = new LinkedList<NavNode>();
            //this.lvl = l;
            Thread t = new Thread(BuildGraph);
            t.IsBackground = true; //ensure the thread does not block the game from properly closing
            t.Start();
        }

        /// <summary>
        /// Builds the graph according to the level geometry and the characters starting points
        /// </summary>
        private void BuildGraph()
        {
            lock (graphLock)
            {
                this.CreateNodesInVertices();                
                //this.squareStartNode = new NavNode(new Point((int)this.lvl.square.Body.Position.X, (int)this.lvl.square.Body.Position.Y), new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE), raycastDownFrom(this.lvl.square.Body.Position));
                this.rectangleStartNode = new NavNode(new Point((int)this.rectangleInfo[0], (int)this.rectangleInfo[1]), new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE), RaycastDownFrom(new Point((int)this.rectangleInfo[0],(int)this.rectangleInfo[1])));
                this.AddNode(rectangleStartNode);
                //this.ballStartNode = new NavNode(new Point((int)this.lvl.ball.Body.Position.X, (int)this.lvl.ball.Body.Position.Y), new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE), raycastDownFrom(this.lvl.ball.Body.Position));
                this.circleStartNode = new NavNode(new Point((int)this.circleInfo[0], (int)this.circleInfo[1]), new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE), RaycastDownFrom(new Point((int)this.circleInfo[0], (int)this.circleInfo[1])));
                this.AddNode(circleStartNode);
                this.GeneratePaths(rectangleStartNode);
                this.GeneratePaths(circleStartNode);

                //Creates a NavNode and generates paths for each collectible
                int temp = 1;
                /*
                foreach (TriangleCollectible tc in this.lvl.getCollectibles())
                {
                  tcNode = new NavNode(new Point((int)tc.X, (int) tc.Y), new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE), raycastDownFrom(tc.getPosition()));
                  this.AddNode(tcNode);
                  this.generatePaths(tcNode);
                }*/
                while (temp <= numbersInfo[3])
                {
                    tcNode = new NavNode(new Point((int)collectiblesInfo[(temp * 2) - 2], (int)collectiblesInfo[(temp * 2) - 1]), new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE), RaycastDownFrom(new Vector2(collectiblesInfo[(temp * 2) - 2], collectiblesInfo[(temp * 2) - 1])));
                    this.AddNode(tcNode);
                    this.GeneratePaths(tcNode);

                    temp++;
                }

                this.CreateNodes();
            }
        }

        private void CreateNodes()
        {
            Point possibleNodePosition;
            Point possibleNodeDimensions;
            NavNode otherNode, newNode;
            int currentNewNode = 0;            
            NavNode origin;

            Log.LogRaw("DEBUG CREATE NODES", Log.EnumDebugLevel.NAVGRAPH);

            int currentNode = 0;
            //while there are nodes to try, this method will create new nodes
            while (currentNode < this.allNodes.Count())
            {
                origin = this.allNodes.ElementAt(currentNode);
                ++currentNode;
                //generate new nodes around this node
                for (currentNewNode = 0; currentNewNode < NUMBER_OF_NODES; ++currentNewNode)
                {

                    //create nodes in a "cross" fashion
                    switch (currentNewNode)
                    {
                        case 0:
                            possibleNodePosition = new Point(origin.Position.X + NavGraph.NODE_RADIUS, origin.Position.Y);
                            break;
                        case 1:
                            possibleNodePosition = new Point(origin.Position.X, origin.Position.Y - NavGraph.NODE_RADIUS);
                            break;
                        case 2:
                            possibleNodePosition = new Point(origin.Position.X - NavGraph.NODE_RADIUS, origin.Position.Y);
                            break;
                        default:
                            possibleNodePosition = new Point(origin.Position.X, origin.Position.Y + NavGraph.NODE_RADIUS);
                            break;
                    }

                    possibleNodeDimensions = new Point(NavGraph.MIN_GAP_X_SIZE, NavGraph.MIN_GAP_Y_SIZE);

                    otherNode = IsInRangeOfAnotherNode(possibleNodePosition, origin, NavGraph.NODE_RADIUS);
                    if (otherNode != null)
                    {
                        Log.LogRaw("skipping node" + possibleNodePosition + "because it is in range of node" + otherNode.Position, Log.EnumDebugLevel.NAVGRAPH);
                        continue; //this makes it jump to next possible point
                    }
                    else
                    {
                        //check if the point is reachable, i.e. if it is inside or on the other side of an obstacle 
                        if (LineIntersectsObstacles(origin.Position, possibleNodePosition, obstaclesInfo))
                        {
                            Log.LogRaw("COLLISION WITH OBJECT. CANNOT CREATE NODE" + possibleNodePosition, Log.EnumDebugLevel.NAVGRAPH);
                            continue;
                        }

                        //cool story bro. now create a node and the respective navPath(s)
                        //if (this.lvl.Area.Contains(new Point(possibleNodePosition.X, possibleNodePosition.Y))) //makes sure no nodes are generated beyond level borders
                        if (area.Contains(new Point(possibleNodePosition.X, possibleNodePosition.Y))) //makes sure no nodes are generated beyond level borders
                        {
                            newNode = new NavNode(possibleNodePosition, possibleNodeDimensions, RaycastDownFrom(possibleNodePosition), origin);
                            this.AddNode(newNode);
                            GeneratePaths(newNode);
                        }
                        else
                        {
                            //DebugLog._Log("Node " + possibleNodePosition + "failed to generate because it is out of bounds: " + this.lvl.Area, DebugLog.EnumDebugLevel.NAVGRAPH);
                            Log.LogRaw("Node " + possibleNodePosition + "failed to generate because it is out of bounds: " + area, Log.EnumDebugLevel.NAVGRAPH);
                        }
                    }
                }
            }
            this.complete = true;
        }

        /// <summary>
        /// Method that generates paths from the node in question to all existent nodes in range
        /// </summary>
        /// <param name="n"></param>
        private void GeneratePaths(NavNode n)
        {
            float dist;
            bool rectangleOnly, circleOnly;
            Vector2 minVel, inverseVel;
            foreach (NavNode node in this.allNodes)
            {
                dist = n.Distance(node.Position);
                if (dist <= NavGraph.PATH_GENERATION_RADIUS && n != node)
                {
                    rectangleOnly = false;
                    circleOnly = false;
                    //verify if there is a clear path                    
                    //if (NavGraph.lineIntersectsObstacles(n.Position, node.Position, this.lvl.getBlackObstacles()))
                    if (NavGraph.LineIntersectsObstacles(n.Position, node.Position, obstaclesInfo))
                    {
                        continue;
                    }
                    else
                    {
                        //squareOnly = lineIntersectsObstacles(node.Position, n.Position, this.lvl.getYellowObstacles());                        
                        rectangleOnly = LineIntersectsObstacles(node.Position, n.Position, circlePlatformsInfo);                        
                        
                        if (!rectangleOnly)
                        {
                            //circleOnly = lineIntersectsObstacles(node.Position, n.Position, this.lvl.getGreenObstacles());
                            circleOnly = LineIntersectsObstacles(node.Position, n.Position, rectanglePlatformsInfo);                            
                        
                        }
                        //verify target node's position relative to this node. if its higher, it needs a larger min velocity. if its lower it doesnt have requirements
                        minVel = node.PositionVector - n.PositionVector;
                        inverseVel = -minVel;
                        if (minVel.Y <= 0)
                        {
                            minVel.Y -= n.DistanceFromGround;
                        }
                        if (inverseVel.Y <= 0)
                        {
                            inverseVel.Y -= node.DistanceFromGround;
                        }

                        if (ValidRectanglePath(minVel) && ValidCirclePath(minVel))
                        {
                            n.AddPath(new NavPath(node, minVel, Vector2.Zero, rectangleOnly, circleOnly));
                        }
                        else
                        {
                            if (!ValidRectanglePath(minVel) && ValidCirclePath(minVel) && !rectangleOnly)
                            {
                                n.AddPath(new NavPath(node, minVel, Vector2.Zero, false, true));
                            }
                            
                            if (ValidRectanglePath(minVel) && !ValidCirclePath(minVel) && !circleOnly)
                            {
                                n.AddPath(new NavPath(node, minVel, Vector2.Zero, false, false));
                            }                            
                        }

                        //same as previous but for inverse path
                        if (ValidRectanglePath(inverseVel) && ValidCirclePath(inverseVel))
                        {
                            node.AddPath(new NavPath(n, inverseVel, Vector2.Zero, rectangleOnly, circleOnly));
                        }
                        else
                        {
                            if (!ValidRectanglePath(inverseVel) && ValidCirclePath(inverseVel) && !rectangleOnly)
                            {
                                node.AddPath(new NavPath(n, inverseVel, Vector2.Zero, false, true));
                            }

                            if (ValidRectanglePath(inverseVel) && !ValidCirclePath(inverseVel) && !circleOnly)
                            {
                                node.AddPath(new NavPath(n, inverseVel, Vector2.Zero, true, false));
                            }
                        }                     
                    }
                }
            }
        }

        /// <summary>
        /// method that checks if this path's velocities make him possible or not
        /// </summary>
        /// <param name="requiredVel"></param>
        /// <returns></returns>
        private bool ValidRectanglePath(Vector2 requiredVel)
        {
            //if acceleration needed to reach is higher than rectangle and circle can achieve, dont create path
            if (requiredVel.Y < MAX_Y_ACCEL_BY_RECTANGLE - RectangleCharacter.MAXIMUM_HEIGHT)
            {
                return false;
            }
            else
            {                
                return true;
            }
        }

        private bool ValidCirclePath(Vector2 requiredVel)
        {
            if (requiredVel.Y < MAX_Y_ACCEL_BY_CIRCLE)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void AddNode(NavNode newNode)
        {
            Log.LogRaw("new node: "+ newNode.Area, Log.EnumDebugLevel.NAVGRAPH);
            this.allNodes.AddLast(newNode);
        }

        /// <summary>
        /// method that compares a line segment defined by point1 and point2, and checks if it intercepts 
        /// any of the 4 lines that define a certain obstacle
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="obstacle"></param>
        /// <returns></returns>
        public static bool LineIntersectsWithObstacle(Point point1, Point point2, float[] obstacle)
        {
            float left, right, top, bot;
            Point v1, v2, v3, v4;

            //obstacle vertexes
            left = obstacle[0] - (obstacle[3] / 2);  // left  = X - (Width/2)
            right = obstacle[0] + (obstacle[3] / 2); // right = X + (Width/2)
            top = obstacle[1] - (obstacle[2] / 2);   // top   = Y - (Height/2)
            bot = obstacle[1] + (obstacle[2] / 2);   // bot   = Y + (Height/2)

            v1 = new Point((int)left, (int)top);
            v2 = new Point((int)right, (int)top);
            v3 = new Point((int)right, (int)bot);
            v4 = new Point((int)left, (int)bot);
            return LinesIntersect(point1, point2, v1, v2) 
                || LinesIntersect(point1, point2, v2, v3)
                || LinesIntersect(point1, point2, v3, v4)
                || LinesIntersect(point1, point2, v4, v1);
        }
        /*
        public static bool lineIntersectsWithObstacle(Point point1, Point point2, GeoShape obstacle)
        {
            float leftTop, leftBot, rightTop, rightBot;

            Point v1, v2, v3, v4;
            //obstacle vertexes            
            v1 = new Point(obstacle.Area.Left, obstacle.Area.Top);
            v2 = new Point(obstacle.Area.Right, obstacle.Area.Top);
            v3 = new Point(obstacle.Area.Right, obstacle.Area.Bottom);
            v4 = new Point(obstacle.Area.Left, obstacle.Area.Bottom);
            return linesIntersect(point1, point2, v1, v2)
                || linesIntersect(point1, point2, v2, v3)
                || linesIntersect(point1, point2, v3, v4)
                || linesIntersect(point1, point2, v4, v1);
         }
        */

        /// <summary>
        /// based on Paul Bourke's 2 line intersection algorithm.
        /// </summary>
        /// <returns></returns>
        private static bool LinesIntersect(Point point1, Point point2, Point point3, Point point4)
        {
            float ua = (point4.X - point3.X) * (point1.Y - point3.Y) - (point4.Y - point3.Y) * (point1.X - point3.X);
            float ub = (point2.X - point1.X) * (point1.Y - point3.Y) - (point2.Y - point1.Y) * (point1.X - point3.X);
            float denominator = (point4.Y - point3.Y) * (point2.X - point1.X) - (point4.X - point3.X) * (point2.Y - point1.Y);

            if (Math.Abs(denominator) <= 0.001f)
            {
                //deliberately ignoring the case where lines can be coincident. I assume that will never happen, since nodes
                //are being created within a limited distance from the obstacles. this saves a couple floating point calculations 
                return false;
            }
            else
            {
                ua /= denominator;
                ub /= denominator;

                if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks whether possibleNodePosition is within range of any node in the node list
        /// </summary>
        /// <param name="possibleNodePosition">position to test for</param>
        /// <param name="origin">node where it is generated from</param>
        /// <returns></returns>
        private NavNode IsInRangeOfAnotherNode(Point possibleNodePosition, NavNode origin, int range)
        {
            foreach (NavNode node in allNodes)
                if (node.Distance(possibleNodePosition) < range && node != origin)
                    return node;
            return null;
        }

        /// <summary>
        /// Function used to check if we reached the final node of the path. we could use getPathFromAtoB to do that, but its much more innefective
        /// </summary>
        /// <param name="a">where we are departing</param>
        /// <param name="b">destination </param>
        /// <param name="isCircle">if agent is circle or rectangle</param>
        /// <returns>true if starting node is the same as ending node, false otherwise</returns>
        public bool AreWeThereYet(Vector2 a, Vector2 b, bool isCircle)
        {
            NavNode startingNode = GetClosestNodeWithClearPathFromPoint(new Point((int)a.X, (int)a.Y), isCircle);
            NavNode endingNode = GetClosestNodeWithClearPathFromPoint(new Point((int)b.X, (int)b.Y), isCircle);

            return startingNode == endingNode;
        }

        /// <summary>
        /// Returns a list with the coordinates of the nodes from point a (included) to b (also included)
        /// specific to rectangle or circle
        /// </summary>
        /// <param name="a">point a. origin of the path</param>
        /// <param name="b">point b. destination of the path</param>
        /// <param name="isCircle">boolean determining whether it is a path for circle or rectangle.</param>
        /// <returns>returns a list of waypoints. if the list is empty, it means the char is already at the spot. if it returns null, it means there is no path available</returns>
        public List<Vector2> GetPathFromAtoB(Vector2 a, Vector2 b, bool isCircle)
        {
            List<Vector2> points = new List<Vector2>();  //list where we add the position of the nodes in the path            
            List<NavNode> visitedNodes = new List<NavNode>();

            if (this.complete)
            {
                lock (graphLock)
                {
                    NavNode startingNode, endingNode;
                    NavNode currentNode;
                    startingNode = GetClosestNodeWithClearPathFromPoint(new Point((int)a.X, (int)a.Y), isCircle);
                    endingNode = GetClosestNodeWithClearPathFromPoint(new Point((int)b.X, (int)b.Y), isCircle);

                    if (startingNode == null || endingNode == null) return null;
                    if (startingNode == endingNode) return points;
                    //if the ending point is the same as the starting point, there is no reason to continue, so we just return an empty list

                    //before doing anything else, reset costs from previous graph run. there must be a more efficient way to do this
                    foreach (NavNode n in this.allNodes)
                    {
                        n.ResetCost();
                    }

                    AStarNavNodeComparer comparer = new AStarNavNodeComparer(endingNode);
                    PriorityQueue openNodes = new PriorityQueue(comparer);

                    startingNode.OpenNode();
                    openNodes.Enqueue(startingNode);
                    do
                    {
                        currentNode = (NavNode)openNodes.Dequeue();
                        visitedNodes.Add(currentNode);
                        foreach (NavPath np in currentNode.Paths)
                        {
                            if ((isCircle ? np.RectangleOnly : np.CircleOnly))
                           {
                               Log.LogRaw("DEBUG skipping path from " + currentNode.Position + " to " + np.TargetNode.Position + " because its " + (isCircle ? "square only" : "circle only"), Log.EnumDebugLevel.NAVGRAPH);
                                continue;
                           }

                            if (!visitedNodes.Contains(np.TargetNode) && !openNodes.Contains(np.TargetNode))
                            {
                                np.TargetNode.OpenNode(currentNode);
                                openNodes.Enqueue(np.TargetNode);
                            }
                            else
                            {
                                if (np.TargetNode.AStarParent != null && np.TargetNode.AStarParent.AStarCost > currentNode.AStarCost)
                                    np.TargetNode.OpenNode(currentNode);
                            }
                        }
                    }
                    while (currentNode != endingNode && openNodes.Count > 0);

                    if (currentNode != endingNode) return null; //it means no path was found, so it is impossible
                    for (; currentNode.AStarParent != null; currentNode = currentNode.AStarParent)
                    {
                        Log.LogRaw("DEBUG path point added", Log.EnumDebugLevel.NAVGRAPH);
                        points.Add(currentNode.PositionVector);
                    }
                }
            }

            Log.LogRaw("DEBUG Nav graph points" + points.Count(), Log.EnumDebugLevel.NAVGRAPH);

            return points;
            
        }

        /// <summary>
        /// Method that returns the closest node with a clear path to a certain point in the map.
        /// Since the nodes are discrete, there is a need to know which is the closest node to the agent
        /// in the navigational graph
        /// </summary>
        /// <param name="p">point that we use to search the navgraph</param>
        /// <param name="isCircle">boolean determining whether we are also looking on circle only nodes or rectangle only nodes</param>
        /// <returns></returns>
        public NavNode GetClosestNodeWithClearPathFromPoint(Point p, bool isCircle)
        {
            NavNode closestNode = null;
            float distance;         
            distance = float.MaxValue;
            foreach (NavNode n in this.allNodes)
            {
                if (n.Distance(p) < distance)
                {
                    if (!LineIntersectsObstacles(p, n.Position, obstaclesInfo))
                    {
                        if (isCircle)
                        {
                            if (!LineIntersectsObstacles(p, n.Position, rectanglePlatformsInfo))
                            {
                                distance = n.Distance(p);
                                closestNode = n;
                            }
                        }
                        else
                            if (!LineIntersectsObstacles(p, n.Position, circlePlatformsInfo))
                            {
                                distance = n.Distance(p);
                                closestNode = n;
                            }
                    }
                }
            }

            return closestNode;
        }
        /*
        public NavNode getClosestNodeWithClearPathFromPoint(Point p, bool isCircle)
        {
            NavNode closestNode = null;
            float distance;
            List<RectanglePlatform> obs;
            distance = float.MaxValue;
            foreach (NavNode n in this.allNodes)
            {
                if (n.Distance(p) < distance)
                {
                    if (!lineIntersectsObstacles(p, n.Position, this.lvl.getBlackObstacles()))
                    {
                        if (isCircle)
                            obs = this.lvl.getGreenObstacles();
                        else
                            obs = this.lvl.getYellowObstacles();
                        if (!lineIntersectsObstacles(p, n.Position, obs))
                        {
                            distance = n.Distance(p);
                            closestNode = n;
                        }
                    }
                }
            }

            return closestNode;
        }
        */

        public static bool LineIntersectsObstacles(Point p1, Point p2, float[] obstaclesI)
        {
            //Todo juntar as plataformas aos obstaculos(diferente para cada agente)
            int temp = 1;  

            while( temp <= (obstaclesI.Count()/4))
            {
                float[] obst = new float[4];
                obst[0] = obstaclesI[(temp * 4) - 4]; // X
                obst[1] = obstaclesI[(temp * 4) - 3]; // Y
                obst[2] = obstaclesI[(temp * 4) - 2]; // Height
                obst[3] = obstaclesI[(temp * 4) - 1]; // Width

                if (NavGraph.LineIntersectsWithObstacle(p1, p2,obst))
                {
                    return true;
                }

                temp++;
            }
            return false;
        }
        /*
        public static bool lineIntersectsObstacles(Point p1, Point p2, List<RectanglePlatform> obstacles)
        {
            foreach (RectanglePlatform obstacle in obstacles)
            {
                if (NavGraph.lineIntersectsWithObstacle(p1, p2, obstacle))
                {
                    return true;
                }
            }
            return false;
        }
        */

        public void CreateNodesInVertices()
        {
            int nodeOffset = 40;
            int nodeRadius = 40;
            Point[] possibleNodes = new Point[4];
            Point[] corners = new Point[4];
            Point nodeDims = new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE);

            NavNode newNode;

            float left, right, top, bot;

            int temp = 1;

            while (temp <= (obstaclesInfo.Count() / 4))
            {
                float[] obst = new float[4];
                obst[0] = obstaclesInfo[(temp * 4) - 4]; // X
                obst[1] = obstaclesInfo[(temp * 4) - 3]; // Y
                obst[2] = obstaclesInfo[(temp * 4) - 2]; // Height
                obst[3] = obstaclesInfo[(temp * 4) - 1]; // Width

                //obstacle vertexes
                left = obst[0] - (obst[3] / 2);  // left  = X - (Width/2)
                right = obst[0] + (obst[3] / 2); // right = X + (Width/2)
                top = obst[1] - (obst[2] / 2);   // top   = Y - (Height/2)
                bot = obst[1] + (obst[2] / 2);   // bot   = Y + (Height/2)


                corners[0] = new Point((int)left, (int)top);
                corners[1] = new Point((int)right, (int)top);
                corners[2] = new Point((int)right, (int)bot);
                corners[3] = new Point((int)left, (int)bot);

                Log.LogRaw("Trying corners: \n" + corners[0] + "\n" + corners[1] + "\n" + corners[2] + "\n" + corners[3] + "\n", Log.EnumDebugLevel.NAVGRAPH);
                foreach (Point corner in corners)
                {
                    possibleNodes[0] = new Point(corner.X - nodeOffset, corner.Y - nodeOffset);
                    possibleNodes[1] = new Point(corner.X + nodeOffset, corner.Y - nodeOffset);
                    possibleNodes[2] = new Point(corner.X + nodeOffset, corner.Y + nodeOffset);
                    possibleNodes[3] = new Point(corner.X - nodeOffset, corner.Y + nodeOffset);

                    for(int i = 0; i < 4; ++i)
                    {
                        //check if the point is reachable, i.e. if it is inside or on the other side of an obstacle 
                        if (PointIsInvalid(possibleNodes[i], nodeRadius) || IsInRangeOfAnotherNode(possibleNodes[i],null, nodeRadius) != null)
                        {
                            Log.LogRaw("COLLISION WITH OBJECT. CANNOT CREATE vertice NODE" + possibleNodes[i], Log.EnumDebugLevel.NAVGRAPH);
                            continue;
                        }
                        else
                        {
                            if (area.Contains(possibleNodes[i])) //makes sure no nodes are generated beyond level borders
                            {
                                newNode = new NavNode(possibleNodes[i], nodeDims, RaycastDownFrom(possibleNodes[i]));
                                this.AddNode(newNode);
                                GeneratePaths(newNode);
                            }
                            else
                            {
                                Log.LogRaw("Node " + possibleNodes[i] + "failed to generate vertice node because it is out of bounds", Log.EnumDebugLevel.NAVGRAPH);
                            }
                        }
                    }
                }
                temp++;
            }
        }
        /*
        
        public void createNodesInVertices()
        {
            int nodeOffset = 40;
            int nodeRadius = 40;
            Point[] possibleNodes = new Point[4];
            Point[] corners = new Point[4];
            Point nodeDims = new Point(MIN_GAP_X_SIZE, MIN_GAP_Y_SIZE);

            NavNode newNode;

            foreach (RectanglePlatform pf in this.lvl.getObstacles())
            {
                corners[0] = new Point(pf.Area.Left, pf.Area.Top);
                corners[1] = new Point(pf.Area.Right, pf.Area.Top);
                corners[2] = new Point(pf.Area.Right, pf.Area.Bottom);
                corners[3] = new Point(pf.Area.Left, pf.Area.Bottom);

                DebugLog._Log("Trying corners: \n" + corners[0] + "\n" + corners[1] + "\n" + corners[2] + "\n" + corners[3] + "\n", DebugLog.EnumDebugLevel.NAVGRAPH);
                foreach (Point corner in corners)
                {
                    possibleNodes[0] = new Point(corner.X - nodeOffset, corner.Y - nodeOffset);
                    possibleNodes[1] = new Point(corner.X + nodeOffset, corner.Y - nodeOffset);
                    possibleNodes[2] = new Point(corner.X + nodeOffset, corner.Y + nodeOffset);
                    possibleNodes[3] = new Point(corner.X - nodeOffset, corner.Y + nodeOffset);

                    for(int i = 0; i < 4; ++i)
                    {
                        //check if the point is reachable, i.e. if it is inside or on the other side of an obstacle 
                        if (pointIsInvalid(possibleNodes[i], nodeRadius) || isInRangeOfAnotherNode(possibleNodes[i],null, nodeRadius) != null)
                        {
                            DebugLog._Log("COLLISION WITH OBJECT. CANNOT CREATE vertice NODE" + possibleNodes[i], DebugLog.EnumDebugLevel.NAVGRAPH);
                            continue;
                        }
                        else
                        {
                            if (this.lvl.Area.Contains(possibleNodes[i])) //makes sure no nodes are generated beyond level borders
                            {
                                newNode = new NavNode(possibleNodes[i], nodeDims, raycastDownFrom(possibleNodes[i]));
                                this.AddNode(newNode);
                                generatePaths(newNode);
                            }
                            else
                            {
                                DebugLog._Log("Node " + possibleNodes[i] + "failed to generate vertice node because it is out of bounds", DebugLog.EnumDebugLevel.NAVGRAPH);
                            }
                        }
                    }
                }
            }
        }

        */

        private bool PointIsInvalid(Point p, int areaFree)
        {
            return (LineIntersectsObstacles(new Point(p.X - areaFree, p.Y), p, obstaclesInfo)
                || LineIntersectsObstacles(new Point(p.X + areaFree, p.Y), p, obstaclesInfo)
                || LineIntersectsObstacles(new Point(p.X, p.Y - areaFree), p, obstaclesInfo)
                || LineIntersectsObstacles(new Point(p.X, p.Y + areaFree), p, obstaclesInfo));
        }
        /*
        private bool pointIsInvalid(Point p, int areaFree)
        {
            return (lineIntersectsObstacles(new Point(p.X - areaFree, p.Y), p, this.lvl.getBlackObstacles())
                || lineIntersectsObstacles(new Point(p.X + areaFree, p.Y), p, this.lvl.getBlackObstacles())
                || lineIntersectsObstacles(new Point(p.X, p.Y - areaFree), p, this.lvl.getBlackObstacles())
                || lineIntersectsObstacles(new Point(p.X, p.Y + areaFree), p, this.lvl.getBlackObstacles()));
        }
        */

        /// <summary>
        /// Method that simulates a raycast straight down from a point.
        /// </summary>
        /// <param name="p"> Point where we are "raycasting" from. The verification is done straight down and only on black obstacles AND bottom border</param>
        /// <returns>distance from point p and closest black obstacle AND bottom border</returns>
        private int RaycastDownFrom(Vector2 v)
        {
            return RaycastDownFrom(new Point((int)v.X, (int)v.Y));
        }

        private int RaycastDownFrom(Point p)
        {
            int closestY = area.Bottom - area.X; //a trick to save a check if the bottom border intersects or not: just used bottom border value as max value, since that is the max distance a node can be from the ground
            float left, right, top, bot;

            int temp = 1;

            while (temp <= (obstaclesInfo.Count() / 4))
            {
                float[] obst = new float[4];
                obst[0] = obstaclesInfo[(temp * 4) - 4]; // X
                obst[1] = obstaclesInfo[(temp * 4) - 3]; // Y
                obst[2] = obstaclesInfo[(temp * 4) - 2]; // Height
                obst[3] = obstaclesInfo[(temp * 4) - 1]; // Width

                //obstacle vertexes
                left = obst[0] - (obst[3] / 2);  // left  = X - (Width/2)
                right = obst[0] + (obst[3] / 2); // right = X + (Width/2)
                top = obst[1] - (obst[2] / 2);   // top   = Y - (Height/2)
                bot = obst[1] + (obst[2] / 2);   // bot   = Y + (Height/2)

                //we only need to run this for the top-most line on each obstacle, which means the line defined by topleft and topright corners
                //so, pick the obstacle with lower Y who has p.X between their top corners AND have their Y higher than p.Y (which means they're under p)
                if (p.Y <= top && p.X >= left && p.X <= right && closestY > top)
                {
                    closestY = (int)top;
                }

                temp++;
            }
            return closestY - p.Y;
        }
        /*
        private int raycastDownFrom(Point p)
        {                        
            int closestY = this.lvl.Area.Bottom - this.lvl.getBorderWidth(); //a trick to save a check if the bottom border intersects or not: just used bottom border value as max value, since that is the max distance a node can be from the ground
            foreach (RectanglePlatform obs in this.lvl.getBlackObstacles())
            {
                //we only need to run this for the top-most line on each obstacle, which means the line defined by topleft and topright corners
                //so, pick the obstacle with lower Y who has p.X between their top corners AND have their Y higher than p.Y (which means they're under p)
                
                if(p.Y <= obs.Area.Top && p.X >= obs.Area.Left && p.X <= obs.Area.Right && closestY > obs.Area.Top)
                {
                    closestY = obs.Area.Top;
                }                
            }
            return closestY - p.Y;
        }
        */
    }
}
