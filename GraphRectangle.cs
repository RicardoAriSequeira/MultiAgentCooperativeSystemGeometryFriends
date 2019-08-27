using System.Collections.Generic;

using static GeometryFriendsAgents.GameInfo;
using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    class GraphRectangle : Graph
    {
        public GraphRectangle() : base(RECTANGLE_AREA, new int[3] { MIN_RECTANGLE_HEIGHT, SQUARE_HEIGHT, MAX_RECTANGLE_HEIGHT }, YELLOW) {}

        public override void SetupPlatforms()
        {
            platforms = PlatformIdentification.SetPlatforms_Rectangle(levelArray);
            platforms = PlatformIdentification.SetPlatformsID(platforms);
        }

        public override void SetupMoves()
        {
             MoveIdentification.Setup_Rectangle(this);
        }

        public override List<ArrayPoint> GetFormPixels(Point center, int height)
        {      
            return GetRectanglePixels(center, height);
        }

        public static List<ArrayPoint> GetRectanglePixels(Point center, int height)
        {
            ArrayPoint rectangleCenterArray = ConvertPointIntoArrayPoint(center, false, false);

            int rectangleHighestY = PointToArrayPoint(center.y - (height / 2), false);
            int rectangleLowestY = PointToArrayPoint(center.y + (height / 2), true);

            float rectangleWidth = RECTANGLE_AREA / height;
            int rectangleLeftX = PointToArrayPoint((int)(center.x - (rectangleWidth / 2)), false);
            int rectangleRightX = PointToArrayPoint((int)(center.x + (rectangleWidth / 2)), true);

            List<ArrayPoint> rectanglePixels = new List<ArrayPoint>();

            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }
    }
}
