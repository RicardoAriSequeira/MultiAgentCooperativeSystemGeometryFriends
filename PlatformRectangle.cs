using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class PlatformRectangle : Platform
    {

        public PlatformRectangle() : base()  { }

        public override void SetPlatformInfoList(int[,] levelArray)
        {

            int[,] platformArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int i = 0; i < levelArray.GetLength(0); i++)
            {
                Parallel.For(0, levelArray.GetLength(1), j =>
                {

                    foreach (int height in RECTANGLE_HEIGHTS)
                    {
                        LevelArray.Point rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(j, i));
                        rectangleCenter.y -= (height / 2);
                        List<LevelArray.ArrayPoint> rectanglePixels = GetRectanglePixels(rectangleCenter, height);

                        if (!IsObstacle_onPixels(levelArray, rectanglePixels))
                        {
                            if (levelArray[i, j - 1] == LevelArray.OBSTACLE || levelArray[i, j] == LevelArray.OBSTACLE)
                            {
                                platformArray[i, j] = LevelArray.OBSTACLE;
                            }
                        }
                    }
                });
            }

            Parallel.For(0, levelArray.GetLength(0), i =>
            {
                bool platformFlag = false;
                int height = 0, leftEdge = 0, rightEdge = 0;

                for (int j = 0; j < platformArray.GetLength(1); j++)
                {
                    if (platformArray[i, j] == LevelArray.OBSTACLE && !platformFlag)
                    {
                        height = LevelArray.ConvertValue_ArrayPointIntoPoint(i);
                        leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(j);
                        platformFlag = true;
                    }

                    if (platformArray[i, j] == LevelArray.OPEN && platformFlag)
                    {
                        rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(j - 1);

                        if (rightEdge >= leftEdge)
                        {
                            lock (platformInfoList)
                            {
                                platformInfoList.Add(new Platform.PlatformInfo(0, height, leftEdge, rightEdge, new List<Platform.MoveInfo>()));
                            }
                        }

                        platformFlag = false;
                    }
                }
            });

            SetPlatformID();

        }

        private List<LevelArray.ArrayPoint> GetRectanglePixels(LevelArray.Point rectangleCenter, int height)
        {
            List<LevelArray.ArrayPoint> rectanglePixels = new List<LevelArray.ArrayPoint>();

            LevelArray.ArrayPoint rectangleCenterArray = LevelArray.ConvertPointIntoArrayPoint(rectangleCenter, false, false);
            int rectangleHighestY = LevelArray.ConvertValue_PointIntoArrayPoint(rectangleCenter.y - (height / 2), false);
            int rectangleLowestY = LevelArray.ConvertValue_PointIntoArrayPoint(rectangleCenter.y + (height / 2), true);


            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                float rectangleWidth = RECTANGLE_AREA / height;

                int rectangleLeftX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(rectangleCenter.x - (rectangleWidth / 2)), false); //+ 1
                int rectangleRightX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(rectangleCenter.x + (rectangleWidth / 2)), true); //+ 1

                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new LevelArray.ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }

        public override void SetMoveInfoList(int[,] levelArray, int numCollectibles)
        {
            foreach (Platform.PlatformInfo i in platformInfoList)
            {

                int from = i.leftEdge + (i.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);
                int to = i.rightEdge - (i.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);

                Parallel.For(0, (to - from) / (LevelArray.PIXEL_LENGTH * 2) + 1, j =>
                {
                    LevelArray.Point movePoint;

                    movePoint = new LevelArray.Point(from + j * LevelArray.PIXEL_LENGTH * 2, i.height - GameInfo.CIRCLE_RADIUS);

                    SetMoveInfoList_NoAction(levelArray, i, movePoint, numCollectibles);
                });
            }
        }

        private void SetMoveInfoList_NoAction(int[,] levelArray, Platform.PlatformInfo fromPlatform, LevelArray.Point movePoint, int numCollectibles)
        {
            List<LevelArray.ArrayPoint> rectanglePixels = GetRectanglePixels(movePoint, 100);

            bool[] collectible_onPath = new bool[numCollectibles];

            collectible_onPath = GetCollectibles_onPixels(levelArray, rectanglePixels, collectible_onPath.Length);

            AddMoveInfoToList(fromPlatform, new Platform.MoveInfo(fromPlatform, movePoint, movePoint, 0, true, Platform.movementType.NO_ACTION, collectible_onPath, 0, false));
        }

        public Platform.PlatformInfo? GetPlatform_onRectangle(LevelArray.Point rectangleCenter, float height)
        {
            foreach (Platform.PlatformInfo i in platformInfoList)
            {
                if (i.leftEdge <= rectangleCenter.x && rectangleCenter.x <= i.rightEdge && 0 <= (i.height - rectangleCenter.y) && (i.height - rectangleCenter.y) <= height)
                {
                    return i;
                }
            }

            return null;
        }
    }
}
