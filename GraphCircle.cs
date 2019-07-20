using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    class GraphCircle : Graph
    {
  
        public GraphCircle() : base()
        {
            this.AREA = GameInfo.CIRCLE_AREA;
            this.MIN_HEIGHT = GameInfo.MIN_CIRCLE_HEIGHT;
            this.MAX_HEIGHT = GameInfo.MAX_CIRCLE_HEIGHT;
        }

        public override void SetupPlatforms()
        {
            platforms = PlatformIdentification.SetPlatforms_Circle(levelArray);
            //List<Platform> platformsCircle = PlatformIdentification.SetPlatformsAsCircle(levelArray);
            //List<Platform> platformsRectangle = PlatformIdentification.SetPlatformsAsRectangle(levelArray);

            //platforms = PlatformIdentification.JoinPlatforms(platformsCircle, platformsRectangle);

            platforms = PlatformIdentification.SetPlatformsID(platforms);
        }

        protected override void SetupMoves_Jump(Platform fromPlatform, int velocityX)
        {
            int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);
            int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);
            Parallel.For(0, (to - from) / (LevelRepresentation.PIXEL_LENGTH * 2) + 1, j =>
            {
                LevelRepresentation.Point movePoint = new LevelRepresentation.Point(from + j * LevelRepresentation.PIXEL_LENGTH * 2, fromPlatform.height - GameInfo.CIRCLE_RADIUS);
                MoveIdentification.Fall(this, fromPlatform, movePoint, MAX_HEIGHT, velocityX, true, movementType.JUMP);
                MoveIdentification.Fall(this, fromPlatform, movePoint, MAX_HEIGHT, velocityX, false, movementType.JUMP);
            });
        }

        protected override void SetupMoves_StairOrGap(Platform fromPlatform)
        {

            foreach (Platform toPlatform in platforms)
            {

                bool rightMove = false;
                bool obstacleFlag = false;
                bool[] collectible_onPath = new bool[nCollectibles];

                if (fromPlatform.Equals(toPlatform) || !IsStairOrGap(fromPlatform, toPlatform, ref rightMove))
                {
                    continue;
                }

                int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;

                for (int k = from; k <= to; k += LevelRepresentation.PIXEL_LENGTH)
                {
                    List<LevelRepresentation.ArrayPoint> circlePixels = GetCirclePixels(new LevelRepresentation.Point(k, toPlatform.height - GameInfo.CIRCLE_RADIUS), GameInfo.CIRCLE_RADIUS);

                    if (IsObstacle_onPixels(circlePixels))
                    {
                        obstacleFlag = true;
                        break;
                    }

                    collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, GetCollectibles_onPixels(circlePixels));
                }

                if (!obstacleFlag)
                {
                    LevelRepresentation.Point movePoint = rightMove ? new LevelRepresentation.Point(fromPlatform.rightEdge, fromPlatform.height) : new LevelRepresentation.Point(fromPlatform.leftEdge, fromPlatform.height);
                    LevelRepresentation.Point landPoint = rightMove ? new LevelRepresentation.Point(toPlatform.leftEdge, toPlatform.height) : new LevelRepresentation.Point(toPlatform.rightEdge, toPlatform.height);

                    AddMove(fromPlatform, new Move(toPlatform, movePoint, landPoint, 0, rightMove, movementType.STAIR_GAP, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false));
                }
            }
        }

        public override List<LevelRepresentation.ArrayPoint> GetFormPixels(LevelRepresentation.Point center, int height)
        {
            return GetCirclePixels(center, height/2);
        }

        public override bool IsObstacle_onPixels(List<LevelRepresentation.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelRepresentation.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelRepresentation.BLACK || levelArray[i.yArray, i.xArray] == LevelRepresentation.GREEN)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
