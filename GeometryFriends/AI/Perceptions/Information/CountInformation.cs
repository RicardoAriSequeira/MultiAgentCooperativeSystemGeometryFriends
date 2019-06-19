
namespace GeometryFriends.AI.Perceptions.Information
{
    /// <summary>
    /// A container for level related information counts
    /// </summary>
    public struct CountInformation
    {
        /// <summary>
        /// Number of obstacles in the level.
        /// </summary>
        public int ObstaclesCount;
        /// <summary>
        /// Number of rectangle specific platforms.
        /// </summary>
        public int RectanglePlatformsCount;
        /// <summary>
        /// Number of circle specific platforms.
        /// </summary>
        public int CirclePlatformsCount;
        /// <summary>
        /// Number of remaining collectibles in the level.
        /// </summary>
        public int CollectiblesCount;

        /// <summary>
        /// Constructor for a count information of a level.
        /// </summary>
        /// <param name="obstacles">Number of obstacles in the level.</param>
        /// <param name="rectanglePlatforms">Number of rectangle specific platforms.</param>
        /// <param name="circlePlatforms">Number of circle specific platforms.</param>
        /// <param name="collectibles">Number of remaining collectibles in the level.</param>
        public CountInformation(int obstacles, int rectanglePlatforms, int circlePlatforms, int collectibles)
        {
            ObstaclesCount = obstacles;
            RectanglePlatformsCount = rectanglePlatforms;
            CirclePlatformsCount = circlePlatforms;
            CollectiblesCount = collectibles;
        }

        /// <summary>
        /// An array representation of the count information.
        /// </summary>
        /// <returns>Array containing the count information (ObstaclesCount, RectanglePlatformsCount, CirclePlatformsCount, CollectiblesCount).</returns>
        public int[] ToArray()
        {
            return new int[4] {
                ObstaclesCount,
                RectanglePlatformsCount,
                CirclePlatformsCount,
                CollectiblesCount
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CountInformation))
                return false;

            CountInformation cr = (CountInformation)obj;

            return this.Equals(cr);
        }

        public bool Equals(CountInformation representation)
        {
            if (this.ObstaclesCount == representation.ObstaclesCount &&
                this.RectanglePlatformsCount == representation.RectanglePlatformsCount &&
                this.CirclePlatformsCount == representation.CirclePlatformsCount &&
                this.CollectiblesCount == representation.CollectiblesCount)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + ObstaclesCount.GetHashCode();
                hash = hash * 23 + RectanglePlatformsCount.GetHashCode();
                hash = hash * 23 + CirclePlatformsCount.GetHashCode();
                hash = hash * 23 + CollectiblesCount.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "Count Information Obstacles: " + ObstaclesCount + " RectanglePlatforms: " + RectanglePlatformsCount + " CirclePlatforms: " + CirclePlatformsCount + " CollectiblesLeft: " + CollectiblesCount;
        }
    }
}
