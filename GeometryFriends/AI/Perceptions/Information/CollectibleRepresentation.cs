
namespace GeometryFriends.AI.Perceptions.Information
{
    /// <summary>
    /// A minimal representation of a collectible.
    /// </summary>
    public struct CollectibleRepresentation
    {
        /// <summary>
        /// X position of the collectible.
        /// </summary>
        public float X;
        /// <summary>
        /// Y position of the collectible.
        /// </summary>
        public float Y;

        /// <summary>
        /// Constructor for a collectible representation.
        /// </summary>
        /// <param name="x">X position of the collectible.</param>
        /// <param name="y">Y position of the collectible.</param>
        public CollectibleRepresentation(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// An array representation of the collectible representation.
        /// </summary>
        /// <returns>Array containing the collectible representation (X, Y).</returns>
        public float[] ToArray()
        {
            return new float[2] {
                X,
                Y
            };
        }

        /// <summary>
        /// Converts an array of collectible representations into a float array (following the obsolete specification)
        /// </summary>
        /// <returns>The float[] containing the obselete representation of the collectibles.</returns>
        public static float[] RepresentationArrayToFloatArray(CollectibleRepresentation[] representations)
        {
            if (representations.Length == 0)
                return new float[2] { 0, 0 };
            else
            {
                float[] result = new float[representations.Length * 2];
                for (int i = 1; i <= representations.Length; i++)
                {
                    result[i * 2 - 2] = representations[i - 1].X;
                    result[i * 2 - 1] = representations[i - 1].Y;
                }
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CollectibleRepresentation))
                return false;

            CollectibleRepresentation cr = (CollectibleRepresentation)obj;

            return this.Equals(cr);
        }

        public bool Equals(CollectibleRepresentation representation)
        {
            if (this.X == representation.X && this.Y == representation.Y)
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
                return hash;
            }
        }

        public override string ToString()
        {
            return "Collectible Representation X: " + X + " Y: " + Y;
        }
    }
}
