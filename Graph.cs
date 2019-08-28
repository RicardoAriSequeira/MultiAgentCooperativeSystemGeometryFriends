﻿using System;
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
            COLLECT, RIDE, RIDING, TRANSITION, FALL, JUMP
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
        protected static int[] LENGTH_TO_ACCELERATE = new int[10] { 1, 5, 13, 20, 31, 49, 70, 95, 128, 166 };

        public bool dynamic_change = false;
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
            public int x;
            public int y;
            public int v_x;
            public int v_y;
            public int height;

            public State(int x, int y, int v_x, int v_y, int height)
            {
                this.x = x;
                this.y = y;
                this.v_x = v_x;
                this.v_y = v_y;
                this.height = height;
            }

            public Point GetPosition()
            {
                return new Point(x, y);
            }
        }

        public struct Move
        {
            public int length;
            public Point land;
            public State state;
            public Platform to;
            public bool ceiling;
            public movementType type;
            public State partner_state;
            public bool[] collectibles;

            public Move(Platform t, State st, Point l, movementType mov_t, bool[] cols, int lgth, bool c, State? p_st = null)
            {
                to = t;
                land = l;
                state = st;
                ceiling = c;
                type = mov_t;
                length = lgth;
                collectibles = cols;
                partner_state = p_st ?? new State();
            }

            public Move Copy()
            {
                return new Move(to, state, land, type, collectibles, length, ceiling, partner_state);
            }

            public bool ToTheRight() { return state.v_x >= 0; }


        }

        public Graph(int area, int[] possible_heights, int obstacle_colour)
        {
            AREA = area;
            OBSTACLE_COLOUR = obstacle_colour;
            POSSIBLE_HEIGHTS = possible_heights;
        }

        public abstract void SetupPlatforms();

        public abstract void SetupMoves();

        public abstract List<ArrayPoint> GetFormPixels(Point center, int height);

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

        public void Setup(int[,] levelArray, int nCollectibles)
        {
            this.levelArray = levelArray;
            this.nCollectibles = nCollectibles;
            possibleCollectibles = new bool[nCollectibles];
            SetupPlatforms();
            SetupMoves();
        }

        public Platform? GetPlatform(Point center, float height, int velocityY = 0)
        {
            if (Math.Abs(velocityY) <= GameInfo.MAX_VELOCITYY)
                foreach (Platform i in platforms)
                    if (i.leftEdge <= center.x && center.x <= i.rightEdge && (i.height - center.y >= (height / 2) - 8) && (i.height - center.y <= (height / 2) + 8))
                        return i;
            return null;
        }

        public bool HasChanged() { return dynamic_change; }

        public void Change() { dynamic_change = true; }

        public void Process() { dynamic_change = false; }

        public bool ObstacleOnPixels(List<ArrayPoint> checkPixels)
        {
            return ObstacleOnPixels(levelArray, checkPixels, OBSTACLE_COLOUR);
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

        public bool[] CollectiblesOnPixels(List<ArrayPoint> pixels)
        {
            return CollectiblesOnPixels(levelArray, pixels, nCollectibles);
        }

        public bool[] CheckCollectiblesPlatform(bool[] platformsChecked, Platform p, bool cooperation = false)
        {

            if (p.type != platformType.RECTANGLE || cooperation)
            {
                platformsChecked[p.id - 1] = true;

                foreach (Move m in p.moves)
                {

                    if (m.to.type != platformType.RECTANGLE || m.to.id == p.id)
                    {
                        possibleCollectibles = Utilities.GetOrMatrix(possibleCollectibles, m.collectibles);

                        if (!platformsChecked[m.to.id - 1])
                        {
                            platformsChecked = CheckCollectiblesPlatform(platformsChecked, m.to);
                        }
                    }

                }
            }

            return platformsChecked;
        }

        public static bool EnoughLength(Platform fromPlatform, State state)
        {

            int neededLengthToAccelerate = LENGTH_TO_ACCELERATE[Math.Abs(state.v_x) / VELOCITYX_STEP];

            if (state.v_x >= 0)
            {
                if (state.x - fromPlatform.leftEdge < neededLengthToAccelerate)
                {
                    return false;
                }
            }
            else
            {
                if (fromPlatform.rightEdge - state.x < neededLengthToAccelerate)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsPriorityHighest(Platform fromPlatform, Move mI, ref List<Move> moveInfoToRemove)
        {

            // if the move is to the same platform and there is no collectible
            if (fromPlatform.id == mI.to.id && !Utilities.IsTrueValue_inMatrix(mI.collectibles))
            {
                return false;
            }

            bool priorityHighestFlag = true;

            foreach (Move i in fromPlatform.moves)
            {

                // finds the reachable platform
                if (!(mI.to.id == i.to.id))
                {
                    continue;
                }

                Utilities.numTrue trueNum = Utilities.CompTrueNum(mI.collectibles, i.collectibles);

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

                        if (mI.state.v_x > i.state.v_x)
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

                        if (mI.state.v_x < i.state.v_x)
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
                        int middlePos = (mI.to.rightEdge + mI.to.leftEdge) / 2;

                        if (Math.Abs(middlePos - mI.land.x) > Math.Abs(middlePos - i.land.x))
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
                        if (mI.ToTheRight() == i.ToTheRight() || (mI.type == movementType.JUMP && i.type == movementType.JUMP && (mI.state.v_x == 0 || i.state.v_x == 0)))
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

                            if (i.state.v_x == 0 && mI.state.v_x > 0)
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

                            if (Math.Abs(mI.state.v_x) > Math.Abs(i.state.v_x))
                            {
                                priorityHighestFlag = false;
                                continue;
                            }

                            if (Math.Abs(mI.state.v_x) < Math.Abs(i.state.v_x))
                            {
                                moveInfoToRemove.Add(i);
                                continue;
                            }

                            int middlePos = (mI.to.rightEdge + mI.to.leftEdge) / 2;

                            if (Math.Abs(middlePos - mI.land.x) > Math.Abs(middlePos - i.land.x))
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
            int circleHighestY = PointToArrayPoint(circleCenter.y - radius, false);
            int circleLowestY = PointToArrayPoint(circleCenter.y + radius, true);


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

                int circleLeftX = PointToArrayPoint((int)(circleCenter.x - circleWidth), false);
                int circleRightX = PointToArrayPoint((int)(circleCenter.x + circleWidth), true);

                for (int j = circleLeftX; j <= circleRightX; j++)
                {
                    circlePixels.Add(new ArrayPoint(j, i));
                }
            }

            return circlePixels;
        }

    }
}
