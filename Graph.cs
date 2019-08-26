using System;
using System.Collections.Generic;

using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    public abstract class Graph
    {

        public enum collideType
        {
            CEILING, FLOOR, RECTANGLE, OTHER
        };

        public enum movementType
        {
            COLLECT, RIDE, RIDING, TRANSITION, FALL, JUMP, GAP
        };

        public enum platformType
        {
            NO_PLATFORM, BLACK, GREEN, YELLOW, GAP, RECTANGLE
        };


        public int OBSTACLE_COLOUR;
        public int[] POSSIBLE_HEIGHTS;
        public const int VELOCITYX_STEP = 20;
        public const float TIME_STEP = 0.01f;
        protected const int STAIR_MAXWIDTH = 48;
        protected const int STAIR_MAXHEIGHT = 16;
        protected int[] LENGTH_TO_ACCELERATE = new int[10] { 1, 5, 13, 20, 31, 49, 70, 95, 128, 166 };

        public int[,] levelArray;
        public int AREA, nCollectibles;
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
                this.allowedHeight = allowedHeight;
            }
        }

        public struct State
        {
            public int height;
            public Point position;
            public bool right_direction;
            public int horizontal_velocity;

            public State(Point position, int height, int horizontal_velocity, bool right_direction)
            {
                this.height = height;
                this.position = position;
                this.right_direction = right_direction;
                this.horizontal_velocity = horizontal_velocity;
            }
        }

        public struct Move
        {
            public State state;
            public int pathLength;
            public Point landPoint;
            public movementType type;
            public State partner_state;
            public bool collideCeiling;
            public bool[] collectibles_onPath;
            public Platform reachablePlatform;

            public Move(Platform reachablePlatform, State state, Point landPoint, movementType type, bool[] collectibles_onPath, int pathLength, bool collideCeiling, State? partner_state = null)
            {
                this.type = type;
                this.state = state;
                this.landPoint = landPoint;
                this.pathLength = pathLength;
                this.collideCeiling = collideCeiling;
                this.reachablePlatform = reachablePlatform;
                this.collectibles_onPath = collectibles_onPath;
                this.partner_state = partner_state ?? new State();
            }
        }

        public Graph(int area, int[] possible_heights, int obstacle_colour)
        {
            AREA = area;
            OBSTACLE_COLOUR = obstacle_colour;
            POSSIBLE_HEIGHTS = possible_heights;
        }

        public void SetupGraph(int[,] levelArray, int nCollectibles)
        {
            this.levelArray = levelArray;
            this.nCollectibles = nCollectibles;
            possibleCollectibles = new bool[nCollectibles];
            SetupPlatforms();
            SetupMoves();
        }

        public abstract void SetupPlatforms();
        public abstract void SetupMoves();
        public abstract List<ArrayPoint> GetFormPixels(Point center, int height);

        public Platform? GetPlatform(Point center, float height, int velocityY = 0)
        {
            if (Math.Abs(velocityY) <= GameInfo.MAX_VELOCITYY)
                foreach (Platform i in platforms)
                    if (i.leftEdge <= center.x && center.x <= i.rightEdge && (i.height - center.y >= (height / 2) - 8) && (i.height - center.y <= (height / 2) + 8))
                        return i;
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

        public bool EnoughLength(Platform fromPlatform, State state)
        {
            int neededLengthToAccelerate;

            neededLengthToAccelerate = LENGTH_TO_ACCELERATE[state.horizontal_velocity / VELOCITYX_STEP];

            if (state.right_direction)
            {
                if (state.position.x - fromPlatform.leftEdge < neededLengthToAccelerate)
                {
                    return false;
                }
            }
            else
            {
                if (fromPlatform.rightEdge - state.position.x < neededLengthToAccelerate)
                {
                    return false;
                }
            }

            return true;
        }

        public bool[] CollectiblesOnPixels(List<ArrayPoint> pixels)
        {
            return CollectiblesOnPixels(levelArray, pixels, nCollectibles);
        }

        public static bool[] CollectiblesOnPixels(int[,] levelArray, List<ArrayPoint> pixels, int nCollectibles)
        {
            bool[] collectible_onPath = new bool[nCollectibles];

            foreach (ArrayPoint i in pixels)
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

                        if (mI.state.horizontal_velocity > i.state.horizontal_velocity)
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

                        if (mI.state.horizontal_velocity < i.state.horizontal_velocity)
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

                        if (i.state.height == GameInfo.SQUARE_HEIGHT ||
                            (Math.Abs(i.state.height - GameInfo.SQUARE_HEIGHT) < Math.Abs(mI.state.height - GameInfo.SQUARE_HEIGHT)))
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
                        if (mI.state.right_direction == i.state.right_direction || ((mI.type == movementType.JUMP && i.type == movementType.JUMP) && (mI.state.horizontal_velocity == 0 || i.state.horizontal_velocity == 0)))
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

                            if (i.state.horizontal_velocity == 0 && mI.state.horizontal_velocity > 0)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (Math.Abs(mI.state.height - GameInfo.SQUARE_HEIGHT) > Math.Abs(i.state.height - GameInfo.SQUARE_HEIGHT))
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (Math.Abs(i.state.height - GameInfo.SQUARE_HEIGHT) > Math.Abs(mI.state.height - GameInfo.SQUARE_HEIGHT))
                            {
                                moveInfoToRemove.Add(i);
                                continue;
                            }

                            if (mI.state.horizontal_velocity > i.state.horizontal_velocity)
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (mI.state.horizontal_velocity < i.state.horizontal_velocity)
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

            if (p.type != platformType.RECTANGLE || cooperation)
            {
                platformsChecked[p.id - 1] = true;

                foreach (Move m in p.moves)
                {

                    if (m.reachablePlatform.type != platformType.RECTANGLE || m.reachablePlatform.id == p.id)
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
            return new Move(m.reachablePlatform, m.state, m.landPoint, m.type, m.collectibles_onPath, m.pathLength, m.collideCeiling, m.partner_state);
        }

        public bool ObstacleOnPixels(List<ArrayPoint> checkPixels)
        {
            return ObstacleOnPixels(levelArray, checkPixels, OBSTACLE_COLOUR);
        }

        public static bool ObstacleOnPixels(int[,] levelArray, List<ArrayPoint> checkPixels, int obstacle_colour)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == BLACK || levelArray[i.yArray, i.xArray] == obstacle_colour)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
