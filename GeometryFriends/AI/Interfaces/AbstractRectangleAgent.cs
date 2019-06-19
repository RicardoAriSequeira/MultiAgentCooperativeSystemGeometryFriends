using GeometryFriends.AI.Perceptions.Information;
using System;

namespace GeometryFriends.AI.Interfaces
{
    public abstract class AbstractRectangleAgent : AbstractAgent
    {
        /// <summary>
        /// Update the sensors of the rectangle agent.
        /// </summary>
        /// <param name="nC">The number of collectibles that remain uncatched</param>
        /// <param name="rI">Updated rectangle information (X, Y, VelocityX, VelocityY, Height)</param>
        /// <param name="cI">Updated circle information (X, Y, VelocityX, VelocityY, Radius)</param>
        /// <param name="colI">Updated collectibles to catch information (X,Y)</param>
        public abstract void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI);

        /// <summary>
        /// Update the sensors of the rectangle agent.
        /// </summary>
        /// <param name="nC">The number of collectibles that remain uncatched</param>
        /// <param name="sI">Updated rectangle information (X, Y, VelocityX, VelocityY, Height)</param>
        /// <param name="cI">Updated circle information (X, Y, VelocityX, VelocityY, Radius)</param>
        /// <param name="colI">Updated collectibles to catch information (X,Y)</param>
        [Obsolete("SensorsUpdated(int nC, float[] sI, float[] cI, float[] colI) is deprecated, please use SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI) instead.", true)]
        public virtual void SensorsUpdated(int nC, float[] sI, float[] cI, float[] colI) { }
        
        /// <summary>
        /// Update the sensors of the rectangle agent.
        /// </summary>
        /// <param name="nC">The number of collectibles that remain uncatched</param>
        /// <param name="sI">Updated rectangle information (X, Y, VelocityX, VelocityY, Height)</param>
        /// <param name="cI">Updated circle information (X, Y, VelocityX, VelocityY, Radius)</param>
        /// <param name="colI">Updated collectibles to catch information (X,Y)</param>
        [Obsolete("UpdateSensors(int nC, float[] sI, float[] cI, float[] colI) is deprecated, please use SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI) instead.", true)]
        public virtual void UpdateSensors(int nC, float[] sI, float[] cI, float[] colI) { }
    }
}
