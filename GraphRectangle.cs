using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;

using static GeometryFriendsAgents.GameInfo;
using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    class GraphRectangle : Graph
    {

        public GraphRectangle() : base()
        {
            AREA = RECTANGLE_AREA;
            MIN_HEIGHT = MIN_RECTANGLE_HEIGHT;
            MAX_HEIGHT = MAX_RECTANGLE_HEIGHT;
        }

        public override void SetupPlatforms()
        {
            platforms = PlatformIdentification.SetPlatforms_Rectangle(levelArray);
            platforms = PlatformIdentification.SetPlatformsID(platforms);
        }

        public override void SetupMoves()
        {
             MoveIdentification.Setup_Rectangle(this);
        }

        public void SetPossibleCollectibles(RectangleRepresentation initial_rI)
        {

            bool[] platformsChecked = new bool[platforms.Count];

            Platform? platform = GetPlatform(new Point((int)initial_rI.X, (int)initial_rI.Y), SQUARE_HEIGHT);

            if (platform.HasValue)
            {
                platformsChecked = CheckCollectiblesPlatform(platformsChecked, platform.Value);
            }
        }

        public override List<ArrayPoint> GetFormPixels(Point center, int height)
        {
            ArrayPoint rectangleCenterArray = ConvertPointIntoArrayPoint(center, false, false);

            int rectangleHighestY = ConvertValue_PointIntoArrayPoint(center.y - (height / 2), false);
            int rectangleLowestY = ConvertValue_PointIntoArrayPoint(center.y + (height / 2), true);

            float rectangleWidth = RECTANGLE_AREA / height;
            int rectangleLeftX = ConvertValue_PointIntoArrayPoint((int)(center.x - (rectangleWidth / 2)), false);
            int rectangleRightX = ConvertValue_PointIntoArrayPoint((int)(center.x + (rectangleWidth / 2)), true);

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

        private bool CheckEmptyGap(Platform fromPlatform, Platform toPlatform)
        {

            int from = ConvertValue_PointIntoArrayPoint(fromPlatform.rightEdge, false) + 1;
            int to = ConvertValue_PointIntoArrayPoint(toPlatform.leftEdge, true);

            int y = ConvertValue_PointIntoArrayPoint(fromPlatform.height, false);

            for (int i = from; i <= to; i++)
            {
                if (levelArray[y, i] == BLACK)
                {
                    return false;
                }
            }

            return true;
        }

        private List<Platform> PlatformsBelow(Point center, int height)
        {

            List<Platform> platforms = new List<Platform>();

            int rectangleWidth = RECTANGLE_AREA / height;
            int rectangleLeftX = center.x - (rectangleWidth / 2);
            int rectangleRightX = center.x + (rectangleWidth / 2);

            foreach (Platform i in platforms)
            {
                if (i.leftEdge <= rectangleLeftX && rectangleLeftX <= i.rightEdge && 0 <= (i.height - center.y) && (i.height - center.y) <= height)
                {
                    platforms.Add(i);
                }
                else if (i.leftEdge <= rectangleRightX && rectangleRightX <= i.rightEdge && 0 <= (i.height - center.y) && (i.height - center.y) <= height)
                {
                    platforms.Add(i);
                }
            }

            return platforms;
        }

        protected override collideType GetCollideType(Point center, bool ascent, bool rightMove, int radius)
        {
            ArrayPoint centerArray = ConvertPointIntoArrayPoint(center, false, false);
            int highestY = ConvertValue_PointIntoArrayPoint(center.y - radius, false);
            int lowestY = ConvertValue_PointIntoArrayPoint(center.y + radius, true);

            if (!ascent)
            {
                if (levelArray[lowestY, centerArray.xArray] == BLACK || levelArray[lowestY, centerArray.xArray] == YELLOW)
                {
                    return collideType.FLOOR;
                }

            }
            else
            {
                if (levelArray[highestY, centerArray.xArray] == BLACK || levelArray[lowestY, centerArray.xArray] == YELLOW)
                {
                    return collideType.CEILING;
                }
            }

            return collideType.OTHER;
        }

        public override bool IsObstacle_onPixels(List<ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == BLACK || levelArray[i.yArray, i.xArray] == YELLOW)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
