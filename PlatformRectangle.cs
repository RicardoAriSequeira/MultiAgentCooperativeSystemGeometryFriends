using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class PlatformRectangle : Platform
    {

        public PlatformRectangle() : base()  { }

        public override int[,] IdentifyObstacles(int[,] levelArray)
        {

            int[,] platformArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int i = 0; i < levelArray.GetLength(0); i++)
            {
                Parallel.For(0, levelArray.GetLength(1), j =>
                {
                    LevelArray.Point rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(j, i));
                    rectangleCenter.y -= 50;

                    List<LevelArray.ArrayPoint> rectanglePixels = GetRectanglePixels(rectangleCenter);

                    if (!IsObstacle_onPixels(levelArray, rectanglePixels))
                    {
                        if (levelArray[i, j - 1] == LevelArray.OBSTACLE || levelArray[i, j] == LevelArray.OBSTACLE)
                        {
                            platformArray[i, j] = LevelArray.OBSTACLE;
                        }
                    }
                });
            }

            return platformArray;

        }

        public override void SetMoveInfoList(int[,] levelArray, int numCollectibles)
        {

            SetMoveInfoList_StairOrGap(levelArray, numCollectibles);

            foreach (Platform.PlatformInfo i in platformInfoList)
            {

                movementType movementType;
                movementType = movementType.JUMP;

                int from = i.leftEdge + (i.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);
                int to = i.rightEdge - (i.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);

                Parallel.For(0, (to - from) / (LevelArray.PIXEL_LENGTH * 2) + 1, j =>
                {
                    LevelArray.Point movePoint;

                    movePoint = new LevelArray.Point(from + j * LevelArray.PIXEL_LENGTH * 2, i.height - GameInfo.RECTANGLE_RADIUS);

                    SetMoveInfoList_NoAction(levelArray, i, movePoint, numCollectibles);

                    Parallel.For(0, (GameInfo.MAX_VELOCITYX / VELOCITYX_STEP), k =>
                    {
                        bool rightMove;
                        int velocityX;

                        velocityX = VELOCITYX_STEP * k;

                        rightMove = true;

                        SetMoveInfoList_JumpOrFall(levelArray, i, movePoint, velocityX, rightMove, movementType, numCollectibles);

                        rightMove = false;

                        SetMoveInfoList_JumpOrFall(levelArray, i, movePoint, velocityX, rightMove, movementType, numCollectibles);
                    });
                });

                movementType = movementType.FALL;

                Parallel.For(0, 10, k =>
                {
                    LevelArray.Point movePoint;
                    bool rightMove;
                    int velocityX;

                    velocityX = VELOCITYX_STEP * k;

                    movePoint = new LevelArray.Point(i.leftEdge - LevelArray.PIXEL_LENGTH, i.height - GameInfo.RECTANGLE_RADIUS);

                    rightMove = false;

                    SetMoveInfoList_JumpOrFall(levelArray, i, movePoint, velocityX, rightMove, movementType, numCollectibles);

                    movePoint = new LevelArray.Point(i.rightEdge + LevelArray.PIXEL_LENGTH, i.height - GameInfo.RECTANGLE_RADIUS);

                    rightMove = true;

                    SetMoveInfoList_JumpOrFall(levelArray, i, movePoint, velocityX, rightMove, movementType, numCollectibles);

                });
            }
        }

        private void SetMoveInfoList_NoAction(int[,] levelArray, Platform.PlatformInfo fromPlatform, LevelArray.Point movePoint, int numCollectibles)
        {
            List<LevelArray.ArrayPoint> rectanglePixels = GetRectanglePixels(movePoint);

            bool[] collectible_onPath = new bool[numCollectibles];

            collectible_onPath = GetCollectibles_onPixels(levelArray, rectanglePixels, collectible_onPath.Length);

            AddMoveInfoToList(fromPlatform, new Platform.MoveInfo(fromPlatform, movePoint, movePoint, 0, true, Platform.movementType.NO_ACTION, collectible_onPath, 0, false));
        }

        private void SetMoveInfoList_StairOrGap(int[,] levelArray, int numCollectibles)
        {
            foreach (PlatformInfo i in platformInfoList)
            {
                foreach (PlatformInfo j in platformInfoList)
                {
                    if (i.Equals(j))
                    {
                        continue;
                    }

                    bool rightMove = false;

                    if (!IsStairOrGap(i, j, ref rightMove))
                    {
                        continue;
                    }

                    bool obstacleFlag = false;
                    bool[] collectible_onPath = new bool[numCollectibles];

                    int from = rightMove ? i.rightEdge : j.rightEdge;
                    int to = rightMove ? j.leftEdge : i.leftEdge;

                    for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                    {
                        List<LevelArray.ArrayPoint> rectanglePixels = GetRectanglePixels(new LevelArray.Point(k, j.height - 50));

                        if (IsObstacle_onPixels(levelArray, rectanglePixels))
                        {
                            obstacleFlag = true;
                            break;
                        }

                        collectible_onPath = GetCollectibles_onPixels(levelArray, rectanglePixels, numCollectibles);
                    }

                    if (!obstacleFlag)
                    {
                        LevelArray.Point movePoint = rightMove ? new LevelArray.Point(i.rightEdge, i.height) : new LevelArray.Point(i.leftEdge, i.height);
                        LevelArray.Point landPoint = rightMove ? new LevelArray.Point(j.leftEdge, j.height) : new LevelArray.Point(j.rightEdge, j.height);

                        AddMoveInfoToList(i, new MoveInfo(j, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (i.height - j.height) + Math.Abs(movePoint.x - landPoint.x), false));
                    }
                }
            }
        }

        private void SetMoveInfoList_JumpOrFall(int[,] levelArray, PlatformInfo fromPlatform, LevelArray.Point movePoint, int velocityX, bool rightMove, movementType movementType, int numCollectibles)
        {
            if (!IsEnoughLengthToAccelerate(fromPlatform, movePoint, velocityX, rightMove))
            {
                return;
            }

            bool[] collectible_onPath = new bool[numCollectibles];
            float pathLength = 0;

            LevelArray.Point collidePoint = movePoint;
            LevelArray.Point prevCollidePoint;

            collideType collideType = collideType.OTHER;
            float collideVelocityX = rightMove ? velocityX : -velocityX;
            float collideVelocityY = (movementType == movementType.JUMP) ? GameInfo.JUMP_VELOCITYY : GameInfo.FALL_VELOCITYY;
            bool collideCeiling = false;

            do
            {
                prevCollidePoint = collidePoint;

                GetPathInfo(levelArray, collidePoint, collideVelocityX, collideVelocityY,
                    ref collidePoint, ref collideType, ref collideVelocityX, ref collideVelocityY, ref collectible_onPath, ref pathLength, GameInfo.RECTANGLE_RADIUS);

                if (collideType == collideType.CEILING)
                {
                    collideCeiling = true;
                }

                if (prevCollidePoint.Equals(collidePoint))
                {
                    break;
                }
            }
            while (!(collideType == collideType.FLOOR));

            if (collideType == collideType.FLOOR)
            {

                PlatformInfo? toPlatform = GetPlatform_onRectangle(collidePoint, 100);

                if (toPlatform.HasValue)
                {
                    if (movementType == movementType.FALL)
                    {
                        movePoint.x = rightMove ? movePoint.x - LevelArray.PIXEL_LENGTH : movePoint.x + LevelArray.PIXEL_LENGTH;
                    }

                    AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform.Value, movePoint, collidePoint, velocityX, rightMove, movementType, collectible_onPath, (int)pathLength, collideCeiling));
                }
            }
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

        private List<LevelArray.ArrayPoint> GetRectanglePixels(LevelArray.Point rectangleCenter)
        {
            List<LevelArray.ArrayPoint> rectanglePixels = new List<LevelArray.ArrayPoint>();

            LevelArray.ArrayPoint rectangleCenterArray = LevelArray.ConvertPointIntoArrayPoint(rectangleCenter, false, false);
            int rectangleHighestY = LevelArray.ConvertValue_PointIntoArrayPoint(rectangleCenter.y - 50, false);
            int rectangleLowestY = LevelArray.ConvertValue_PointIntoArrayPoint(rectangleCenter.y + 50, true);


            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                float rectangleWidth = 100;

                int rectangleLeftX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(rectangleCenter.x - (rectangleWidth / 2)), false); //+ 1
                int rectangleRightX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(rectangleCenter.x + (rectangleWidth / 2)), true); //+ 1

                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new LevelArray.ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }
    }
}
