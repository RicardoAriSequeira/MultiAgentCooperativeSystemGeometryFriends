using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class PlatformCircle : Platforms
    {
  
        public PlatformCircle() : base()
        {
            area = GameInfo.CIRCLE_AREA;
            min_height = GameInfo.MIN_CIRCLE_HEIGHT;
            max_height = GameInfo.MAX_CIRCLE_HEIGHT;
        }

        public override platformType[,] GetPlatformArray()
        {
            platformType[,] platformArray = new platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    LevelArray.Point circleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(x, y));
                    circleCenter.y -= GameInfo.CIRCLE_RADIUS;
                    List<LevelArray.ArrayPoint> circlePixels = GetCirclePixels(circleCenter, GameInfo.CIRCLE_RADIUS);

                    if (!IsObstacle_onPixels(circlePixels))
                    {
                        if (levelArray[y, x - 1] == LevelArray.BLACK || levelArray[y, x] == LevelArray.BLACK)
                        {
                            platformArray[y, x] = platformType.BLACK;
                        }
                        else if (levelArray[y, x - 1] == LevelArray.GREEN || levelArray[y, x] == LevelArray.GREEN)
                        {
                            platformArray[y, x] = platformType.GREEN;
                        }
                    }
                });
            }

            return platformArray;

        }

        public override void SetPlatformList()
        {
            platformType[,] platformArray = GetPlatformArray();

            Parallel.For(0, levelArray.GetLength(0), i =>
            {
                platformType currentPlatform = platformType.NO_PLATFORM;
                int leftEdge = 0;

                for (int j = 0; j < platformArray.GetLength(1); j++)
                {
                    if (currentPlatform == platformType.NO_PLATFORM)
                    {
                        if (platformArray[i, j] == platformType.BLACK || platformArray[i, j] == platformType.GREEN)
                        {
                            leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(j);
                            currentPlatform = platformArray[i, j];
                        }
                    }
                    else
                    {
                        if (platformArray[i, j] != currentPlatform)
                        {
                            int rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(j - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platformList)
                                {
                                    platformList.Add(new Platform(platformType.BLACK, LevelArray.ConvertValue_ArrayPointIntoPoint(i), leftEdge, rightEdge, new List<Move>(), max_height / LevelArray.PIXEL_LENGTH));
                                }
                            }

                            currentPlatform = platformArray[i, j];
                        }
                    }
                }
            });

            SetPlatformID();
        }

        protected override void SetMoveInfoList_Jump(Platform fromPlatform, int velocityX)
        {
            int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);
            int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);
            Parallel.For(0, (to - from) / (LevelArray.PIXEL_LENGTH * 2) + 1, j =>
            {
                LevelArray.Point movePoint = new LevelArray.Point(from + j * LevelArray.PIXEL_LENGTH * 2, fromPlatform.height - GameInfo.CIRCLE_RADIUS);
                SetMoves_Fall(fromPlatform, movePoint, max_height, velocityX, true, movementType.JUMP);
                SetMoves_Fall(fromPlatform, movePoint, max_height, velocityX, false, movementType.JUMP);
            });
        }

        protected override void SetMoves_StairOrGap(Platform fromPlatform)
        {

            foreach (Platform toPlatform in platformList)
            {

                bool rightMove = false;
                bool obstacleFlag = false;
                bool[] collectible_onPath = new bool[nCollectibles];

                if (fromPlatform.Equals(toPlatform) || !IsStairOrGap(fromPlatform, toPlatform, ref rightMove))
                {
                    continue;
                }

                int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;

                for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                {
                    List<LevelArray.ArrayPoint> circlePixels = GetCirclePixels(new LevelArray.Point(k, toPlatform.height - GameInfo.CIRCLE_RADIUS), GameInfo.CIRCLE_RADIUS);

                    if (IsObstacle_onPixels(circlePixels))
                    {
                        obstacleFlag = true;
                        break;
                    }

                    collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(circlePixels));
                }

                if (!obstacleFlag)
                {
                    LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.leftEdge, toPlatform.height) : new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);

                    AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                }
            }
        }

        protected override List<LevelArray.ArrayPoint> GetFormPixels(LevelArray.Point center, int height)
        {
            return GetCirclePixels(center, height/2);
        }

        protected override bool IsObstacle_onPixels(List<LevelArray.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelArray.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelArray.BLACK || levelArray[i.yArray, i.xArray] == LevelArray.GREEN)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
