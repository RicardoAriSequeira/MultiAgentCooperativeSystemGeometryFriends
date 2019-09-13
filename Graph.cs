using System;
using System.Collections.Generic;

using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    public abstract class Graph
    {

        public const int VELOCITYX_STEP = 20;
        public const float TIME_STEP = 0.01f;
        protected const int STAIR_MAXWIDTH = 48;
        protected const int STAIR_MAXHEIGHT = 16;
        protected static int[] LENGTH_TO_ACCELERATE = new int[10] { 1, 5, 13, 20, 31, 49, 70, 95, 128, 166 };

        public enum collideType
        {
            CEILING, FLOOR, RECTANGLE, OTHER
        };

        public enum movementType
        {
            COLLECT, COOPERATION, TRANSITION, FALL, JUMP
        };

        public enum platformType
        {
            NO_PLATFORM, BLACK, GAP, RECTANGLE
        };

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

            public Platform Copy()
            {
                return new Platform(type, height, leftEdge, rightEdge, moves, allowedHeight, id);
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

            public bool Equals(State s)
            {
                return x == s.x && y == s.y && v_x == s.v_x && v_y == s.v_y && height == s.height;
            }

            public State Copy()
            {
                return new State(x, y, v_x, v_y, height);
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

            public bool ToTheRight()
            {
                if (state.v_x == 0) return (land.x - state.x >= 0);
                return state.v_x >= 0;
            }

            public Move Copy()
            {
                return new Move(to, state, land, type, collectibles, length, ceiling, partner_state);
            }

            public int GetXToAccelerate()
            {
                int length = LENGTH_TO_ACCELERATE[Math.Abs(state.v_x) / VELOCITYX_STEP];
                return state.x + (ToTheRight() ? (- length) : length);
            }
        

        }

        public int[,] levelArray;
        public int obstacle_colour;
        public int[] possible_heights;
        public int area, nCollectibles;
        public List<Platform> platforms;
        public bool[] checked_platforms;
        public bool dynamic_change = false;
        public bool[] possibleCollectibles;
        public State initial_rectangle_state;

        public Graph(int area, int[] possible_heights, int obstacle_colour)
        {
            this.area = area;
            this.obstacle_colour = obstacle_colour;
            this.possible_heights = possible_heights;
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
            return ObstacleOnPixels(levelArray, checkPixels, obstacle_colour);
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

        public void SetPossibleCollectibles(State st)
        {

            Platform? platform = GetPlatform(st.GetPosition(), st.height);

            if (platform.HasValue)
            {
                checked_platforms = CheckCollectiblesPlatform(checked_platforms, platform.Value);
            }
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

                            // JUMPS FROM PLATFORMS TO RECTANGLE (RIDE)
                            if (fromPlatform.type != platformType.RECTANGLE &&
                                mI.type == movementType.JUMP &&
                                i.type == mI.type &&
                                mI.to.type == platformType.RECTANGLE &&
                                i.to.id == mI.to.id &&
                                mI.ToTheRight() == i.ToTheRight())
                            {

                                // SAME JUMP POINT
                                //if (mI.state.x == i.state.x)
                                //{

                                    if (Math.Abs(i.land.x - i.state.x) < 140)
                                    {

                                        if (Math.Abs(mI.land.x - mI.state.x) >= 140)
                                        {
                                            moveInfoToRemove.Add(i);
                                            continue;
                                        }

                                        else if (Math.Abs(mI.state.v_x) < Math.Abs(i.state.v_x))
                                        {
                                            moveInfoToRemove.Add(i);
                                            continue;
                                        }

                                        else
                                        {
                                            priorityHighestFlag = false;
                                            continue;
                                        }

                                    }
                                    else
                                    {
                                        if (Math.Abs(mI.land.x - mI.state.x) < 140)
                                        {
                                            priorityHighestFlag = false;
                                            continue;
                                        }

                                        else if (Math.Abs(mI.state.v_x) < Math.Abs(i.state.v_x))
                                        {
                                            moveInfoToRemove.Add(i);
                                            continue;
                                        }

                                        else
                                        {
                                            priorityHighestFlag = false;
                                            continue;
                                        }
                                    }
                                //}

                                //continue;

                            }

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

        public static bool CanRectangleMorphUp(int[,] levelArray, Point position)
        {

            List<ArrayPoint> pixelsToMorph = new List<ArrayPoint>();

            int lowestY = PointToArrayPoint(position.y + (GameInfo.MIN_RECTANGLE_HEIGHT / 2), false);
            int highestY = PointToArrayPoint(position.y - (200 - (GameInfo.MIN_RECTANGLE_HEIGHT / 2)), false);

            int rectangleWidth = GameInfo.RECTANGLE_AREA / GameInfo.MIN_RECTANGLE_HEIGHT;

            int lowestLeft = PointToArrayPoint(position.x - (rectangleWidth / 2), false);
            int highestLeft = PointToArrayPoint(position.x - (GameInfo.MIN_RECTANGLE_HEIGHT / 2), false);

            int lowestRight = PointToArrayPoint(position.x + (GameInfo.MIN_RECTANGLE_HEIGHT / 2), false);
            int highestRight = PointToArrayPoint(position.x + (rectangleWidth / 2), false);

            for (int y = highestY; y <= lowestY; y++)
            {
                for (int x = lowestLeft; x <= highestLeft; x++)
                {
                    pixelsToMorph.Add(new ArrayPoint(x, y));
                }

                for (int x = lowestRight; x <= highestRight; x++)
                {
                    pixelsToMorph.Add(new ArrayPoint(x, y));
                }

            }

            return !ObstacleOnPixels(levelArray, pixelsToMorph, YELLOW);
        }

        public Platform? GetRectanglePlatform(int x, int y, int height)
        {
            foreach (Platform p in platforms)
            {
                if (p.type == platformType.RECTANGLE && x >= p.leftEdge && x <= p.rightEdge && Math.Abs(y - (height / 2) - p.height) <= 8)
                {
                    return p;
                }
            }

            return null;
        }

    }
}
