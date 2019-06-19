using FarseerGames.FarseerPhysics.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GeometryFriends.AI.Perceptions.Navigation
{
    /// <summary>
    /// Node representing a position in the game world.
    /// </summary>
    public class NavNode
    {   
        
        private NavNode parent;
        public NavNode AStarParent
        {
            get
            {
                return this.parent;
            }

            set
            {
                this.parent = value;
            }
        }

        private int costUntilNow;
        public int AStarCost
        {
            get
            {
                return this.costUntilNow;
            }
        }

        private Point position;
        public Point Position
        {
            get
            {
                return this.position;
            }
        }

        private Vector2 positionVector;
        public Vector2 PositionVector
        {
            get
            {
                return this.positionVector;
            }
        }

        private Rectangle area;
        public Rectangle Area
        {
            get
            {
                return area;
            }
        }

        private Dictionary<NavNode,NavPath> paths;
        public List<NavPath> Paths
        {
            get
            {
                return this.paths.Values.ToList();
            }
        }

        public NavNode(Point pos, Point dimensions, int distanceFromGround, NavNode origin = null)
        {
            this.distanceFromGround = distanceFromGround;
            this.paths = new Dictionary<NavNode,NavPath>();
           
            this.area = new Rectangle(pos.X - dimensions.X / 2, pos.Y - dimensions.Y / 2, dimensions.X, dimensions.Y);
            this.position = pos;
            this.positionVector = new Vector2(pos.X, pos.Y);
            //try to reach origin node in case it is possible
        }
        /// <summary>
        /// adds a new path to its list and creates a return path from this node to the one where this path comes from if possible
        /// </summary>
        /// <param name="np">new path to add</param>
        /// <param name="target">node where this path comes from, in case it is possible to create a returning path</param>
        public void AddPath(NavPath np)
        {
            
            if (!this.paths.ContainsKey(np.TargetNode))
            {
                this.paths.Add(np.TargetNode, np);
            }
        }

        public double SquaredDistance(Point otherPoint)
        {
            //attempting an optimization, assuming most points will have at least one equal coordinate, which simplifies the math greatly
            if (this.position.X == otherPoint.X)
                return Math.Pow((otherPoint.Y - this.position.Y),2);
            if (this.position.Y == otherPoint.Y)
                return Math.Pow((otherPoint.X - this.position.X),2);

            return Math.Pow((otherPoint.X - this.position.X), 2) + Math.Pow((otherPoint.Y - this.position.Y), 2);
        }

        public float Distance(Point otherPoint)
        {        
            return (float)Math.Round(Math.Sqrt(this.SquaredDistance(otherPoint)));
        }

        public void OpenNode(NavNode parent = null)
        {
            this.parent = parent;
            if (parent == null)
                this.costUntilNow = 0;
            else
                this.costUntilNow = (int)(parent.AStarCost + this.Distance(parent.Position));
        }

        public double GetTotalCostTo(Point target)
        {
            return this.costUntilNow + this.SquaredDistance(target);
        }

        private int distanceFromGround;
        public int DistanceFromGround
        {
            get
            {
                return this.distanceFromGround;
            }
        }

        public void ResetCost()
        {
            this.costUntilNow = 0;
        }
    }
}
