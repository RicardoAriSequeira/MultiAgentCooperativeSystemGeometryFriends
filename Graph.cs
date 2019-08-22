using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    public abstract class Graph
    {

        public enum collideType
        {
            CEILING, FLOOR, COOPERATION, OTHER
        };

        public enum movementType
        {
            COLLECT, RIDE, RIDING, MORPH_UP, MORPH_DOWN, STAIR_GAP, FALL, JUMP, GAP
        };

        public enum platformType
        {
            NO_PLATFORM, BLACK, GREEN, YELLOW, GAP, COOPERATION
        };

        public const int VELOCITYX_STEP = 20;

        public const float TIME_STEP = 0.01f;

        protected int[] LENGTH_TO_ACCELERATE = new int[10] { 1, 5, 13, 20, 31, 49, 70, 95, 128, 166 };

        protected const int STAIR_MAXWIDTH = 48;
        protected const int STAIR_MAXHEIGHT = 16;

        protected int[,] levelArray;
        public int AREA, MIN_HEIGHT, MAX_HEIGHT, nCollectibles;

        public List<Platform> platforms;
        public bool[] possibleCollectibles;

        public struct Platform
        {
            public int id;
            public int height;
            public int leftEdge;
            public int rightEdge;
            public int allowedHeight;
            public List<Move> moves;
            public platformType type;

            public Platform(platformType type, int height, int leftEdge, int rightEdge, List<Move> moves, int allowedHeight, int id = 0)
            {
                this.id = id;
                this.type = type;
                this.moves = moves;
                this.height = height;
                this.leftEdge = leftEdge;
                this.rightEdge = rightEdge;
                this.allowedHeight = allowedHeight * PIXEL_LENGTH;
            }
        }

        public struct PreCondition
        {
            public int height;
            public Point position;
            public bool right_direction;
            public int horizontal_velocity;

            public PreCondition(Point position, int height, int horizontal_velocity, bool right_direction)
            {
                this.height = height;
                this.position = position;
                this.right_direction = right_direction;
                this.horizontal_velocity = horizontal_velocity;
            }
        }

        public struct Move
        {
            public int pathLength;
            public Point landPoint;
            public movementType type;
            public bool collideCeiling;
            public PreCondition precondition;
            public bool[] collectibles_onPath;
            public Platform reachablePlatform;
            public PreCondition partner_precondition;

            public Move(Platform reachablePlatform, PreCondition precondition, Point landPoint, movementType type, bool[] collectibles_onPath, int pathLength, bool collideCeiling, PreCondition? partner_precondition = null)
            {
                this.type = type;
                this.landPoint = landPoint;
                this.pathLength = pathLength;
                this.precondition = precondition;
                this.collideCeiling = collideCeiling;
                this.reachablePlatform = reachablePlatform;
                this.collectibles_onPath = collectibles_onPath;
                this.partner_precondition = partner_precondition ?? new PreCondition();
            }
        }

        public void SetupGraph(int[,] levelArray, int nCollectibles)
        {
            this.platforms = new List<Platform>();
            this.levelArray = levelArray;
            this.nCollectibles = nCollectibles;
            this.possibleCollectibles = new bool[nCollectibles];

            SetupPlatforms();
            SetupMoves();
        }

        public abstract void SetupPlatforms();
        public abstract void SetupMoves();
        public abstract bool IsObstacle_onPixels(List<ArrayPoint> checkPixels);
        public abstract List<ArrayPoint> GetFormPixels(Point center, int height);
        protected abstract collideType GetCollideType(Point center, bool ascent, bool rightMove, int radius);

        public Platform? GetPlatform(Point center, float height, int velocityY = 0)
        {

            if (Math.Abs(velocityY) > GameInfo.MAX_VELOCITYY)
            {
                return null;
            }

            foreach (Platform i in platforms)
            {
                if (i.leftEdge <= center.x && center.x <= i.rightEdge && (i.height - center.y >= (height / 2) - 8) && (i.height - center.y <= (height/2) + 8))
                {
                    return i;
                }
            }

            return null;
        }

        public bool IsStairOrGap(Platform fromPlatform, Platform toPlatform, ref bool rightMove)
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

        public bool IsEnoughLengthToAccelerate(Platform fromPlatform, Point movePoint, int velocityX, bool rightMove)
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

        public bool[] GetCollectibles_onPixels(List<ArrayPoint> checkPixels)
        {
            bool[] collectible_onPath = new bool[nCollectibles];

            foreach (ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] > 0)
                {
                    collectible_onPath[levelArray[i.yArray, i.xArray] - 1] = true;
                }
            }

            return collectible_onPath;
        }

        public void AddMove(Platform fromPlatform, Move mI)
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

        protected static bool IsPriorityHighest(Platform fromPlatform, Move mI, ref List<Move> moveInfoToRemove)
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
                    if (mI.type != movementType.COLLECT && i.type == movementType.COLLECT)
                    {
                        continue;
                    }

                    // comparison between no action movements
                    else if (mI.type != movementType.COLLECT && i.type != movementType.COLLECT)
                    {
                        if (mI.type > i.type)
                        {
                            continue;
                        }

                        if (mI.precondition.horizontal_velocity > i.precondition.horizontal_velocity)
                        {
                            continue;
                        }
                    }

                    moveInfoToRemove.Add(i);
                    continue;
                }

                if (trueNum == Utilities.numTrue.LESSTRUE)
                {
                    if (mI.type == movementType.COLLECT && i.type != movementType.COLLECT)
                    {
                        continue;
                    }
                    else if (mI.type != movementType.COLLECT && i.type != movementType.COLLECT)
                    {
                        if (mI.type < i.type)
                        {
                            continue;
                        }

                        if (mI.precondition.horizontal_velocity < i.precondition.horizontal_velocity)
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
                    if (mI.type == movementType.COLLECT && i.type == movementType.COLLECT)
                    {
                        int middlePos = (mI.reachablePlatform.rightEdge + mI.reachablePlatform.leftEdge) / 2;

                        if (Math.Abs(middlePos - mI.landPoint.x) > Math.Abs(middlePos - i.landPoint.x))
                        {
                            priorityHighestFlag = false;
                            continue;
                        }

                        if (i.precondition.height == GameInfo.SQUARE_HEIGHT ||
                            (Math.Abs(i.precondition.height - GameInfo.SQUARE_HEIGHT) < Math.Abs(mI.precondition.height - GameInfo.SQUARE_HEIGHT)))
                        {
                            priorityHighestFlag = false;
                            continue;
                        }

                        moveInfoToRemove.Add(i);
                        continue;
                    }

                    if (mI.type == movementType.COLLECT && i.type != movementType.COLLECT)
                    {
                        moveInfoToRemove.Add(i);
                        continue;
                    }

                    if (mI.type != movementType.COLLECT && i.type == movementType.COLLECT)
                    {
                        priorityHighestFlag = false;
                        continue;
                    }

                    if (mI.type != movementType.COLLECT && i.type != movementType.COLLECT)
                    {
                        if (mI.precondition.right_direction == i.precondition.right_direction || ((mI.type == movementType.JUMP && i.type == movementType.JUMP) && (mI.precondition.horizontal_velocity == 0 || i.precondition.horizontal_velocity == 0)))
                        {
                            if (mI.type > i.type)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (mI.type < i.type)
                            {
                                moveInfoToRemove.Add(i);
                                continue;
                            }

                            if (i.precondition.horizontal_velocity == 0 && mI.precondition.horizontal_velocity > 0)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (Math.Abs(mI.precondition.height - GameInfo.SQUARE_HEIGHT) > Math.Abs(i.precondition.height - GameInfo.SQUARE_HEIGHT))
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (Math.Abs(i.precondition.height - GameInfo.SQUARE_HEIGHT) > Math.Abs(mI.precondition.height - GameInfo.SQUARE_HEIGHT))
                            {
                                moveInfoToRemove.Add(i);
                                continue;
                            }

                            if (mI.precondition.horizontal_velocity > i.precondition.horizontal_velocity)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (mI.precondition.horizontal_velocity < i.precondition.horizontal_velocity)
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

        public void GetPathInfo(Point movePoint, float velocityX, float velocityY,
            ref Point collidePoint, ref collideType collideType, ref float collideVelocityX, ref float collideVelocityY, ref bool[] collectible_onPath, ref float pathLength, int radius)
        {
            Point previousCenter;
            Point currentCenter = movePoint;

            for (int i = 1; true; i++)
            {
                float currentTime = i * TIME_STEP;

                previousCenter = currentCenter;
                currentCenter = GetCurrentCenter(movePoint, velocityX, velocityY, currentTime);
                List<ArrayPoint> pixels = GetCirclePixels(currentCenter, radius);
                
                ArrayPoint centerArray = ConvertPointIntoArrayPoint(currentCenter, false, false);
                int lowestY = ConvertValue_PointIntoArrayPoint(currentCenter.y + radius, true);
                bool ascent = velocityY - GameInfo.GRAVITY * (i - 1) * TIME_STEP >= 0;

                if (!ascent && (collideType != collideType.COOPERATION) && levelArray[lowestY, centerArray.xArray] == COOPERATION)
                {
                    collidePoint = previousCenter;
                    collideType = collideType.COOPERATION;
                    collideVelocityX = 0;
                    collideVelocityY = 0;
                    return;
                }

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

        public static Point GetCurrentCenter(Point movePoint, float velocityX, float velocityY, float currentTime)
        {
            float distanceX = velocityX * currentTime;
            float distanceY = -velocityY * currentTime + GameInfo.GRAVITY * (float)Math.Pow(currentTime, 2) / 2;

            return new Point((int)(movePoint.x + distanceX), (int)(movePoint.y + distanceY));
        }

        public static List<ArrayPoint> GetCirclePixels(Point circleCenter, int radius)
        {
            List<ArrayPoint> circlePixels = new List<ArrayPoint>();

            ArrayPoint circleCenterArray = ConvertPointIntoArrayPoint(circleCenter, false, false);
            int circleHighestY = ConvertValue_PointIntoArrayPoint(circleCenter.y - radius, false);
            int circleLowestY = ConvertValue_PointIntoArrayPoint(circleCenter.y + radius, true);


            for (int i = circleHighestY; i <= circleLowestY; i++)
            {
                float circleWidth;

                if (i < circleCenterArray.yArray)
                {
                    circleWidth = (float)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(ConvertValue_ArrayPointIntoPoint(i + 1) - circleCenter.y, 2));
                }
                else if (i > circleCenterArray.yArray)
                {
                    circleWidth = (float)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(ConvertValue_ArrayPointIntoPoint(i) - circleCenter.y, 2));
                }
                else
                {
                    circleWidth = radius;
                }

                int circleLeftX = ConvertValue_PointIntoArrayPoint((int)(circleCenter.x - circleWidth), false);
                int circleRightX = ConvertValue_PointIntoArrayPoint((int)(circleCenter.x + circleWidth), true);

                for (int j = circleLeftX; j <= circleRightX; j++)
                {
                    circlePixels.Add(new ArrayPoint(j, i));
                }
            }

            return circlePixels;
        }

        protected bool[] CheckCollectiblesPlatform(bool[] platformsChecked, Platform p, bool cooperation = false)
        {
            
            if (p.type != platformType.COOPERATION || cooperation)
            {
                platformsChecked[p.id - 1] = true;

                foreach (Move m in p.moves)
                {

                    if (m.reachablePlatform.type != platformType.COOPERATION || m.reachablePlatform.id == p.id)
                    {
                        possibleCollectibles = Utilities.GetOrMatrix(possibleCollectibles, m.collectibles_onPath);

                        if (!platformsChecked[m.reachablePlatform.id - 1])
                        {
                            platformsChecked = CheckCollectiblesPlatform(platformsChecked, m.reachablePlatform);
                        }
                    }
                    
                }
            }

            return platformsChecked;
        }

        public static Move CopyMove(Move m)
        {
            return new Move(m.reachablePlatform, m.precondition, m.landPoint, m.type, m.collectibles_onPath, m.pathLength, m.collideCeiling, m.partner_precondition);
        }

    }
}
