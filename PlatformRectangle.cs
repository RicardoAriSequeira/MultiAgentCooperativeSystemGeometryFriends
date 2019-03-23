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
            NO_ACTION, STAIR_GAP, FALL, MORPH_DOWN, MORPH_UP
        };

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
            /*
            int[,] platformArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int i = 0; i < levelArray.GetLength(0); i++)
            {
                Parallel.For(0, levelArray.GetLength(1), j =>
                {

                    LevelArray.Point rectangleCenter = LevelArray.ConvertArrayPointIntoPoint(new LevelArray.ArrayPoint(j, i));



                });
            }
            */
        }

        public void SetMoveInfoList(int[,] levelArray, int numCollectibles)
        {
        }
    }
}
