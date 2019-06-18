using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class PlatformCircle : Platform
    {
  
        public PlatformCircle() : base()
        {
            area = GameInfo.CIRCLE_AREA;
            min_height = GameInfo.MIN_CIRCLE_HEIGHT;
            max_height = GameInfo.MAX_CIRCLE_HEIGHT;
        }

        public override int[,] GetPlatformArray(int[,] levelArray)
        {
            int[,] platformArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {

                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    LevelArray.Point circleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(x, y));
                    circleCenter.y -= GameInfo.CIRCLE_RADIUS;
                    List<LevelArray.ArrayPoint> circlePixels = GetCirclePixels(circleCenter, GameInfo.CIRCLE_RADIUS);

                    if (!IsObstacle_onPixels(levelArray, circlePixels))
                    {
                        if (levelArray[y, x - 1] == LevelArray.OBSTACLE || levelArray[y, x] == LevelArray.OBSTACLE)
                        {
                            platformArray[y, x] = LevelArray.OBSTACLE;
                        }
                    }
                });
            }

            return platformArray;

        }

        public override void SetPlatformInfoList(int[,] levelArray)
        {
            int[,] platformArray = GetPlatformArray(levelArray);

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
                                platformInfoList.Add(new PlatformInfo(height, leftEdge, rightEdge, new List<MoveInfo>()));
                            }
                        }

                        platformFlag = false;
                    }
                }
            });

            SetPlatformID();
        }

        protected override void SetMoveInfoList_Jump(int[,] levelArray, int numCollectibles, PlatformInfo fromPlatform, int velocityX)
        {
            int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);
            int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);
            Parallel.For(0, (to - from) / (LevelArray.PIXEL_LENGTH * 2) + 1, j =>
            {
                LevelArray.Point movePoint = new LevelArray.Point(from + j * LevelArray.PIXEL_LENGTH * 2, fromPlatform.height - GameInfo.CIRCLE_RADIUS);
                SetMoveInfoList_Fall(levelArray, numCollectibles, fromPlatform, movePoint, velocityX, true, movementType.JUMP);
                SetMoveInfoList_Fall(levelArray, numCollectibles, fromPlatform, movePoint, velocityX, false, movementType.JUMP);
            });
        }

        protected override void SetMoveInfoList_StairOrGap(int[,] levelArray, int numCollectibles, PlatformInfo fromPlatform)
        {

            bool rightMove = false;
            bool obstacleFlag = false;
            bool[] collectible_onPath = new bool[numCollectibles];

            foreach (PlatformInfo toPlatform in platformInfoList)
            {
                if (fromPlatform.Equals(toPlatform) || !IsStairOrGap(fromPlatform, toPlatform, ref rightMove))
                {
                    continue;
                }

                int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;

                for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                {
                    List<LevelArray.ArrayPoint> circlePixels = GetCirclePixels(new LevelArray.Point(k, toPlatform.height - GameInfo.CIRCLE_RADIUS), GameInfo.CIRCLE_RADIUS);

                    if (IsObstacle_onPixels(levelArray, circlePixels))
                    {
                        obstacleFlag = true;
                        break;
                    }

                    collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(levelArray, circlePixels, numCollectibles));
                }

                if (!obstacleFlag)
                {
                    LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.leftEdge, toPlatform.height) : new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);

                    AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                }
            }
        }

        protected override List<LevelArray.ArrayPoint> GetFormPixels(LevelArray.Point center, int height)
        {
            return GetCirclePixels(center, height/2);
        }

    }
}
