using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class PlatformsRectangle : Platforms
    {

        public PlatformsRectangle() : base()
        {
            area = GameInfo.RECTANGLE_AREA;
            min_height = GameInfo.MIN_RECTANGLE_HEIGHT;
            max_height = GameInfo.MAX_RECTANGLE_HEIGHT;
        }

        public override int[,] GetPlatformArray()
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

                    if (IsObstacle_onPixels(rectanglePixels))
                    {

                        // RECTANGLE WITH HEIGHT 50
                        rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(x, y));
                        rectangleCenter.y -= GameInfo.MIN_RECTANGLE_HEIGHT / 2;
                        rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.MIN_RECTANGLE_HEIGHT);

                        if (IsObstacle_onPixels(rectanglePixels))
                        {

                            // RECTANGLE WITH HEIGHT 200
                            rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(x, y));
                            rectangleCenter.y -= GameInfo.MAX_RECTANGLE_HEIGHT / 2;
                            rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.MAX_RECTANGLE_HEIGHT);

                            if (IsObstacle_onPixels(rectanglePixels))
                            {
                                return;
                            }
                        }
                    }

                    // if there is no obstacles but there is one below the rectangle, we add this point as platform
                    if (levelArray[y, x] == LevelArray.OBSTACLE)
                    {
                        platformArray[y, x] = LevelArray.OBSTACLE;
                    }

                });
            }

            return platformArray;

        }

        public override void SetPlatformInfoList()
        {
            int[,] platformArray = GetPlatformArray();

            Parallel.For(0, levelArray.GetLength(0), y =>
            {
                bool platformFlag = false;
                int height = 0, leftEdge = 0, rightEdge = 0;

                int MIN_H = (GameInfo.MIN_RECTANGLE_HEIGHT / LevelArray.PIXEL_LENGTH);
                int MAX_H = Math.Min((GameInfo.MAX_RECTANGLE_HEIGHT / LevelArray.PIXEL_LENGTH), y + LevelArray.MARGIN + MIN_H);
                int allowedHeight = MAX_H;

                for (int x = 0; x < platformArray.GetLength(1); x++)
                {
                    if (!platformFlag && platformArray[y, x] == LevelArray.OBSTACLE)
                    {
                        platformFlag = true;

                        for (int h = MIN_H; h <= MAX_H; h++)
                        {
                            if (levelArray[y - h, x] == LevelArray.OBSTACLE)
                            {
                                allowedHeight = h;
                                break;
                            }

                        }

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
                                lock (platformList)
                                {
                                    platformList.Add(new Platform(height, leftEdge, rightEdge, new List<Move>(), allowedHeight));
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
                                            lock (platformList)
                                            {
                                                platformList.Add(new Platform(height, leftEdge, rightEdge, new List<Move>(), allowedHeight));
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
                                            lock (platformList)
                                            {
                                                platformList.Add(new Platform(height, leftEdge, rightEdge, new List<Move>(), allowedHeight));
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

        protected override void SetMoveInfoList_StairOrGap(Platform fromPlatform)
        {

            foreach (Platform toPlatform in platformList)
            {

                if (fromPlatform.Equals(toPlatform))
                {
                    continue;
                }

                if (fromPlatform.height == toPlatform.height)
                {
                    SetMoveInfoList_Gap(fromPlatform, toPlatform);
                }

                bool rightMove = false;

                if (IsStairOrGap(fromPlatform, toPlatform, ref rightMove))
                {
                    bool obstacleFlag = false;
                    bool[] collectible_onPath = new bool[nCollectibles];

                    int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                    int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;

                    for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                    {
                        List<LevelArray.ArrayPoint> rectanglePixels = GetFormPixels(new LevelArray.Point(k, toPlatform.height - GameInfo.SQUARE_HEIGHT), GameInfo.SQUARE_HEIGHT);

                        if (IsObstacle_onPixels(rectanglePixels))
                        {
                            obstacleFlag = true;
                            break;
                        }

                        collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(rectanglePixels));
                    }

                    if (!obstacleFlag)
                    {
                        LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                        LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.leftEdge, toPlatform.height) : new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);
                        AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                    }
                }

                else 
                {
                    SetMoveInfoList_Stair(fromPlatform, toPlatform);
                }
            }
        }

        private void SetMoveInfoList_Gap(Platform fromPlatform, Platform toPlatform)
        {

            bool rightMove = false;
            bool gap = false;

            // COMING FROM THE LEFT
            int gap_size = toPlatform.leftEdge - fromPlatform.rightEdge;
            if (50 < gap_size && gap_size < 150 && CheckEmptyGap(fromPlatform, toPlatform))
            {
                gap = true;
                rightMove = true;
            }
            else
            {
                // COMING FROM THE RIGHT
                gap_size = fromPlatform.leftEdge - toPlatform.rightEdge;
                if (50 < gap_size && gap_size < 150 && CheckEmptyGap(toPlatform, fromPlatform))
                {
                    gap = true;
                    rightMove = false;
                }
            }

            if (gap)
            {
                bool[] collectible_onPath = new bool[nCollectibles];

                int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;
                int required_height = GameInfo.RECTANGLE_AREA / (gap_size + 6 * LevelArray.PIXEL_LENGTH);

                for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                {
                    List<LevelArray.ArrayPoint> rectanglePixels = GetFormPixels(new LevelArray.Point(k, toPlatform.height - (required_height / 2)), required_height);

                    if (IsObstacle_onPixels(rectanglePixels))
                    {
                        return;
                    }

                    collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(rectanglePixels));
                }

                LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.leftEdge, toPlatform.height) : new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);
                AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, required_height));

            }

        }

        private void SetMoveInfoList_Stair(Platform fromPlatform, Platform toPlatform)
        {
            if (fromPlatform.height - 90 <= toPlatform.height && toPlatform.height <= fromPlatform.height)
            {
                if (toPlatform.leftEdge - 33 <= fromPlatform.rightEdge && fromPlatform.rightEdge <= toPlatform.leftEdge)
                {
                    LevelArray.Point movePoint = new LevelArray.Point(fromPlatform.rightEdge, fromPlatform.height);
                    LevelArray.Point landPoint = new LevelArray.Point(toPlatform.leftEdge, toPlatform.height);
                    AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 50, true, movementType.MORPH_UP, new bool[nCollectibles], (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 190));
                }
                else if (toPlatform.rightEdge <= fromPlatform.leftEdge && fromPlatform.leftEdge <= toPlatform.rightEdge + 33)
                {
                    LevelArray.Point movePoint = new LevelArray.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelArray.Point landPoint = new LevelArray.Point(toPlatform.rightEdge, toPlatform.height);
                    AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 50, false, movementType.MORPH_UP, new bool[nCollectibles], (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 190));
                }
            }
        }

        protected override void SetMoveInfoList_Morph(Platform fromPlatform)
        {
            foreach (Platform toPlatform in platformList)
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
                bool[] collectible_onPath = new bool[nCollectibles];

                LevelArray.Point movePoint = rightMove ? new LevelArray.Point(fromPlatform.rightEdge - 100, fromPlatform.height - (toPlatform.allowedHeight / 2)) : new LevelArray.Point(fromPlatform.leftEdge + 100, fromPlatform.height - (toPlatform.allowedHeight / 2));
                LevelArray.Point landPoint = rightMove ? new LevelArray.Point(toPlatform.rightEdge + 100, toPlatform.height - (toPlatform.allowedHeight / 2)) : new LevelArray.Point(toPlatform.leftEdge - 100, toPlatform.height - (toPlatform.allowedHeight / 2));

                for (int k = from; k <= to; k += LevelArray.PIXEL_LENGTH)
                {
                    List<LevelArray.ArrayPoint> rectanglePixels = GetFormPixels(new LevelArray.Point(k, toPlatform.height - (toPlatform.allowedHeight / 2)), toPlatform.allowedHeight);       
                    collectible_onPath = collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(rectanglePixels));
                }

                AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.MORPH_DOWN, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, toPlatform.allowedHeight));

            }
        }

        protected override void SetMoveInfoList_StraightFall(Platform fromPlatform)
        {
            foreach (Platform toPlatform in platformList)
            {
                if (fromPlatform.Equals(toPlatform) ||
                    fromPlatform.height < toPlatform.height - 2 * LevelArray.PIXEL_LENGTH ||
                    fromPlatform.height > toPlatform.height + 2 * LevelArray.PIXEL_LENGTH)
                {
                    continue;
                }

                // COMING FROM THE LEFT

                int hole_size = toPlatform.leftEdge - fromPlatform.rightEdge;

                if (50 < hole_size && hole_size < 150)
                {
                    //int required_height = GameInfo.RECTANGLE_AREA / (hole_size + (6 * LevelArray.PIXEL_LENGTH));
                    LevelArray.Point movePoint = new LevelArray.Point(fromPlatform.rightEdge + (hole_size / 2) + LevelArray.PIXEL_LENGTH, fromPlatform.height - (min_height / 2));
                    SetMoveInfoList_Fall(fromPlatform, movePoint, min_height, 0, true, movementType.FALL);
                }

                // COMING FROM THE RIGHT

                hole_size = fromPlatform.leftEdge - toPlatform.rightEdge;

                if (50 < hole_size && hole_size < 150)
                {                
                    //int required_height = GameInfo.RECTANGLE_AREA / (hole_size + (6 * LevelArray.PIXEL_LENGTH));
                    LevelArray.Point movePoint = new LevelArray.Point(fromPlatform.leftEdge - (hole_size / 2) - LevelArray.PIXEL_LENGTH, fromPlatform.height - (min_height / 2));
                    SetMoveInfoList_Fall(fromPlatform, movePoint, min_height, 0, false, movementType.FALL);
                }

            }
        }

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

        private bool CheckEmptyGap(Platform fromPlatform, Platform toPlatform)
        {

            int from = LevelArray.ConvertValue_PointIntoArrayPoint(fromPlatform.rightEdge, false) + 1;
            int to = LevelArray.ConvertValue_PointIntoArrayPoint(toPlatform.leftEdge, true);
           
            int y = LevelArray.ConvertValue_PointIntoArrayPoint(fromPlatform.height, false);

            for (int i = from; i <= to; i++)
            {
                if (levelArray[y, i] == LevelArray.OBSTACLE)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
