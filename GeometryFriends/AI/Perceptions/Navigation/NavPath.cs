using FarseerGames.FarseerPhysics.Mathematics;

namespace GeometryFriends.AI.Perceptions.Navigation
{
    /// <summary>
    /// 
    /// </summary>
    public class NavPath
    {
        private bool circleOnly;
        /// <summary>
        /// Property that defines whether or not this is a path only usable by the circle
        /// (i.e. crosses an obstacle that rectangle cannot cross)
        /// </summary>
        public bool CircleOnly
        {
            get
            {
                return this.circleOnly;
            }
        }

        
        private bool rectangleOnly;
        /// <summary>
        /// Property that defines whether or not this is a path only usable by the rectangle
        /// (i.e. crosses an obstacle that circle cannot cross)
        /// </summary>
        public bool RectangleOnly
        {
            get
            {
                return this.rectangleOnly;
            }
        }

        private Vector2 maxDimensions;
        /// <summary>
        /// Maximum size allowed to navigate through this path.
        /// Useful to determine if a certain character can go through in its current shape/size
        /// </summary>
        public Vector2 MaximumDimensions
        {
            get
            {
                return this.maxDimensions;
            }
        }
        private Vector2 requiredVel;

        /// <summary>
        /// Minimum velocity required to reach target node using this path.
        /// </summary>
        public Vector2 RequiredVelocity
        {
            get
            {
                return this.requiredVel;
            }
        }

        private NavNode targetNode;
        public NavNode TargetNode
        {
            get
            {
                return this.targetNode;
            }
        }

        public NavPath(NavNode target, Vector2 requiredVelocity, Vector2 maxSize, bool rectangleOnly, bool circleOnly)
        {
            this.targetNode = target;
            this.requiredVel = requiredVelocity;
            this.maxDimensions = maxSize;
            this.rectangleOnly = rectangleOnly;
            this.circleOnly = circleOnly;
        }
    }
}
