
namespace GeometryFriends.AI.Perceptions.Information
{
    /// <summary>
    /// A minimal representation of an obstacle or platform.
    /// </summary>
    public struct ObstacleRepresentation
    {
        /// <summary>
        /// X position of the obstacle or platform.
        /// </summary>
        public float X;
        /// <summary>
        /// Y position of the obstacle or platform.
        /// </summary>
        public float Y;
        /// <summary>
        /// Width position of the obstacle or platform.
        /// </summary>
        public float Width;
        /// <summary>
        /// Height position of the obstacle or platform.
        /// </summary>
        public float Height;

        /// <summary>
        /// Constructor for a obstacle/platform representation.
        /// </summary>
        /// <param name="x">X position of the obstacle or platform.</param>
        /// <param name="y">Y position of the obstacle or platform.</param>
        /// <param name="width">Width position of the obstacle or platform.</param>
        /// <param name="height">Height position of the obstacle or platform.</param>
        public ObstacleRepresentation(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// An array representation of the obstacle/platform representation.
        /// </summary>
        /// <returns>Array containing the obstacle/platform representation (X, Y, Height, Width).</returns>
        public float[] ToArray()
        {
            return new float[4] {
                X,
                Y,
                Height,
                Width
            };
        }

        /// <summary>
        /// Converts an array of obstacle/platform representations into a float array (following the obsolete specification)
        /// </summary>
        /// <returns>The float[] containing the obselete representation of the obstacle/platform.</returns>
        public static float[] RepresentationArrayToFloatArray(ObstacleRepresentation[] representations)
        {
            if (representations.Length == 0)
                return new float[4] { 0, 0, 0, 0 };
            else
            {
                float[] result = new float[representations.Length * 4];
                for (int i = 1; i <= representations.Length; i++)
                {
                    result[i * 4 - 4] = representations[i - 1].X;
                    result[i * 4 - 3] = representations[i - 1].Y;
                    result[i * 4 - 2] = representations[i - 1].Height;
                    result[i * 4 - 1] = representations[i - 1].Width;
                }
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObstacleRepresentation))
                return false;

            ObstacleRepresentation or = (ObstacleRepresentation)obj;

            return this.Equals(or);
        }

        public bool Equals(ObstacleRepresentation representation)
        {
            if (this.X == representation.X && 
                this.Y == representation.Y &&
                this.Width == representation.Width &&
                this.Height == representation.Height)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Width.GetHashCode();
                hash = hash * 23 + Height.GetHashCode();
                return hash;
            }
        }

        public string ToString(string specificObstacleID)
        {
            return specificObstacleID + " Representation X: " + X + " Y: " + Y + " Width: " + Width + " Height: " + Height;
        }

        public override string ToString()
        {
            return ToString("Obstacle");
        }
    }
}
