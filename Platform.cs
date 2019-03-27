using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    public abstract class Platform
    {
        public enum collideType
        {
            CEILING, FLOOR, OTHER
        };

        public enum movementType
        {
            NO_ACTION, STAIR_GAP, FALL, JUMP, MORPH_DOWN, MORPH_UP
        };
         
        public const int VELOCITYX_STEP = 20;

        protected const float TIME_STEP = 0.01f;

        protected int[] LENGTH_TO_ACCELERATE = new int[10] { 1, 5, 13, 20, 31, 49, 70, 95, 128, 166 };

        protected const int STAIR_MAXWIDTH = 48;
        protected const int STAIR_MAXHEIGHT = 16;

        protected int[] RECTANGLE_HEIGHTS = new int[8] { 192, 172, 152, 132, 112, 92, 72, 52 };
        protected int RECTANGLE_AREA = 10000; // ou 9984?

        protected List<PlatformInfo> platformInfoList;

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

        public Platform()
        {
            platformInfoList = new List<PlatformInfo>();
        }

        public void SetUp(int[,] levelArray, int numCollectibles)
        {
            SetPlatformInfoList(levelArray);
            SetMoveInfoList(levelArray, numCollectibles);
        }

        public abstract void SetPlatformInfoList(int[,] levelArray);

        public abstract void SetMoveInfoList(int[,] levelArray, int numCollectibles);

        public void SetPlatformID()
        {
            platformInfoList.Sort((a, b) => {
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

        protected bool IsStairOrGap(PlatformInfo fromPlatform, PlatformInfo toPlatform, ref bool rightMove)
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

        protected bool IsEnoughLengthToAccelerate(PlatformInfo fromPlatform, LevelArray.Point movePoint, int velocityX, bool rightMove)
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

        protected collideType GetCollideType(int[,] levelArray, LevelArray.Point center, bool ascent, bool rightMove, int height)
        {
            LevelArray.ArrayPoint centerArray = LevelArray.ConvertPointIntoArrayPoint(center, false, false);
            int highestY = LevelArray.ConvertValue_PointIntoArrayPoint(center.y - (height/2), false);
            int lowestY = LevelArray.ConvertValue_PointIntoArrayPoint(center.y + (height/2), true);

            if (!ascent)
            {
                if (levelArray[lowestY, centerArray.xArray] == LevelArray.OBSTACLE)
                {
                    return collideType.FLOOR;
                }
            }
            else
            {
                if (levelArray[highestY, centerArray.xArray] == LevelArray.OBSTACLE)
                {
                    return collideType.CEILING;
                }
            }         

            return collideType.OTHER;
        }

        protected bool IsObstacle_onPixels(int[,] levelArray, List<LevelArray.ArrayPoint> checkPixels)
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

        protected bool[] GetCollectibles_onPixels(int[,] levelArray, List<LevelArray.ArrayPoint> checkPixels, int numCollectibles)
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

        protected void AddMoveInfoToList(PlatformInfo fromPlatform, MoveInfo mI)
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

        protected bool IsPriorityHighest(PlatformInfo fromPlatform, MoveInfo mI, ref List<MoveInfo> moveInfoToRemove)
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

        protected LevelArray.Point GetCurrentCenter(LevelArray.Point movePoint, float velocityX, float velocityY, float currentTime)
        {
            float distanceX = velocityX * currentTime;
            float distanceY = -velocityY * currentTime + GameInfo.GRAVITY * (float)Math.Pow(currentTime, 2) / 2;

            return new LevelArray.Point((int)(movePoint.x + distanceX), (int)(movePoint.y + distanceY));
        }

    }
}
