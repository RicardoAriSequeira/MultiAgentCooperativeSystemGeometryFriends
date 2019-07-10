using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    public abstract class Graph
    {
        public enum collideType
        {
            CEILING, FLOOR, OTHER
        };

        public enum movementType
        {
            COLLECT, MORPH_UP, MORPH_DOWN, STAIR_GAP, FALL, JUMP
        };

        public enum platformType
        {
            NO_PLATFORM, BLACK, GREEN, YELLOW, GAP
        };

        public const int VELOCITYX_STEP = 20;

        protected const float TIME_STEP = 0.01f;

        protected int[] LENGTH_TO_ACCELERATE = new int[10] { 1, 5, 13, 20, 31, 49, 70, 95, 128, 166 };

        protected const int STAIR_MAXWIDTH = 48;
        protected const int STAIR_MAXHEIGHT = 16;

        protected int[,] levelArray;
        protected int area, min_height, max_height, nCollectibles;

        public List<Platform> platforms;

        public struct Platform
        {
            public int id;
            public int height;
            public int leftEdge;
            public int rightEdge;
            public int allowedHeight;
            public List<Move> moves;
            public platformType type;

            public Platform(platformType type, int height, int leftEdge, int rightEdge, List<Move> moves, int allowedHeight)
            {
                this.id = 0;
                this.type = type;
                this.moves = moves;
                this.height = height;
                this.leftEdge = leftEdge;
                this.rightEdge = rightEdge;
                this.allowedHeight = allowedHeight * LevelRepresentation.PIXEL_LENGTH;
            }
        }

        public struct Move
        {
            public Platform reachablePlatform;
            public LevelRepresentation.Point movePoint;
            public LevelRepresentation.Point landPoint;
            public int velocityX;
            public bool rightMove;
            public movementType movementType;
            public bool[] collectibles_onPath;
            public int pathLength;
            public bool collideCeiling;
            public int height;


            public Move(Platform reachablePlatform, LevelRepresentation.Point movePoint, LevelRepresentation.Point landPoint, int velocityX, bool rightMove, movementType movementType, bool[] collectibles_onPath, int pathLength, bool collideCeiling, int height = GameInfo.SQUARE_HEIGHT)
            {
                this.reachablePlatform = reachablePlatform;
                this.movePoint = movePoint;
                this.landPoint = landPoint;
                this.velocityX = velocityX;
                this.rightMove = rightMove;
                this.movementType = movementType;
                this.collectibles_onPath = collectibles_onPath;
                this.pathLength = pathLength;
                this.collideCeiling = collideCeiling;
                this.height = height;
            }
        }

        public void SetupGraph(int[,] levelArray, int nCollectibles)
        {
            this.platforms = new List<Platform>();
            this.levelArray = levelArray;
            this.nCollectibles = nCollectibles;

            SetupPlatforms();
            SetupMoves();
        }

        public abstract void SetupPlatforms();

        public void SetupMoves()
        {
            foreach (Platform fromPlatform in platforms)
            {
                SetMoves_Morph(fromPlatform);
                SetMoves_Collect(fromPlatform);
                SetMoves_StairOrGap(fromPlatform);

                Parallel.For(0, (GameInfo.MAX_VELOCITYX / VELOCITYX_STEP), k =>
                {
                    SetMoveInfoList_Jump(fromPlatform, VELOCITYX_STEP * k);

                    bool success_fall = false;

                    for (int height = Math.Min(max_height, GameInfo.SQUARE_HEIGHT); height >= Math.Min(max_height, GameInfo.SQUARE_HEIGHT) && !success_fall; height -= 8)
                    {
                        LevelRepresentation.Point movePoint = new LevelRepresentation.Point(fromPlatform.rightEdge + LevelRepresentation.PIXEL_LENGTH, fromPlatform.height - (height / 2));
                        success_fall = SetMoves_Fall(fromPlatform, movePoint, height, VELOCITYX_STEP * k, true, movementType.FALL);
                    }

                    success_fall = false;

                    for (int height = Math.Min(max_height, GameInfo.SQUARE_HEIGHT); height >= Math.Min(max_height, GameInfo.SQUARE_HEIGHT) && !success_fall; height -= 8)
                    {
                        LevelRepresentation.Point movePoint = new LevelRepresentation.Point(fromPlatform.leftEdge - LevelRepresentation.PIXEL_LENGTH, fromPlatform.height - (height / 2));
                        success_fall = SetMoves_Fall(fromPlatform, movePoint, height, VELOCITYX_STEP * k, false, movementType.FALL);
                    }
                });
            }
        }

        protected void SetMoves_Collect(Platform fromPlatform)
        {

            int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);
            int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);

            Parallel.For(0, (to - from) / (LevelRepresentation.PIXEL_LENGTH * 2) + 1, j =>
            {
                for (int height = min_height; height <= max_height; height += 8)
                {
                    LevelRepresentation.Point movePoint = new LevelRepresentation.Point(from + j * LevelRepresentation.PIXEL_LENGTH * 2, fromPlatform.height - (height / 2));
                    List<LevelRepresentation.ArrayPoint> pixels = GetFormPixels(movePoint, height);

                    if (!IsObstacle_onPixels(pixels))
                    {
                        bool[] collectible_onPath = GetCollectibles_onPixels(pixels);
                        AddMoveInfoToList(fromPlatform, new Graph.Move(fromPlatform, movePoint, movePoint, 0, true, movementType.COLLECT, collectible_onPath, 0, false, height));
                    }
                }
            });

        }

        protected abstract void SetMoves_StairOrGap(Platform fromPlatform);

        protected virtual void SetMoves_Morph(Platform fromPlatform)
        {
            return;
        }

        protected virtual void SetMoveInfoList_Jump(Platform fromPlatform, int velocityX)
        {
            return;
        }

        protected virtual void SetMoves_GapFall(Platform fromPlatform)
        {
            return;
        }

        protected bool SetMoves_Fall(Platform fromPlatform, LevelRepresentation.Point movePoint, int height, int velocityX, bool rightMove, movementType movementType)
        {

            if (!IsEnoughLengthToAccelerate(fromPlatform, movePoint, velocityX, rightMove))
            {
                return false;
            }

            bool[] collectible_onPath = new bool[nCollectibles];
            float pathLength = 0;

            LevelRepresentation.Point collidePoint = movePoint;
            LevelRepresentation.Point prevCollidePoint;

            collideType collideType = collideType.OTHER;
            float collideVelocityX = rightMove ? velocityX : -velocityX;
            float collideVelocityY = (movementType == movementType.JUMP) ? GameInfo.JUMP_VELOCITYY : GameInfo.FALL_VELOCITYY;
            bool collideCeiling = false;

            do
            {
                prevCollidePoint = collidePoint;

                GetPathInfo(collidePoint, collideVelocityX, collideVelocityY, ref collidePoint, ref collideType, ref collideVelocityX, ref collideVelocityY, ref collectible_onPath, ref pathLength, (Math.Min(area / height, height) / 2));

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

                Platform? toPlatform = GetPlatform(collidePoint, height);

                if (toPlatform.HasValue)
                {
                    if (movementType == movementType.FALL)
                    {
                        movePoint.x = rightMove ? movePoint.x - LevelRepresentation.PIXEL_LENGTH : movePoint.x + LevelRepresentation.PIXEL_LENGTH;
                    }

                    AddMoveInfoToList(fromPlatform, new Move(toPlatform.Value, movePoint, collidePoint, velocityX, rightMove, movementType, collectible_onPath, (int)pathLength, collideCeiling, height));

                    return true;
                }
            }

            return false;
        }

        public Platform? GetPlatform(LevelRepresentation.Point center, float height)
        {
            foreach (Platform i in platforms)
            {
                if (i.leftEdge <= center.x && center.x <= i.rightEdge && (i.height - center.y >= (height / 2) - 8) && (i.height - center.y <= (height/2) + 8))
                {
                    return i;
                }
            }
            return null;
        }

        public void SetPlatformID()
        {
            platforms.Sort((a, b) => {
                int result = a.height - b.height;
                return result != 0 ? result : a.leftEdge - b.leftEdge;
            });

            Parallel.For(0, platforms.Count, i =>
            {
               Platform tempPlatfom = platforms[i];
               tempPlatfom.id = i + 1;
                platforms[i] = tempPlatfom;
            });
        }

        protected bool IsStairOrGap(Platform fromPlatform, Platform toPlatform, ref bool rightMove)
        {
            if (0 <= toPlatform.leftEdge - fromPlatform.rightEdge && toPlatform.leftEdge - fromPlatform.rightEdge <= STAIR_MAXWIDTH)
            {
                if (0 <= (fromPlatform.height - toPlatform.height) && (fromPlatform.height - toPlatform.height) <= STAIR_MAXHEIGHT)
                {
                    rightMove = true;
                    return true;
                }
            }

            if (0 <= fromPlatform.leftEdge - toPlatform.rightEdge && fromPlatform.leftEdge - toPlatform.rightEdge <= STAIR_MAXWIDTH)
            {
                if (0 <= (fromPlatform.height - toPlatform.height) && (fromPlatform.height - toPlatform.height) <= STAIR_MAXHEIGHT)
                {
                    rightMove = false;
                    return true;
                }
            }

            return false;
        }

        protected bool IsEnoughLengthToAccelerate(Platform fromPlatform, LevelRepresentation.Point movePoint, int velocityX, bool rightMove)
        {
            int neededLengthToAccelerate;

            neededLengthToAccelerate = LENGTH_TO_ACCELERATE[velocityX / VELOCITYX_STEP];

            if (rightMove)
            {
                if (movePoint.x - fromPlatform.leftEdge < neededLengthToAccelerate)
                {
                    return false;
                }
            }
            else
            {
                if (fromPlatform.rightEdge - movePoint.x < neededLengthToAccelerate)
                {
                    return false;
                }
            }

            return true;
        }

        protected collideType GetCollideType(LevelRepresentation.Point center, bool ascent, bool rightMove, int radius)
        {
            LevelRepresentation.ArrayPoint centerArray = LevelRepresentation.ConvertPointIntoArrayPoint(center, false, false);
            int highestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y - radius, false);
            int lowestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y + radius, true);

            if (!ascent)
            {
                if (levelArray[lowestY, centerArray.xArray] == LevelRepresentation.BLACK)
                {
                    return collideType.FLOOR;
                }
            }
            else
            {
                if (levelArray[highestY, centerArray.xArray] == LevelRepresentation.BLACK)
                {
                    return collideType.CEILING;
                }
            }         

            return collideType.OTHER;
        }

        protected abstract bool IsObstacle_onPixels(List<LevelRepresentation.ArrayPoint> checkPixels);

        protected bool[] GetCollectibles_onPixels(List<LevelRepresentation.ArrayPoint> checkPixels)
        {
            bool[] collectible_onPath = new bool[nCollectibles];

            foreach (LevelRepresentation.ArrayPoint i in checkPixels)
            {
                if (!(levelArray[i.yArray, i.xArray] == LevelRepresentation.BLACK || levelArray[i.yArray, i.xArray] == LevelRepresentation.OPEN))
                {
                    collectible_onPath[levelArray[i.yArray, i.xArray] - 1] = true;
                }
            }

            return collectible_onPath;
        }

        protected void AddMoveInfoToList(Platform fromPlatform, Move mI)
        {
            lock (platforms)
            {
                List<Move> moveInfoToRemove = new List<Move>();

                if (IsPriorityHighest(fromPlatform, mI, ref moveInfoToRemove))
                {
                    fromPlatform.moves.Add(mI);
                }

                foreach (Move i in moveInfoToRemove)
                {
                    fromPlatform.moves.Remove(i);
                }
            }
        }

        protected bool IsPriorityHighest(Platform fromPlatform, Move mI, ref List<Move> moveInfoToRemove)
        {

            // if the move is to the same platform and there is no collectible
            if (fromPlatform.id == mI.reachablePlatform.id && !Utilities.IsTrueValue_inMatrix(mI.collectibles_onPath))
            {
                return false;
            }

            bool priorityHighestFlag = true;

            foreach (Move i in fromPlatform.moves)
            {

                // finds the reachable platform
                if (!(mI.reachablePlatform.id == i.reachablePlatform.id))
                {
                    continue;
                }
                
                Utilities.numTrue trueNum = Utilities.CompTrueNum(mI.collectibles_onPath, i.collectibles_onPath);

                if (trueNum == Utilities.numTrue.MORETRUE)
                {
                    // actions have higher priority than no actions
                    if (mI.movementType != movementType.COLLECT && i.movementType == movementType.COLLECT)
                    {
                        continue;
                    }

                    // comparison between no action movements
                    else if (mI.movementType != movementType.COLLECT && i.movementType != movementType.COLLECT)
                    {
                        if (mI.movementType > i.movementType)
                        {
                            continue;
                        }

                        if (mI.velocityX > i.velocityX)
                        {
                            continue;
                        }
                    }

                    moveInfoToRemove.Add(i);
                    continue;
                }

                if (trueNum == Utilities.numTrue.LESSTRUE)
                {
                    if (mI.movementType == movementType.COLLECT && i.movementType != movementType.COLLECT)
                    {
                        continue;
                    }
                    else if (mI.movementType != movementType.COLLECT && i.movementType != movementType.COLLECT)
                    {
                        if (mI.movementType < i.movementType)
                        {
                            continue;
                        }

                        if (mI.velocityX < i.velocityX)
                        {
                            continue;
                        }
                    }

                    priorityHighestFlag = false;
                    continue;
                }

                if (trueNum == Utilities.numTrue.DIFFERENTTRUE)
                {
                    continue;
                }

                if (trueNum == Utilities.numTrue.SAMETRUE)
                {
                    if (mI.movementType == movementType.COLLECT && i.movementType == movementType.COLLECT)
                    {
                        int middlePos = (mI.reachablePlatform.rightEdge + mI.reachablePlatform.leftEdge) / 2;

                        if (Math.Abs(middlePos - mI.landPoint.x) > Math.Abs(middlePos - i.landPoint.x))
                        {
                            priorityHighestFlag = false;
                            continue;
                        }

                        if (i.height == GameInfo.SQUARE_HEIGHT ||
                            (Math.Abs(i.height - GameInfo.SQUARE_HEIGHT) < Math.Abs(mI.height - GameInfo.SQUARE_HEIGHT)))
                        {
                            priorityHighestFlag = false;
                            continue;
                        }

                        moveInfoToRemove.Add(i);
                        continue;
                    }

                    if (mI.movementType == movementType.COLLECT && i.movementType != movementType.COLLECT)
                    {
                        moveInfoToRemove.Add(i);
                        continue;
                    }

                    if (mI.movementType != movementType.COLLECT && i.movementType == movementType.COLLECT)
                    {
                        priorityHighestFlag = false;
                        continue;
                    }

                    if (mI.movementType != movementType.COLLECT && i.movementType != movementType.COLLECT)
                    {
                        if (mI.rightMove == i.rightMove || ((mI.movementType == movementType.JUMP && i.movementType == movementType.JUMP) && (mI.velocityX == 0 || i.velocityX == 0)))
                        {
                            if (mI.movementType > i.movementType)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (mI.movementType < i.movementType)
                            {
                                moveInfoToRemove.Add(i);
                                continue;
                            }

                            if (i.velocityX == 0 && mI.velocityX > 0)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (Math.Abs(mI.height - GameInfo.SQUARE_HEIGHT) > Math.Abs(i.height - GameInfo.SQUARE_HEIGHT))
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (Math.Abs(i.height - GameInfo.SQUARE_HEIGHT) > Math.Abs(mI.height - GameInfo.SQUARE_HEIGHT))
                            {
                                moveInfoToRemove.Add(i);
                                continue;
                            }

                            if (mI.velocityX > i.velocityX)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (mI.velocityX < i.velocityX)
                            {
                                moveInfoToRemove.Add(i);
                                continue;
                            }

                            int middlePos = (mI.reachablePlatform.rightEdge + mI.reachablePlatform.leftEdge) / 2;

                            if (Math.Abs(middlePos - mI.landPoint.x) > Math.Abs(middlePos - i.landPoint.x))
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            moveInfoToRemove.Add(i);
                            continue;
                        }

                    }
                }              
            }

            return priorityHighestFlag;
        }

        protected void GetPathInfo(LevelRepresentation.Point movePoint, float velocityX, float velocityY,
            ref LevelRepresentation.Point collidePoint, ref collideType collideType, ref float collideVelocityX, ref float collideVelocityY, ref bool[] collectible_onPath, ref float pathLength, int radius)
        {
            LevelRepresentation.Point previousCenter;
            LevelRepresentation.Point currentCenter = movePoint;

            for (int i = 1; true; i++)
            {
                float currentTime = i * TIME_STEP;

                previousCenter = currentCenter;
                currentCenter = GetCurrentCenter(movePoint, velocityX, velocityY, currentTime);
                List<LevelRepresentation.ArrayPoint> pixels = GetCirclePixels(currentCenter, radius);

                if (IsObstacle_onPixels(pixels))
                {
                    collidePoint = previousCenter;
                    collideType = GetCollideType(currentCenter, velocityY - GameInfo.GRAVITY * (i - 1) * TIME_STEP >= 0, velocityX > 0, radius);

                     if (collideType == collideType.CEILING)
                    {
                        collideVelocityX = velocityX / 3;
                        collideVelocityY = -(velocityY - GameInfo.GRAVITY * (i - 1) * TIME_STEP) / 3;
                    }
                    else
                    {
                        collideVelocityX = 0;
                        collideVelocityY = 0;
                    }

                    return;
                }

                collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(pixels));

                pathLength += (float)Math.Sqrt(Math.Pow(currentCenter.x - previousCenter.x, 2) + Math.Pow(currentCenter.y - previousCenter.y, 2));
            }
        }

        protected LevelRepresentation.Point GetCurrentCenter(LevelRepresentation.Point movePoint, float velocityX, float velocityY, float currentTime)
        {
            float distanceX = velocityX * currentTime;
            float distanceY = -velocityY * currentTime + GameInfo.GRAVITY * (float)Math.Pow(currentTime, 2) / 2;

            return new LevelRepresentation.Point((int)(movePoint.x + distanceX), (int)(movePoint.y + distanceY));
        }

        protected abstract List<LevelRepresentation.ArrayPoint> GetFormPixels(LevelRepresentation.Point center, int height);

        protected List<LevelRepresentation.ArrayPoint> GetCirclePixels(LevelRepresentation.Point circleCenter, int radius)
        {
            List<LevelRepresentation.ArrayPoint> circlePixels = new List<LevelRepresentation.ArrayPoint>();

            LevelRepresentation.ArrayPoint circleCenterArray = LevelRepresentation.ConvertPointIntoArrayPoint(circleCenter, false, false);
            int circleHighestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(circleCenter.y - radius, false);
            int circleLowestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(circleCenter.y + radius, true);


            for (int i = circleHighestY; i <= circleLowestY; i++)
            {
                float circleWidth;

                if (i < circleCenterArray.yArray)
                {
                    circleWidth = (float)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(LevelRepresentation.ConvertValue_ArrayPointIntoPoint(i + 1) - circleCenter.y, 2));
                }
                else if (i > circleCenterArray.yArray)
                {
                    circleWidth = (float)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(LevelRepresentation.ConvertValue_ArrayPointIntoPoint(i) - circleCenter.y, 2));
                }
                else
                {
                    circleWidth = radius;
                }

                int circleLeftX = LevelRepresentation.ConvertValue_PointIntoArrayPoint((int)(circleCenter.x - circleWidth), false);
                int circleRightX = LevelRepresentation.ConvertValue_PointIntoArrayPoint((int)(circleCenter.x + circleWidth), true);

                for (int j = circleLeftX; j <= circleRightX; j++)
                {
                    circlePixels.Add(new LevelRepresentation.ArrayPoint(j, i));
                }
            }

            return circlePixels;
        }

    }
}
