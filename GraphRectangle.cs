using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class GraphRectangle : Graph
    {

        public GraphRectangle() : base()
        {
            area = GameInfo.RECTANGLE_AREA;
            min_height = GameInfo.MIN_RECTANGLE_HEIGHT;
            max_height = GameInfo.MAX_RECTANGLE_HEIGHT;
        }

        public platformType[,] GetPlatformArray()
        {
            platformType[,] platformArray = new platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    if (levelArray[y, x] == LevelRepresentation.BLACK || levelArray[y, x] == LevelRepresentation.YELLOW)
                    {

                        // RECTANGLE WITH HEIGHT 100
                        LevelRepresentation.Point rectangleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                        rectangleCenter.y -= GameInfo.SQUARE_HEIGHT / 2;
                        List<LevelRepresentation.ArrayPoint> rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.SQUARE_HEIGHT);

                        if (IsObstacle_onPixels(rectanglePixels))
                        {

                            // RECTANGLE WITH HEIGHT 50
                            rectangleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                            rectangleCenter.y -= GameInfo.MIN_RECTANGLE_HEIGHT / 2;
                            rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.MIN_RECTANGLE_HEIGHT);

                            if (IsObstacle_onPixels(rectanglePixels))
                            {

                                // RECTANGLE WITH HEIGHT 200
                                rectangleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                                rectangleCenter.y -= GameInfo.MAX_RECTANGLE_HEIGHT / 2;
                                rectanglePixels = GetFormPixels(rectangleCenter, GameInfo.MAX_RECTANGLE_HEIGHT);

                                if (IsObstacle_onPixels(rectanglePixels))
                                {
                                    return;
                                }
                            }
                        }

                        platformArray[y, x] = (levelArray[y, x] == LevelRepresentation.BLACK)? platformType.BLACK : platformType.YELLOW;
                    }

                });
            }

            return platformArray;

        }

        public override void SetupPlatforms()
        {
            platformType[,] platformArray = GetPlatformArray();

            Parallel.For(0, platformArray.GetLength(0), y =>
            {

                int min_height_pixels = min_height / LevelRepresentation.PIXEL_LENGTH;
                int max_height_pixels = Math.Min((max_height / LevelRepresentation.PIXEL_LENGTH), y + LevelRepresentation.MARGIN + min_height_pixels);

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
                                int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

                                if (rightEdge >= leftEdge)
                                {

                                    int gap_allowed_height = (area / (Math.Min((gap_size + 8) * LevelRepresentation.PIXEL_LENGTH, max_height))) / LevelRepresentation.PIXEL_LENGTH;

                                    lock (platforms)
                                    {
                                        platforms.Add(new Platform(platformType.GAP, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), gap_allowed_height));
                                    }
                                }
                            }

                            gap_size = 0;
                            currentPlatform = platformArray[y, x];

                            for (int h = min_height_pixels; h <= max_height_pixels; h++)
                            {
                                if (levelArray[y - h, x] == LevelRepresentation.BLACK || levelArray[y - h, x] == LevelRepresentation.YELLOW)
                                {
                                    allowedHeight = h;
                                    break;
                                }
                            }

                            leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x);
                        }

                        else if (levelArray[y,x] == LevelRepresentation.GREEN || levelArray[y,x] == LevelRepresentation.OPEN)
                        {
                            gap_size++;
                        }
                    }

                    else
                    {
                        if (platformArray[y, x] != currentPlatform)
                        {
                            int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platforms)
                                {
                                    platforms.Add(new Platform(currentPlatform, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), allowedHeight));
                                }
                            }

                            allowedHeight = max_height_pixels;
                            currentPlatform = platformArray[y, x];
                            leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x);

                        }

                        if (platformArray[y, x] != platformType.NO_PLATFORM && y > LevelRepresentation.MARGIN + min_height_pixels)
                        {

                            for (int h = min_height_pixels; h <= max_height_pixels; h++)
                            {

                                if (levelArray[y - h, x] == LevelRepresentation.BLACK ||
                                    levelArray[y - h, x] == LevelRepresentation.YELLOW ||
                                    (h == max_height_pixels && allowedHeight != max_height_pixels && levelArray[y - h, x] == LevelRepresentation.OPEN))
                                {

                                    if (h != allowedHeight)
                                    {

                                        int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

                                        if (rightEdge >= leftEdge)
                                        {
                                            lock (platforms)
                                            {
                                                platforms.Add(new Platform(currentPlatform, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), allowedHeight));
                                            }
                                        }

                                        allowedHeight = h;
                                        leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

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

            foreach (Platform toPlatform in platforms)
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

                    for (int k = from; k <= to; k += LevelRepresentation.PIXEL_LENGTH)
                    {
                        List<LevelRepresentation.ArrayPoint> rectanglePixels = GetFormPixels(new LevelRepresentation.Point(k, toPlatform.height - GameInfo.SQUARE_HEIGHT), GameInfo.SQUARE_HEIGHT);

                        if (IsObstacle_onPixels(rectanglePixels))
                        {
                            obstacleFlag = true;
                            break;
                        }

                        collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(rectanglePixels));
                    }

                    if (!obstacleFlag)
                    {
                        LevelRepresentation.Point movePoint = rightMove ? new LevelRepresentation.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelRepresentation.Point(fromPlatform.leftEdge, fromPlatform.height);
                        LevelRepresentation.Point landPoint = rightMove ? new LevelRepresentation.Point(toPlatform.leftEdge, toPlatform.height) : new LevelRepresentation.Point(toPlatform.rightEdge, toPlatform.height);
                        AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
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
            foreach (Platform gap in platforms)
            {
                if (gap.type != platformType.GAP)
                {
                    continue;
                }

                int gap_size = (gap.rightEdge - gap.leftEdge);
                int fall_height = GameInfo.RECTANGLE_AREA / Math.Min(gap_size - (2 * LevelRepresentation.PIXEL_LENGTH), GameInfo.MIN_RECTANGLE_HEIGHT);
                LevelRepresentation.Point movePoint = new LevelRepresentation.Point(gap.leftEdge + (gap_size / 2), gap.height - (fall_height / 2));

                float pathLength = 0;
                LevelRepresentation.Point currentCenter = movePoint;
                bool[] collectible_onPath = new bool[nCollectibles];

                for (int i = 1; true; i++)
                {
                    float currentTime = i * TIME_STEP;
                    LevelRepresentation.Point previousCenter = currentCenter;
                    currentCenter = GetCurrentCenter(movePoint, 0, GameInfo.FALL_VELOCITYY, currentTime);
                    List<LevelRepresentation.ArrayPoint> pixels = GetFormPixels(currentCenter, fall_height);

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

                foreach (Platform platform in platforms)
                {
                    if (platform.Equals(gap) || platform.height != gap.height)
                    {
                        continue;
                    }

                    if (gap.rightEdge + 8 == platform.leftEdge)
                    {
                        collectible_onPath = new bool[nCollectibles];
                        int rectangleWidth = GameInfo.RECTANGLE_AREA / gap.allowedHeight;
                        movePoint = new LevelRepresentation.Point(gap.rightEdge - (rectangleWidth / 2), gap.height - (gap.allowedHeight / 2));
                        LevelRepresentation.Point landPoint = new LevelRepresentation.Point(platform.leftEdge + (rectangleWidth / 2), platform.height - (platform.allowedHeight / 2));
                        AddMoveInfoToList(gap, new Move(platform, movePoint, landPoint, 0, true, movementType.STAIR_GAP, collectible_onPath, (gap.height - platform.height) + Math.Abs(movePoint.x - landPoint.x), false, gap.allowedHeight));
                        AddMoveInfoToList(platform, new Move(gap, landPoint, movePoint, 0, false, movementType.STAIR_GAP, collectible_onPath, (platform.height - gap.height) + Math.Abs(landPoint.x - movePoint.x), false, gap.allowedHeight));
                    }

                    if (gap.leftEdge - 8 == platform.rightEdge)
                    {
                        collectible_onPath = new bool[nCollectibles];
                        int rectangleWidth = GameInfo.RECTANGLE_AREA / gap.allowedHeight;
                        movePoint = new LevelRepresentation.Point(gap.leftEdge + (rectangleWidth / 2), gap.height - (gap.allowedHeight / 2));
                        LevelRepresentation.Point landPoint = new LevelRepresentation.Point(platform.rightEdge - (rectangleWidth / 2), platform.height - (platform.allowedHeight / 2));
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
                    LevelRepresentation.Point movePoint = new LevelRepresentation.Point(fromPlatform.rightEdge, fromPlatform.height);
                    LevelRepresentation.Point landPoint = new LevelRepresentation.Point(toPlatform.leftEdge, toPlatform.height);
                    AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 50, true, movementType.MORPH_UP, new bool[nCollectibles], (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 190));
                }
                else if (toPlatform.rightEdge <= fromPlatform.leftEdge && fromPlatform.leftEdge <= toPlatform.rightEdge + 33)
                {
                    LevelRepresentation.Point movePoint = new LevelRepresentation.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelRepresentation.Point landPoint = new LevelRepresentation.Point(toPlatform.rightEdge, toPlatform.height);
                    AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 50, false, movementType.MORPH_UP, new bool[nCollectibles], (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, 190));
                }
            }
        }

        protected override void SetMoves_Morph(Platform fromPlatform)
        {
            foreach (Platform toPlatform in platforms)
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

                LevelRepresentation.Point movePoint = rightMove ? new LevelRepresentation.Point(fromPlatform.rightEdge - 100, fromPlatform.height - (toPlatform.allowedHeight / 2)) : new LevelRepresentation.Point(fromPlatform.leftEdge + 100, fromPlatform.height - (toPlatform.allowedHeight / 2));
                LevelRepresentation.Point landPoint = rightMove ? new LevelRepresentation.Point(toPlatform.rightEdge + 100, toPlatform.height - (toPlatform.allowedHeight / 2)) : new LevelRepresentation.Point(toPlatform.leftEdge - 100, toPlatform.height - (toPlatform.allowedHeight / 2));

                for (int k = from; k <= to; k += LevelRepresentation.PIXEL_LENGTH)
                {
                    List<LevelRepresentation.ArrayPoint> rectanglePixels = GetFormPixels(new LevelRepresentation.Point(k, toPlatform.height - (toPlatform.allowedHeight / 2)), toPlatform.allowedHeight);       
                    collectible_onPath = collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(rectanglePixels));
                }

                AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.MORPH_DOWN, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, toPlatform.allowedHeight));

            }
        }

        protected override List<LevelRepresentation.ArrayPoint> GetFormPixels(LevelRepresentation.Point center, int height)
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

        protected override bool IsObstacle_onPixels(List<LevelRepresentation.ArrayPoint> checkPixels)
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
