using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class GraphCircle : Graph
    {
  
        public GraphCircle() : base()
        {
            area = GameInfo.CIRCLE_AREA;
            min_height = GameInfo.MIN_CIRCLE_HEIGHT;
            max_height = GameInfo.MAX_CIRCLE_HEIGHT;
        }

        public platformType[,] GetPlatformArray()
        {
            platformType[,] platformArray = new platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    LevelRepresentation.Point circleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                    circleCenter.y -= GameInfo.CIRCLE_RADIUS;
                    List<LevelRepresentation.ArrayPoint> circlePixels = GetCirclePixels(circleCenter, GameInfo.CIRCLE_RADIUS);

                    if (!IsObstacle_onPixels(circlePixels))
                    {
                        if (levelArray[y, x - 1] == LevelRepresentation.BLACK || levelArray[y, x] == LevelRepresentation.BLACK)
                        {
                            platformArray[y, x] = platformType.BLACK;
                        }
                        else if (levelArray[y, x - 1] == LevelRepresentation.GREEN || levelArray[y, x] == LevelRepresentation.GREEN)
                        {
                            platformArray[y, x] = platformType.GREEN;
                        }
                    }
                });
            }

            return platformArray;

        }

        public override void SetupPlatforms()
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
                            leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(j);
                            currentPlatform = platformArray[i, j];
                        }
                    }
                    else
                    {
                        if (platformArray[i, j] != currentPlatform)
                        {
                            int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(j - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platforms)
                                {
                                    platforms.Add(new Platform(platformType.BLACK, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(i), leftEdge, rightEdge, new List<Move>(), max_height / LevelRepresentation.PIXEL_LENGTH));
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
            int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);
            int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);
            Parallel.For(0, (to - from) / (LevelRepresentation.PIXEL_LENGTH * 2) + 1, j =>
            {
                LevelRepresentation.Point movePoint = new LevelRepresentation.Point(from + j * LevelRepresentation.PIXEL_LENGTH * 2, fromPlatform.height - GameInfo.CIRCLE_RADIUS);
                SetMoves_Fall(fromPlatform, movePoint, max_height, velocityX, true, movementType.JUMP);
                SetMoves_Fall(fromPlatform, movePoint, max_height, velocityX, false, movementType.JUMP);
            });
        }

        protected override void SetMoves_StairOrGap(Platform fromPlatform)
        {

            foreach (Platform toPlatform in platforms)
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

                for (int k = from; k <= to; k += LevelRepresentation.PIXEL_LENGTH)
                {
                    List<LevelRepresentation.ArrayPoint> circlePixels = GetCirclePixels(new LevelRepresentation.Point(k, toPlatform.height - GameInfo.CIRCLE_RADIUS), GameInfo.CIRCLE_RADIUS);

                    if (IsObstacle_onPixels(circlePixels))
                    {
                        obstacleFlag = true;
                        break;
                    }

                    collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(circlePixels));
                }

                if (!obstacleFlag)
                {
                    LevelRepresentation.Point movePoint = rightMove ? new LevelRepresentation.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelRepresentation.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelRepresentation.Point landPoint = rightMove ? new LevelRepresentation.Point(toPlatform.leftEdge, toPlatform.height) : new LevelRepresentation.Point(toPlatform.rightEdge, toPlatform.height);

                    AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                }
            }
        }

        protected override List<LevelRepresentation.ArrayPoint> GetFormPixels(LevelRepresentation.Point center, int height)
        {
            return GetCirclePixels(center, height/2);
        }

        protected override bool IsObstacle_onPixels(List<LevelRepresentation.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelRepresentation.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelRepresentation.BLACK || levelArray[i.yArray, i.xArray] == LevelRepresentation.GREEN)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
