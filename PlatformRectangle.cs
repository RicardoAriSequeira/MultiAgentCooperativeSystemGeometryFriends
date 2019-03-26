using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class PlatformRectangle
    {
        public enum movementType
        {
            NO_ACTION, STAIR_GAP, FALL, MORPH_DOWN, MORPH_UP, JUMP
        };

        private int[] RECTANGLE_HEIGHTS = new int[8] { 192, 172, 152, 132, 112, 92, 72, 52 };
        private int RECTANGLE_AREA = 10000; // ou 9984?

        public struct PlatformInfo
        {
            public int id;
            public int height;
            public int leftEdge;
            public int rightEdge;
            public List<MoveInfo> moveInfoList;

            public PlatformInfo(int id, int height, int leftEdge, int rightEdge, List<MoveInfo> moveInfoList)
            {
                this.id = id;
                this.height = height;
                this.leftEdge = leftEdge;
                this.rightEdge = rightEdge;
                this.moveInfoList = moveInfoList;
            }
        }

        public struct MoveInfo
        {
            public PlatformInfo reachablePlatform;
            public LevelArray.Point movePoint;
            public LevelArray.Point landPoint;
            public int velocityX;
            public bool rightMove;
            public movementType movementType;
            public bool[] collectibles_onPath;
            public int pathLength;
            public bool collideCeiling;

            public MoveInfo(PlatformInfo reachablePlatform, LevelArray.Point movePoint, LevelArray.Point landPoint, int velocityX, bool rightMove, movementType movementType, bool[] collectibles_onPath, int pathLength, bool collideCeiling)
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
            }
        }

        private List<PlatformInfo> platformInfoList;

        public PlatformRectangle()
        {
            platformInfoList = new List<PlatformInfo>();
        }

        public void SetUp(int[,] levelArray, int numCollectibles)
        {
            SetPlatformInfoList(levelArray);
            SetMoveInfoList(levelArray, numCollectibles);
        }

        public void SetPlatformInfoList(int[,] levelArray)
        {

            int[,] platformArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int i = 0; i < levelArray.GetLength(0); i++)
            {
                Parallel.For(0, levelArray.GetLength(1), j =>
                {

                    foreach (int height in RECTANGLE_HEIGHTS)
                    {
                        LevelArray.Point rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(j, i));
                        rectangleCenter.y -= (height / 2);
                        List<LevelArray.ArrayPoint> rectanglePixels = GetRectanglePixels(rectangleCenter, height);

                        if (!IsObstacle_onPixels(levelArray, rectanglePixels))
                        {
                            if (levelArray[i, j - 1] == LevelArray.OBSTACLE || levelArray[i, j] == LevelArray.OBSTACLE)
                            {
                                platformArray[i, j] = LevelArray.OBSTACLE;
                            }
                        }
                    }
                });
            }

            Parallel.For(0, levelArray.GetLength(0), i =>
            {
                bool platformFlag = false;
                int height = 0, leftEdge = 0, rightEdge = 0;

                for (int j = 0; j < platformArray.GetLength(1); j++)
                {
                    if (platformArray[i, j] == LevelArray.OBSTACLE && !platformFlag)
                    {
                        height = LevelArray.ConvertValue_ArrayPointIntoPoint(i);
                        leftEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(j);
                        platformFlag = true;
                    }

                    if (platformArray[i, j] == LevelArray.OPEN && platformFlag)
                    {
                        rightEdge = LevelArray.ConvertValue_ArrayPointIntoPoint(j - 1);

                        if (rightEdge >= leftEdge)
                        {
                            lock (platformInfoList)
                            {
                                platformInfoList.Add(new PlatformInfo(0, height, leftEdge, rightEdge, new List<MoveInfo>()));
                            }
                        }

                        platformFlag = false;
                    }
                }
            });

            SetPlatformID();

        }

        private List<LevelArray.ArrayPoint> GetRectanglePixels(LevelArray.Point rectangleCenter, int height)
        {
            List<LevelArray.ArrayPoint> rectanglePixels = new List<LevelArray.ArrayPoint>();

            LevelArray.ArrayPoint rectangleCenterArray = LevelArray.ConvertPointIntoArrayPoint(rectangleCenter, false, false);
            int rectangleHighestY = LevelArray.ConvertValue_PointIntoArrayPoint(rectangleCenter.y - (height / 2), false);
            int rectangleLowestY = LevelArray.ConvertValue_PointIntoArrayPoint(rectangleCenter.y + (height / 2), true);


            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                float rectangleWidth = RECTANGLE_AREA / height;

                int rectangleLeftX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(rectangleCenter.x - (rectangleWidth / 2)), false); //+ 1
                int rectangleRightX = LevelArray.ConvertValue_PointIntoArrayPoint((int)(rectangleCenter.x + (rectangleWidth / 2)), true); //+ 1

                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new LevelArray.ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }

        public void SetPlatformID()
        {
            platformInfoList.Sort((a, b) =>
            {
                int result = a.height - b.height;
                return result != 0 ? result : a.leftEdge - b.leftEdge;
            });

            Parallel.For(0, platformInfoList.Count, i =>
            {
                PlatformInfo tempPlatfom = platformInfoList[i];
                tempPlatfom.id = i + 1;
                platformInfoList[i] = tempPlatfom;
            });
        }

        private bool IsObstacle_onPixels(int[,] levelArray, List<LevelArray.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelArray.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelArray.OBSTACLE)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetMoveInfoList(int[,] levelArray, int numCollectibles)
        {
            foreach (PlatformInfo i in platformInfoList)
            {

                int from = i.leftEdge + (i.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);
                int to = i.rightEdge - (i.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelArray.PIXEL_LENGTH * 2);

                Parallel.For(0, (to - from) / (LevelArray.PIXEL_LENGTH * 2) + 1, j =>
                {
                    LevelArray.Point movePoint;

                    movePoint = new LevelArray.Point(from + j * LevelArray.PIXEL_LENGTH * 2, i.height - GameInfo.CIRCLE_RADIUS);

                    SetMoveInfoList_NoAction(levelArray, i, movePoint, numCollectibles);
                });
            }
        }

        private void SetMoveInfoList_NoAction(int[,] levelArray, PlatformInfo fromPlatform, LevelArray.Point movePoint, int numCollectibles)
        {
            List<LevelArray.ArrayPoint> rectanglePixels = GetRectanglePixels(movePoint, 100);

            bool[] collectible_onPath = new bool[numCollectibles];

            collectible_onPath = GetCollectibles_onPixels(levelArray, rectanglePixels, collectible_onPath.Length);

            AddMoveInfoToList(fromPlatform, new MoveInfo(fromPlatform, movePoint, movePoint, 0, true, movementType.NO_ACTION, collectible_onPath, 0, false));
        }

        private bool[] GetCollectibles_onPixels(int[,] levelArray, List<LevelArray.ArrayPoint> checkPixels, int numCollectibles)
        {
            bool[] collectible_onPath = new bool[numCollectibles];

            foreach (LevelArray.ArrayPoint i in checkPixels)
            {
                if (!(levelArray[i.yArray, i.xArray] == LevelArray.OBSTACLE || levelArray[i.yArray, i.xArray] == LevelArray.OPEN))
                {
                    collectible_onPath[levelArray[i.yArray, i.xArray] - 1] = true;
                }
            }

            return collectible_onPath;
        }

        private void AddMoveInfoToList(PlatformInfo fromPlatform, MoveInfo mI)
        {
            lock (platformInfoList)
            {
                List<MoveInfo> moveInfoToRemove = new List<MoveInfo>();

                if (IsPriorityHighest(fromPlatform, mI, ref moveInfoToRemove))
                {
                    fromPlatform.moveInfoList.Add(mI);
                }

                foreach (MoveInfo i in moveInfoToRemove)
                {
                    fromPlatform.moveInfoList.Remove(i);
                }
            }
        }

        private bool IsPriorityHighest(PlatformInfo fromPlatform, MoveInfo mI, ref List<MoveInfo> moveInfoToRemove)
        {
            if (fromPlatform.id == mI.reachablePlatform.id && !Utilities.IsTrueValue_inMatrix(mI.collectibles_onPath))
            {
                return false;
            }

            bool priorityHighestFlag = true;

            foreach (MoveInfo i in fromPlatform.moveInfoList)
            {
                if (!(mI.reachablePlatform.id == i.reachablePlatform.id))
                {
                    continue;
                }

                Utilities.numTrue trueNum = Utilities.CompTrueNum(mI.collectibles_onPath, i.collectibles_onPath);

                if (trueNum == Utilities.numTrue.MORETRUE)
                {
                    if (mI.movementType != movementType.NO_ACTION && i.movementType == movementType.NO_ACTION)
                    {
                        continue;
                    }
                    else if (mI.movementType != movementType.NO_ACTION && i.movementType != movementType.NO_ACTION)
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
                    if (mI.movementType == movementType.NO_ACTION && i.movementType != movementType.NO_ACTION)
                    {
                        continue;
                    }
                    else if (mI.movementType != movementType.NO_ACTION && i.movementType != movementType.NO_ACTION)
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
                    if (mI.movementType == movementType.NO_ACTION && i.movementType == movementType.NO_ACTION)
                    {
                        int middlePos = (mI.reachablePlatform.rightEdge + mI.reachablePlatform.leftEdge) / 2;

                        if (Math.Abs(middlePos - mI.landPoint.x) > Math.Abs(middlePos - i.landPoint.x))
                        {
                            priorityHighestFlag = false;
                            continue;
                        }

                        moveInfoToRemove.Add(i);
                        continue;
                    }

                    if (mI.movementType == movementType.NO_ACTION && i.movementType != movementType.NO_ACTION)
                    {
                        moveInfoToRemove.Add(i);
                        continue;
                    }

                    if (mI.movementType != movementType.NO_ACTION && i.movementType == movementType.NO_ACTION)
                    {
                        priorityHighestFlag = false;
                        continue;
                    }

                    if (mI.movementType != movementType.NO_ACTION && i.movementType != movementType.NO_ACTION)
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
    }
}
