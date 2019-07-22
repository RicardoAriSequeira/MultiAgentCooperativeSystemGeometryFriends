using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class GraphRectangle : Graph
    {

        public GraphRectangle() : base()
        {
            this.AREA = GameInfo.RECTANGLE_AREA;
            this.MIN_HEIGHT = GameInfo.MIN_RECTANGLE_HEIGHT;
            this.MAX_HEIGHT = GameInfo.MAX_RECTANGLE_HEIGHT;
        }

        public override void SetupPlatforms()
        {
            //List<Platform> platformsCircle = PlatformIdentification.SetPlatformsAsCircle(levelArray);
            //List<Platform> platformsRectangle = PlatformIdentification.SetPlatformsAsRectangle(levelArray);
            platforms = PlatformIdentification.SetPlatforms_Rectangle(levelArray);
            //platforms = PlatformIdentification.JoinPlatforms(platformsRectangle, platformsCircle);

            platforms = PlatformIdentification.SetPlatformsID(platforms);
        }

        public override void SetupMoves()
        {
            foreach (Platform fromPlatform in platforms)
            {

                MoveIdentification.StairOrGap_Rectangle(this, fromPlatform);

                if (fromPlatform.type != platformType.GAP)
                {
                    MoveIdentification.Fall(this, fromPlatform);
                    MoveIdentification.Morph(this, fromPlatform);
                    MoveIdentification.Collect(this, fromPlatform);
                }

            }
        }

        public override List<LevelRepresentation.ArrayPoint> GetFormPixels(LevelRepresentation.Point center, int height)
        {
            LevelRepresentation.ArrayPoint rectangleCenterArray = LevelRepresentation.ConvertPointIntoArrayPoint(center, false, false);

            int rectangleHighestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y - (height/2), false);
            int rectangleLowestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y + (height/2), true);

            float rectangleWidth = GameInfo.RECTANGLE_AREA / height;
            int rectangleLeftX = LevelRepresentation.ConvertValue_PointIntoArrayPoint((int)(center.x - (rectangleWidth / 2)), false);
            int rectangleRightX = LevelRepresentation.ConvertValue_PointIntoArrayPoint((int)(center.x + (rectangleWidth / 2)), true);

            List<LevelRepresentation.ArrayPoint> rectanglePixels = new List<LevelRepresentation.ArrayPoint>();

            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new LevelRepresentation.ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }

        private bool CheckEmptyGap(Platform fromPlatform, Platform toPlatform)
        {

            int from = LevelRepresentation.ConvertValue_PointIntoArrayPoint(fromPlatform.rightEdge, false) + 1;
            int to = LevelRepresentation.ConvertValue_PointIntoArrayPoint(toPlatform.leftEdge, true);
           
            int y = LevelRepresentation.ConvertValue_PointIntoArrayPoint(fromPlatform.height, false);

            for (int i = from; i <= to; i++)
            {
                if (levelArray[y, i] == LevelRepresentation.BLACK)
                {
                    return false;
                }
            }

            return true;
        }

        private List<Platform> PlatformsBelow(LevelRepresentation.Point center, int height)
        {

            List<Platform> platforms = new List<Platform>();

            int rectangleWidth = GameInfo.RECTANGLE_AREA / height;
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

        public override bool IsObstacle_onPixels(List<LevelRepresentation.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelRepresentation.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelRepresentation.BLACK || levelArray[i.yArray, i.xArray] == LevelRepresentation.YELLOW)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
