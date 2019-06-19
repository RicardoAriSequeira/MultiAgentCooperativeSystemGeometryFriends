
namespace GeometryFriends.AI.Perceptions.Information
{
    /// <summary>
    /// A minimal representation of a rectangle.
    /// </summary>
    public struct RectangleRepresentation
    {
        /// <summary>
        /// X position of the rectangle.
        /// </summary>
        public float X;
        /// <summary>
        /// Y position of the rectangle.
        /// </summary>
        public float Y;
        /// <summary>
        /// Velocity in the X axis of the rectangle.
        /// </summary>
        public float VelocityX;
        /// <summary>
        /// Velocity in the Y axis of the rectangle.
        /// </summary>
        public float VelocityY;
        /// <summary>
        /// Height of the rectangle.
        /// </summary>
        public float Height;

        /// <summary>
        /// Constructor for a rectangle representation.
        /// </summary>
        /// <param name="x">X position of the rectangle.</param>
        /// <param name="y">Y position of the rectangle.</param>
        /// <param name="velX">Velocity in the X axis of the rectangle.</param>
        /// <param name="velY">Velocity in the Y axis of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        public RectangleRepresentation(float x, float y, float velX, float velY, float height)
        {
            X = x;
            Y = y;
            VelocityX = velX;
            VelocityY = velY;
            Height = height;
        }

        /// <summary>
        /// An array representation of the rectangle representation.
        /// </summary>
        /// <returns>Array containing the rectangle representation (X, Y, VelocityX, VelocityY, Height).</returns>
        public float[] ToArray()
        {
            return new float[5] {
                X,
                Y,
                VelocityX,
                VelocityY,
                Height
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RectangleRepresentation))
                return false;

            RectangleRepresentation rr = (RectangleRepresentation)obj;

            return this.Equals(rr);
        }

        public bool Equals(RectangleRepresentation representation)
        {
            if (this.X == representation.X &&
                this.Y == representation.Y &&
                this.VelocityX == representation.VelocityX &&
                this.VelocityY == representation.VelocityY &&
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
                hash = hash * 23 + VelocityX.GetHashCode();
                hash = hash * 23 + VelocityY.GetHashCode();
                hash = hash * 23 + Height.GetHashCode();
                return hash;
            }
        }
        public override string ToString()
        {
            return "Rectangle Representation X: " + X + " Y: " + Y + " VelX: " + VelocityX + " VelY: " + VelocityY + " Height: " + Height;
        }

    }
}
