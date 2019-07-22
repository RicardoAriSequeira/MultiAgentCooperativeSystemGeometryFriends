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

        public override void SetupMoves()
        {
            foreach (Platform fromPlatform in platforms)
            {

                MoveIdentification.StairOrGap_Circle(this, fromPlatform);

                if (fromPlatform.type != platformType.GAP)
                {
                    MoveIdentification.Fall(this, fromPlatform);
                    MoveIdentification.Jump(this, fromPlatform);
                    MoveIdentification.Collect(this, fromPlatform);
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
