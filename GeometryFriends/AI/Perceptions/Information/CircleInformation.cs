
namespace GeometryFriends.AI.Perceptions.Information
{
    /// <summary>
    /// A minimal representation of a circle.
    /// </summary>
    public struct CircleRepresentation
    {
        /// <summary>
        /// X position of the circle.
        /// </summary>
        public float X;
        /// <summary>
        /// Y position of the circle.
        /// </summary>
        public float Y;
        /// <summary>
        /// Velocity in the X axis of the circle.
        /// </summary>
        public float VelocityX;
        /// <summary>
        /// Velocity in the Y axis of the circle.
        /// </summary>
        public float VelocityY;
        /// <summary>
        /// Radius of the circle.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Constructor for a circle representation.
        /// </summary>
        /// <param name="x">X position of the circle.</param>
        /// <param name="y">Y position of the circle.</param>
        /// <param name="velX">Velocity in the X axis of the circle.</param>
        /// <param name="velY">Velocity in the Y axis of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        public CircleRepresentation(float x, float y, float velX, float velY, float radius)
        {
            X = x;
            Y = y;
            VelocityX = velX;
            VelocityY = velY;
            Radius = radius;
        }

        /// <summary>
        /// An array representation of the circle representation.
        /// </summary>
        /// <returns>Array containing the circle representation (X, Y, VelocityX, VelocityY, Radius).</returns>
        public float[] ToArray()
        {
            return new float[5] {
                X,
                Y,
                VelocityX,
                VelocityY,
                Radius
            };
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CircleRepresentation))
                return false;

            CircleRepresentation cr = (CircleRepresentation)obj;

            return this.Equals(cr);
        }

        public bool Equals(CircleRepresentation representation)
        {
            if (this.X == representation.X &&
                this.Y == representation.Y &&
                this.VelocityX == representation.VelocityX &&
                this.VelocityY == representation.VelocityY &&
                this.Radius == representation.Radius)
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
                hash = hash * 23 + Radius.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "Circle Representation X: " + X + " Y: " + Y + " VelX: " + VelocityX + " VelY: " + VelocityY + " Radius: " + Radius;
        }
    }
}
