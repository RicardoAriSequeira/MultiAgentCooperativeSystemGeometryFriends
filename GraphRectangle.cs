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

        protected override void SetupMoves_StairOrGap(Platform fromPlatform)
        {

            foreach (Platform toPlatform in platforms)
            {

                if (fromPlatform.Equals(toPlatform))
                {
                    continue;
                }

                if (fromPlatform.type == platformType.GAP && fromPlatform.height == toPlatform.height)
                {
                    SetMoves_Gap(fromPlatform, toPlatform);
                }
                else
                {
                    SetMoves_Stair(fromPlatform, toPlatform);
                }


                //bool rightMove = false;

                //if (IsStairOrGap(fromPlatform, toPlatform, ref rightMove))
                //{
                //    bool obstacleFlag = false;
                //    bool[] collectible_onPath = new bool[nCollectibles];

                //    int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                //    int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;

                //    for (int k = from; k <= to; k += LevelRepresentation.PIXEL_LENGTH)
                //    {
                //        List<LevelRepresentation.ArrayPoint> rectanglePixels = GetFormPixels(new LevelRepresentation.Point(k, toPlatform.height - GameInfo.SQUARE_HEIGHT), GameInfo.SQUARE_HEIGHT);

                //        if (IsObstacle_onPixels(rectanglePixels))
                //        {
                //            obstacleFlag = true;
                //            break;
                //        }

                //        collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(rectanglePixels));
                //    }

                //    if (!obstacleFlag)
                //    {
                //        LevelRepresentation.Point movePoint = rightMove ? new LevelRepresentation.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelRepresentation.Point(fromPlatform.leftEdge, fromPlatform.height);
                //        LevelRepresentation.Point landPoint = rightMove ? new LevelRepresentation.Point(toPlatform.leftEdge, toPlatform.height) : new LevelRepresentation.Point(toPlatform.rightEdge, toPlatform.height);
                //        AddMoveInfoToList(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                //    }
                //}

                //else 
                //{
                //    SetMoves_Stair(fromPlatform, toPlatform);
                //}
            }
        }

        private void SetMoves_Gap(Platform gap, Platform platform)
        {

            SetMoves_GapBridge(gap, platform);

            bool rightMove;

            if (platform.leftEdge == gap.rightEdge + LevelRepresentation.PIXEL_LENGTH)
            {
                rightMove = false;
            }
            else if (platform.rightEdge == gap.leftEdge - LevelRepresentation.PIXEL_LENGTH)
            {
                rightMove = true;
            }
            else
            {
                return;
            }

            int gap_size = (gap.rightEdge - gap.leftEdge);
            int fall_width = Math.Min(gap_size - (2 * LevelRepresentation.PIXEL_LENGTH), GameInfo.MIN_RECTANGLE_HEIGHT);
            int fall_height = GameInfo.RECTANGLE_AREA / fall_width;
            LevelRepresentation.Point movePoint = new LevelRepresentation.Point(gap.leftEdge + (gap_size/2), gap.height - (fall_height/2));

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
                    Platform? reachablePlatform = GetPlatform(previousCenter, fall_height);

                    if (reachablePlatform != null)
                    {
                        AddMove(platform, new Move((Platform)reachablePlatform, movePoint, previousCenter, 0, rightMove, movementType.GAP, collectible_onPath, (int)pathLength, false, fall_height));
                        break;
                    }

                }

                collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(pixels));
                pathLength += (float)Math.Sqrt(Math.Pow(currentCenter.x - previousCenter.x, 2) + Math.Pow(currentCenter.y - previousCenter.y, 2));

            }

        }

        private void SetMoves_GapBridge(Platform gap, Platform platform)
        {
            bool rightMove = false;
            int start_x = 0, end_x = 0;
            bool[] collectibles = new bool[nCollectibles];
            int rectangleWidth = GameInfo.RECTANGLE_AREA / gap.allowedHeight;

            if (gap.rightEdge + LevelRepresentation.PIXEL_LENGTH == platform.leftEdge)
            {
                start_x = gap.rightEdge - (rectangleWidth / 2);
                end_x = platform.leftEdge + (rectangleWidth / 2);
            }
            else if (gap.leftEdge - LevelRepresentation.PIXEL_LENGTH == platform.rightEdge)
            {
                rightMove = true;
                start_x = gap.leftEdge + (rectangleWidth / 2);
                end_x = platform.rightEdge - (rectangleWidth / 2);
            }
            else
            {
                return;
            }

            LevelRepresentation.Point start = new LevelRepresentation.Point(start_x, gap.height - (gap.allowedHeight / 2));
            LevelRepresentation.Point end = new LevelRepresentation.Point(end_x, platform.height - (platform.allowedHeight / 2));

            int pathLength = (gap.height - platform.height) + Math.Abs(start.x - end.x);
            AddMove(gap, new Move(platform, start, end, 0, !rightMove, movementType.STAIR_GAP, collectibles, pathLength, false, gap.allowedHeight));

            pathLength = (platform.height - gap.height) + Math.Abs(end.x - start.x);
            AddMove(platform, new Move(gap, end, start, 0, rightMove, movementType.STAIR_GAP, collectibles, pathLength, false, gap.allowedHeight));
        }

        //private void SetMoves_Gap(Platform fromPlatform, Platform toPlatform)
        //{
        //    foreach (Platform gap in platforms)
        //    {
        //        if (gap.type != platformType.GAP)
        //        {
        //            continue;
        //        }

        //        int gap_size = (gap.rightEdge - gap.leftEdge);
        //        int fall_height = GameInfo.RECTANGLE_AREA / Math.Min(gap_size - (2 * LevelRepresentation.PIXEL_LENGTH), GameInfo.MIN_RECTANGLE_HEIGHT);
        //        LevelRepresentation.Point movePoint = new LevelRepresentation.Point(gap.leftEdge + (gap_size / 2), gap.height - (fall_height / 2));

        //        float pathLength = 0;
        //        LevelRepresentation.Point currentCenter = movePoint;
        //        bool[] collectible_onPath = new bool[nCollectibles];

        //        for (int i = 1; true; i++)
        //        {
        //            float currentTime = i * TIME_STEP;
        //            LevelRepresentation.Point previousCenter = currentCenter;
        //            currentCenter = GetCurrentCenter(movePoint, 0, GameInfo.FALL_VELOCITYY, currentTime);
        //            List<LevelRepresentation.ArrayPoint> pixels = GetFormPixels(currentCenter, fall_height);

        //            if ((currentCenter.y > movePoint.y + 16) && IsObstacle_onPixels(pixels))
        //            {
        //                Platform? platform = GetPlatform(previousCenter, fall_height);

        //                if (platform != null)
        //                {
        //                    AddMove(gap, new Move((Platform)platform, movePoint, previousCenter, 0, true, movementType.FALL, collectible_onPath, (int)pathLength, false, fall_height));
        //                    break;
        //                }

        //            }

        //            collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(pixels));
        //            pathLength += (float)Math.Sqrt(Math.Pow(currentCenter.x - previousCenter.x, 2) + Math.Pow(currentCenter.y - previousCenter.y, 2));

        //            int new_height = GameInfo.MIN_RECTANGLE_HEIGHT;
        //            pixels = GetFormPixels(currentCenter, new_height);
        //            if ((previousCenter.y > movePoint.y + 100) && IsObstacle_onPixels(pixels))
        //            {
        //                Platform? platform = GetPlatform(previousCenter, new_height);

        //                if (platform != null)
        //                {
        //                    AddMove(gap, new Move((Platform)platform, movePoint, previousCenter, 0, true, movementType.FALL, collectible_onPath, (int)pathLength, false, new_height));
        //                }

        //            }
        //        }

        //        foreach (Platform platform in platforms)
        //        {
        //            if (platform.Equals(gap) || platform.height != gap.height)
        //            {
        //                continue;
        //            }

        //            if (gap.rightEdge + 8 == platform.leftEdge)
        //            {
        //                collectible_onPath = new bool[nCollectibles];
        //                int rectangleWidth = GameInfo.RECTANGLE_AREA / gap.allowedHeight;
        //                movePoint = new LevelRepresentation.Point(gap.rightEdge - (rectangleWidth / 2), gap.height - (gap.allowedHeight / 2));
        //                LevelRepresentation.Point landPoint = new LevelRepresentation.Point(platform.leftEdge + (rectangleWidth / 2), platform.height - (platform.allowedHeight / 2));
        //                AddMove(gap, new Move(platform, movePoint, landPoint, 0, true, movementType.STAIR_GAP, collectible_onPath, (gap.height - platform.height) + Math.Abs(movePoint.x - landPoint.x), false, gap.allowedHeight));
        //                AddMove(platform, new Move(gap, landPoint, movePoint, 0, false, movementType.STAIR_GAP, collectible_onPath, (platform.height - gap.height) + Math.Abs(landPoint.x - movePoint.x), false, gap.allowedHeight));
        //            }

        //            if (gap.leftEdge - 8 == platform.rightEdge)
        //            {
        //                collectible_onPath = new bool[nCollectibles];
        //                int rectangleWidth = GameInfo.RECTANGLE_AREA / gap.allowedHeight;
        //                movePoint = new LevelRepresentation.Point(gap.leftEdge + (rectangleWidth / 2), gap.height - (gap.allowedHeight / 2));
        //                LevelRepresentation.Point landPoint = new LevelRepresentation.Point(platform.rightEdge - (rectangleWidth / 2), platform.height - (platform.allowedHeight / 2));
        //                AddMove(gap, new Move(platform, movePoint, landPoint, 0, false, movementType.STAIR_GAP, collectible_onPath, (gap.height - platform.height) + Math.Abs(movePoint.x - landPoint.x), false, gap.allowedHeight));
        //                AddMove(platform, new Move(gap, landPoint, movePoint, 0, true, movementType.STAIR_GAP, collectible_onPath, (platform.height - gap.height) + Math.Abs(landPoint.x - movePoint.x), false, gap.allowedHeight));
        //            }

        //        }
        //    }

        //}

        private void SetMoves_Stair(Platform fromPlatform, Platform toPlatform)
        {

            int stairHeight = fromPlatform.height - toPlatform.height;

            if (1 <= stairHeight && stairHeight <= 90)
            {

                if (toPlatform.leftEdge - 33 <= fromPlatform.rightEdge && fromPlatform.rightEdge <= toPlatform.leftEdge)
                {
                    LevelRepresentation.Point start = new LevelRepresentation.Point(fromPlatform.rightEdge, fromPlatform.height);
                    LevelRepresentation.Point end = new LevelRepresentation.Point(toPlatform.leftEdge, toPlatform.height);

                    int pathLength = (fromPlatform.height - toPlatform.height) + Math.Abs(start.x - end.x);
                    int requiredHeight = 3 * stairHeight + LevelRepresentation.PIXEL_LENGTH;

                    AddMove(fromPlatform, new Move(toPlatform, start, end, 150, true, movementType.MORPH_UP, new bool[nCollectibles], pathLength, false, requiredHeight));
                }
                else if (toPlatform.rightEdge <= fromPlatform.leftEdge && fromPlatform.leftEdge <= toPlatform.rightEdge + 33)
                {
                    LevelRepresentation.Point start = new LevelRepresentation.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelRepresentation.Point end = new LevelRepresentation.Point(toPlatform.rightEdge, toPlatform.height);

                    int pathLength = (fromPlatform.height - toPlatform.height) + Math.Abs(start.x - end.x);
                    int requiredHeight = 3 * stairHeight + LevelRepresentation.PIXEL_LENGTH;

                    AddMove(fromPlatform, new Move(toPlatform, start, end, 150, false, movementType.MORPH_UP, new bool[nCollectibles], pathLength, false, requiredHeight));
                }
            }
        }

        protected override void SetupMoves_Morph(Platform fromPlatform)
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

                AddMove(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.MORPH_DOWN, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false, toPlatform.allowedHeight));

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
