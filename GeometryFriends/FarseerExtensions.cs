using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Mathematics;
using System;

namespace GeometryFriends
{
    //HACK TO REPLICATE PREVIOUSLY EXISTING FUNCTIONALITY NOT PRESENT ON THE FRAMEWORK
    static class FarseerExtensions
    {
        public static Vector2 GetRectangleCorner(this Geom self, int cornerIndex)
        {
            if (cornerIndex == 1)
                return self.WorldVertices[0];
            else if (cornerIndex == 2)
                return self.WorldVertices[12];
            else if (cornerIndex == 3)
                return self.WorldVertices[4];
            else if (cornerIndex == 4)
                return self.WorldVertices[8];
            else
            {
                Log.LogRaw("Should not ask for this corner! Rectangles only have 4 corners.");
                throw new Exception("Should not ask for this corner! Rectangles only have 4 corners.");
            }
        }
    }
}
