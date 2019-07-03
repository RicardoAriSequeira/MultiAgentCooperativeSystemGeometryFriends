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

        public override platformType[,] GetPlatformArray()
        {
            platformType[,] platformArray = new platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    if (levelArray[y, x] == LevelArray.BLACK || levelArray[y, x] == LevelArray.YELLOW)
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

                        platformArray[y, x] = levelArray[y, x] == LevelArray.BLACK? platformType.BLACK : platformType.YELLOW;
                    }

                });
            }

            return platformArray;

        }

        public override void SetPlatformList()
        {
            platformType[,] platformArray = GetPlatformArray();

            Parallel.For(0, platformArray.GetLength(0), y =>
            {

                int min_height_pixels = min_height / LevelArray.PIXEL_LENGTH;
                int max_height_pixels = Math.Min((max_height / LevelArray.PIXEL_LENGTH), y + LevelArray.MARGIN + min_height_pixels);

                int leftEdge = 0, allowedHeight = max_height_pixels, gap_size = 0;
                platformType currentPlatform = platformType.NO_PLATFORM;

                for (int x = 0; x < platformArray.GetLength(1); x++)
                {

                    if (currentPlatform == platformType.NO_PLATFORM)
                    {
                        if (platformArray[y, x] == platformType.BLACK || platformArray[y, x] == platformType.YELLOW)
                        {

                            if (7 <= gap_size && gap_size <= 19)
                            {
                                int rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);

                                if (rightEdge >= leftEdge)
                                {

                                    int gap_allowed_height = (area / (Math.Min((gap_size + 8) * LevelArray.PIXEL_LENGTH, max_height))) / LevelArray.PIXEL_LENGTH;

                                    lock (platformList)
                                    {
                                        platformList.Add(new Platform(platformType.GAP, LevelArray.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), gap_allowed_height));
                                    }
                                }
                            }

                            gap_size = 0;
                            currentPlatform = platformArray[y, x];

                            for (int h = min_height_pixels; h <= max_height_pixels; h++)
                            {
                                if (levelArray[y - h, x] == LevelArray.BLACK || levelArray[y - h, x] == LevelArray.YELLOW)
                                {
                                    allowedHeight = h;
                                    break;
                                }
                            }

                            leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x);
                        }

                        else if (levelArray[y,x] == LevelArray.GREEN || levelArray[y,x] == LevelArray.OPEN)
                        {
                            gap_size++;
                        }
                    }

                    else
                    {
                        if (platformArray[y, x] != currentPlatform)
                        {
                            int rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platformList)
                                {
                                    platformList.Add(new Platform(currentPlatform, LevelArray.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), allowedHeight));
                                }
                            }

                            allowedHeight = max_height_pixels;
                            currentPlatform = platformArray[y, x];
                            leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x);

                        }

                        if (platformArray[y, x] != platformType.NO_PLATFORM && y > LevelArray.MARGIN + min_height_pixels)
                        {

                            for (int h = min_height_pixels; h <= max_height_pixels; h++)
                            {

                                if (levelArray[y - h, x] == LevelArray.BLACK ||
                                    levelArray[y - h, x] == LevelArray.YELLOW ||
                                    (h == max_height_pixels && allowedHeight != max_height_pixels && levelArray[y - h, x] == LevelArray.OPEN))
                                {

                                    if (h != allowedHeight)
                                    {

                                        int rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);

                                        if (rightEdge >= leftEdge)
                                        {
                                            lock (platformList)
                                            {
                                                platformList.Add(new Platform(currentPlatform, LevelArray.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), allowedHeight));
                                            }
                                        }

                                        allowedHeight = h;
                                        leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(x - 1);

                                    }

                                    break;

                                }
                            }
                        }
                    }
                }
            });

            SetPlatformID();
        }

        protected override void SetMoves_StairOrGap(Platform fromPlatform)
        {

            foreach (Platform toPlatform in platformList)
            {

                if (fromPlatform.Equals(toPlatform))
                {
                    continue;
                }

                if (fromPlatform.height == toPlatform.height)
                {
                    SetMoves_Gap(fromPlatform, toPlatform);
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
                        //AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                    }
                }

                else 
                {
                    SetMoves_Stair(fromPlatform, toPlatform);
                }
            }
        }

        private void SetMoves_Gap(Platform fromPlatform, Platform toPlatform)
        {
            foreach (Platform gap in platformList)
            {
                if (gap.type != platformType.GAP)
                {
                    continue;
                }

                int gap_size = (gap.rightEdge - gap.leftEdge);
                int fall_height = GameInfo.RECTANGLE_AREA / Math.Min(gap_size - (2 * LevelArray.PIXEL_LENGTH), GameInfo.MIN_RECTANGLE_HEIGHT);
                LevelArray.Point movePoint = new LevelArray.Point(gap.leftEdge + (gap_size / 2), gap.height - (fall_height / 2));

                float pathLength = 0;
                LevelArray.Point currentCenter = movePoint;
                bool[] collectible_onPath = new bool[nCollectibles];

                for (int i = 1; true; i++)
                {
                    float currentTime = i * TIME_STEP;
                    LevelArray.Point previousCenter = currentCenter;
                    currentCenter = GetCurrentCenter(movePoint, 0, GameInfo.FALL_VELOCITYY, currentTime);
                    List<LevelArray.ArrayPoint> pixels = GetFormPixels(currentCenter, fall_height);

                    if ((currentCenter.y > movePoint.y + 16) && IsObstacle_onPixels(pixels))
                    {
                        Platform? platform = GetPlatform(previousCenter, fall_height);

                        if (platform != null)
                        {
                            AddMoveInfoToList(gap, new Move((Platform)platform, movePoint, previousCenter, 0, true, movementType.FALL, collectible_onPath, (int)pathLength, false, fall_height));
                            break;
                        }

                    }

                    collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(pixels));
                    pathLength += (float)Math.Sqrt(Math.Pow(currentCenter.x - previousCenter.x, 2) + Math.Pow(currentCenter.y - previousCenter.y, 2));

                    int new_height = GameInfo.MIN_RECTANGLE_HEIGHT;
                    pixels = GetFormPixels(currentCenter, new_height);
                    if ((previousCenter.y > movePoint.y + 100) && IsObstacle_onPixels(pixels))
                    {
                        Platform? platform = GetPlatform(previousCenter, new_height);

                        if (platform != null)
                        {
                            AddMoveInfoToList(gap, new Move((Platform)platform, movePoint, previousCenter, 0, true, movementType.FALL, collectible_onPath, (int)pathLength, false, new_height));
                        }

                    }
                }

                foreach (Platform platform in platformList)
                {
                    if (platform.Equals(gap) || platform.height != gap.height)
                    {
                        continue;
                    }

                    if (gap.rightEdge + 8 == platform.leftEdge)
                    {
                        collectible_onPath = new bool[nCollectibles];
                        int rectangleWidth = GameInfo.RECTANGLE_AREA / gap.allowedHeight;
                        movePoint = new LevelArray.Point(gap.rightEdge - (rectangleWidth / 2), gap.height - (gap.allowedHeight / 2));
                        LevelArray.Point landPoint = new LevelArray.Point(platform.leftEdge + (rectangleWidth / 2), platform.height - (platform.allowedHeight / 2));
                        AddMoveInfoToList(gap, new Move(platform, movePoint, landPoint, 0, true, movementType.STAIR_GAP, collectible_onPath, (gap.height - platform.height) + Math.Abs(movePoint.x - landPoint.x), false, gap.allowedHeight));
                        AddMoveInfoToList(platform, new Move(gap, landPoint, movePoint, 0, false, movementType.STAIR_GAP, collectible_onPath, (platform.height - gap.height) + Math.Abs(landPoint.x - movePoint.x), false, gap.allowedHeight));
                    }

                    if (gap.leftEdge - 8 == platform.rightEdge)
                    {
                        collectible_onPath = new bool[nCollectibles];
                        int rectangleWidth = GameInfo.RECTANGLE_AREA / gap.allowedHeight;
                        movePoint = new LevelArray.Point(gap.leftEdge + (rectangleWidth / 2), gap.height - (gap.allowedHeight / 2));
                        LevelArray.Point landPoint = new LevelArray.Point(platform.rightEdge - (rectangleWidth / 2), platform.height - (platform.allowedHeight / 2));
                        AddMoveInfoToList(gap, new Move(platform, movePoint, landPoint, 0, false, movementType.STAIR_GAP, collectible_onPath, (gap.height - platform.height) + Math.Abs(movePoint.x - landPoint.x), false, gap.allowedHeight));
                        AddMoveInfoToList(platform, new Move(gap, landPoint, movePoint, 0, true, movementType.STAIR_GAP, collectible_onPath, (platform.height - gap.height) + Math.Abs(landPoint.x - movePoint.x), false, gap.allowedHeight));
                    }

                }
            }

        }

        private void SetMoves_Stair(Platform fromPlatform, Platform toPlatform)
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

        protected override void SetMoves_Morph(Platform fromPlatform)
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
                if (levelArray[y, i] == LevelArray.BLACK)
                {
                    return false;
                }
            }

            return true;
        }

        private List<Platform> PlatformsBelow(LevelArray.Point center, int height)
        {

            List<Platform> platforms = new List<Platform>();

            int rectangleWidth = GameInfo.RECTANGLE_AREA / height;
            int rectangleLeftX = center.x - (rectangleWidth / 2);
            int rectangleRightX = center.x + (rectangleWidth / 2);

            foreach (Platform i in platformList)
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

        protected override bool IsObstacle_onPixels(List<LevelArray.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelArray.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelArray.BLACK || levelArray[i.yArray, i.xArray] == LevelArray.YELLOW)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
