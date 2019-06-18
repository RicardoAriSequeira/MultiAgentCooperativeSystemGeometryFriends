using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class PlatformRectangle : Platform
    {

        public PlatformRectangle() : base()
        {
            area = GameInfo.RECTANGLE_AREA;
            min_height = GameInfo.MIN_RECTANGLE_HEIGHT;
            max_height = GameInfo.MAX_RECTANGLE_HEIGHT;
        }

        public override int[,] GetPlatformArray(int[,] levelArray)
        {
            int[,] platformArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    // RECTANGLE WITH HEIGHT 100
                    LevelArray.Point rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(x, y));
                    rectangleCenter.y -= GameInfo.SQUARE_HEIGHT / 2;
                    List<LevelArray.ArrayPoint> rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.SQUARE_HEIGHT);

                    if (IsObstacle_onPixels(levelArray, rectanglePixels))
                    {

                        // RECTANGLE WITH HEIGHT 50
                        rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(x, y));
                        rectangleCenter.y -= GameInfo.MIN_RECTANGLE_HEIGHT / 2;
                        rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.MIN_RECTANGLE_HEIGHT);

                        if (IsObstacle_onPixels(levelArray, rectanglePixels))
                        {

                            // RECTANGLE WITH HEIGHT 200
                            rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(x, y));
                            rectangleCenter.y -= GameInfo.MAX_RECTANGLE_HEIGHT / 2;
                            rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.MAX_RECTANGLE_HEIGHT);

                            if (IsObstacle_onPixels(levelArray, rectanglePixels))
                            {
                                return;
                            }
                        }
                    }

                    // if there is no obstacles but there is one below the rectangle, we add this point as platform
                    //if (levelArray[y, x - 1] == LevelArray.OBSTACLE || levelArray[y, x] == LevelArray.OBSTACLE)
                    if (levelArray[y, x] == LevelArray.OBSTACLE)
                    {
                        platformArray[y, x] = LevelArray.OBSTACLE;
                    }

                });
            }

            return platformArray;

        }

        public override void SetPlatformInfoList(int[,] levelArray)
        {
            int[,] platformArray = GetPlatformArray(levelArray);

            Parallel.For(0, levelArray.GetLength(0), y =>
            {
                bool platformFlag = false;
                int height = 0, leftEdge = 0, rightEdge = 0;

                int MIN_H = (GameInfo.MIN_RECTANGLE_HEIGHT / LevelArray.PIXEL_LENGTH);
                int MAX_H = (GameInfo.MAX_RECTANGLE_HEIGHT / LevelArray.PIXEL_LENGTH);
                int allowedHeight = MAX_H;

                for (int x = 0; x < platformArray.GetLength(1); x++)
                {
                    if (!platformFlag && platformArray[y, x] == LevelArray.OBSTACLE)
                    {
                        platformFlag = true;
                        allowedHeight = MAX_H;
                        height = LevelArray.ConvertValue_ArrayPointIntoPoint(y);
                        leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x);
                    }

                    else if (platformFlag)
                    {
                        if (platformArray[y, x] == LevelArray.OPEN)
                        {
                            rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platformInfoList)
                                {
                                    platformInfoList.Add(new PlatformInfo(height, leftEdge, rightEdge, new List<MoveInfo>(), allowedHeight));
                                }
                            }

                            allowedHeight = MAX_H;
                            platformFlag = false;
                            continue;
                        }

                        if (y > LevelArray.MARGIN + MIN_H)
                        {

                            for (int h = MIN_H; h <= MAX_H; h++)
                            {

                                if (levelArray[y - h, x] == LevelArray.OBSTACLE)
                                {

                                    if (h != allowedHeight)
                                    {

                                        rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);

                                        if (rightEdge >= leftEdge)
                                        {
                                            lock (platformInfoList)
                                            {
                                                platformInfoList.Add(new PlatformInfo(height, leftEdge, rightEdge, new List<MoveInfo>(), allowedHeight));
                                            }
                                        }

                                        allowedHeight = h;
                                        leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1); 
                                                                              
                                    }

                                    break;

                                }

                                else
                                {
                                    if (h == MAX_H && allowedHeight != MAX_H)
                                    {

                                        rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);

                                        if (rightEdge >= leftEdge)
                                        {
                                            lock (platformInfoList)
                                            {
                                                platformInfoList.Add(new PlatformInfo(height, leftEdge, rightEdge, new List<MoveInfo>(), allowedHeight));
                                            }
                                        }

                                        allowedHeight = MAX_H;
                                        leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);
                                    }
                                }

                            }
                        }
                    }
                                
                }
            });

            SetPlatformID();
        }

        protected override void SetMoveInfoList_StairOrGap(int[,] levelArray, int numCollectibles, PlatformInfo fromPlatform)
        {
            foreach (PlatformInfo toPlatform in platformInfoList)
            {
                if (fromPlatform.Equals(toPlatform))
                {
                    continue;
                }

                bool rightMove = false;
                bool obstacleFlag = false;
                bool[] collectible_onPath = new bool[numCollectibles];

                if (IsStairOrGap(fromPlatform, toPlatform, ref rightMove))
                {
                    int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                    int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;

                    for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                    {
                        List<LevelArray.ArrayPoint> rectanglePixels = GetCirclePixels(new LevelArray.Point(k, toPlatform.height - 50), GameInfo.SQUARE_HEIGHT);

                        if (IsObstacle_onPixels(levelArray, rectanglePixels))
                        {
                            obstacleFlag = true;
                            break;
                        }

                        collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(levelArray, rectanglePixels, numCollectibles));
                    }

                    if (!obstacleFlag)
                    {
                        LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                        LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.leftEdge, toPlatform.height) : new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);
                        AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                    }
                }

                else if (toPlatform.height >= fromPlatform.height - 90 && toPlatform.height <= fromPlatform.height)
                {
                    if (fromPlatform.rightEdge >= toPlatform.leftEdge - 33 && fromPlatform.rightEdge <= toPlatform.leftEdge)
                    {
                        LevelArray.Point movePoint = new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height);
                        LevelArray.Point landPoint = new LevelArray.Point(toPlatform.leftEdge, toPlatform.height);
                        AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, true, movementType.STAIR_GAP, new bool[numCollectibles], (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 200));
                    }
                    else if (fromPlatform.leftEdge <= toPlatform.rightEdge + 16 && fromPlatform.leftEdge >= toPlatform.rightEdge)
                    {
                        LevelArray.Point movePoint = new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                        LevelArray.Point landPoint = new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);
                        AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, false, movementType.STAIR_GAP, new bool[numCollectibles], (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 200));
                    }
                }                   
            }
        }

        protected override void SetMoveInfoList_Morph(int[,] levelArray, int numCollectibles, PlatformInfo fromPlatform)
        {
            foreach (PlatformInfo toPlatform in platformInfoList)
            {
                if (fromPlatform.Equals(toPlatform) ||
                    fromPlatform.height != toPlatform.height)
                {
                    continue;
                }

                bool rightMove;

                if (fromPlatform.rightEdge == toPlatform.leftEdge)
                {
                    rightMove = true;
                }
                else if (fromPlatform.leftEdge == toPlatform.rightEdge)
                {
                    rightMove = false;
                }
                else
                {
                    continue;
                }

                int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;
                bool[] collectible_onPath = new bool[numCollectibles];

                if (toPlatform.allowedHeight > 0 && toPlatform.allowedHeight < 100)
                {

                    LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge - 100, fromPlatform.height) : new LevelArray.Point(fromPlatform.leftEdge + 100, fromPlatform.height);
                    LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.rightEdge + 100, toPlatform.height) : new LevelArray.Point(toPlatform.leftEdge - 100, toPlatform.height);

                    for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                    {
                        List<LevelArray.ArrayPoint> rectanglePixels = GetFormPixels(new LevelArray.Point(k, toPlatform.height - (toPlatform.allowedHeight / 2)), toPlatform.allowedHeight);
                      
                        collectible_onPath = GetCollectibles_onPixels(levelArray, rectanglePixels, numCollectibles);

                        AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, rightMove, movementType.MORPH_DOWN, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, toPlatform.allowedHeight));
                    }
                }
                else
                {
                    LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.leftEdge, toPlatform.height) : new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);

                    AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, new bool[numCollectibles], (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                }
            }
        }

        //private void SetMoveInfoList_StraightFall(int[,] levelArray, PlatformInfo fromPlatform, int numCollectibles)
        //{
        //    foreach (PlatformInfo toPlatform in platformInfoList)
        //    {
        //        if (fromPlatform.Equals(toPlatform) ||
        //            fromPlatform.height < toPlatform.height - 2 * LevelArray.PIXEL_LENGTH ||
        //            fromPlatform.height > toPlatform.height + 2 * LevelArray.PIXEL_LENGTH)
        //        {
        //            continue;
        //        }

        //        // COMING FROM THE LEFT

        //        if (toPlatform.leftEdge - fromPlatform.rightEdge < 150 && toPlatform.leftEdge - fromPlatform.rightEdge > 50)
        //        {
        //            int movePoint_X = fromPlatform.rightEdge + ((toPlatform.leftEdge - fromPlatform.rightEdge) / 2) + LevelArray.PIXEL_LENGTH;
        //            int movePoint_Y = fromPlatform.height - (GameInfo.MIN_RECTANGLE_HEIGHT / 2);
        //            LevelArray.Point movePoint = new LevelArray.Point(movePoint_X, movePoint_Y);

        //            SetMoveInfoList_Fall(levelArray, fromPlatform, movePoint, 0, true, numCollectibles, GameInfo.MIN_RECTANGLE_HEIGHT);

        //            movePoint = new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height);
        //            LevelArray.Point landPoint = new LevelArray.Point(toPlatform.leftEdge, toPlatform.height);

        //            bool[] collectible_onPath = new bool[numCollectibles];

        //            /*
        //                * VAI SER PRECISO TRATAR DOS COLLECTIBLES E OBSTACULOS
        //                */

        //            AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, true, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 50));


        //        }

        //        // COMING FROM THE RIGHT

        //        if (fromPlatform.leftEdge - toPlatform.rightEdge < 150 && fromPlatform.leftEdge - toPlatform.rightEdge > 50)
        //        {
        //            int movePoint_X = fromPlatform.leftEdge - ((fromPlatform.leftEdge - toPlatform.rightEdge) / 2) - LevelArray.PIXEL_LENGTH;
        //            int movePoint_Y = fromPlatform.height - (GameInfo.MIN_RECTANGLE_HEIGHT / 2);
        //            LevelArray.Point movePoint = new LevelArray.Point(movePoint_X, movePoint_Y);

        //            SetMoveInfoList_Fall(levelArray, fromPlatform, movePoint, 0, false, numCollectibles, GameInfo.MIN_RECTANGLE_HEIGHT);

        //            movePoint = new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
        //            LevelArray.Point landPoint = new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);

        //            bool[] collectible_onPath = new bool[numCollectibles];

        //            /*
        //                * VAI SER PRECISO TRATAR DOS COLLECTIBLES E OBSTACULOS
        //                */

        //            AddMoveInfoToList(fromPlatform, new MoveInfo(toPlatform, movePoint, landPoint, 0, false, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 50));
        //        }

        //    }
        //}

        protected override List<LevelArray.ArrayPoint> GetFormPixels(LevelArray.Point center, int height)
        {
            LevelArray.ArrayPoint rectangleCenterArray = LevelArray.ConvertPointIntoArrayPoint(center, false, false);

            int rectangleHighestY = LevelArray.ConvertValue_PointIntoArrayPoint(center.y - (height/2), false);
            int rectangleLowestY = LevelArray.ConvertValue_PointIntoArrayPoint(center.y + (height/2), true);

            float rectangleWidth = GameInfo.RECTANGLE_AREA / height;
            int rectangleLeftX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(center.x - (rectangleWidth / 2)), false);
            int rectangleRightX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(center.x + (rectangleWidth / 2)), true);

            List<LevelArray.ArrayPoint> rectanglePixels = new List<LevelArray.ArrayPoint>();

            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new LevelArray.ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }
    }
}
