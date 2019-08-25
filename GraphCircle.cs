using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;

using static GeometryFriendsAgents.GameInfo;
using static GeometryFriendsAgents.MoveIdentification;
using static GeometryFriendsAgents.LevelRepresentation;
using static GeometryFriendsAgents.PlatformIdentification;

namespace GeometryFriendsAgents
{
    class GraphCircle : Graph
    {
        bool[] platformsChecked;
        int[,] previous_levelArray;
        public RectangleRepresentation initialRectangleInfo;

        public GraphCircle() : base(CIRCLE_AREA, new int[1] { CIRCLE_HEIGHT }, GREEN) { }

        public override void SetupPlatforms()
        {
            OBSTACLE_COLOUR = YELLOW;

            platforms = SetPlatforms_Rectangle(levelArray);
            Setup_Rectangle(this);
            platforms = DeleteUnreachablePlatforms(platforms, initialRectangleInfo);

            OBSTACLE_COLOUR = GREEN;

            List<Platform> platformsCircle = SetPlatforms_Circle(levelArray);
            platforms = JoinPlatforms(platformsCircle, platforms);
            platforms = SetPlatformsID(platforms);

            previous_levelArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];
            Array.Copy(levelArray, previous_levelArray, levelArray.Length);
            levelArray = SetCooperation(levelArray, platforms);

            platformsChecked = new bool[platforms.Count];
        }

        public override void SetupMoves()
        {
            foreach (Platform from in platforms)
            {
                foreach (Platform to in platforms)
                    StairOrGap(this, from, to, CIRCLE_HEIGHT);

                Fall(this, from, CIRCLE_HEIGHT);
                Jump(this, from);
                Collect(this, from);

            }
        }

        public void SetPossibleCollectibles(CircleRepresentation initial_cI)
        {

            Platform? platform = GetPlatform(new Point((int)initial_cI.X, (int)initial_cI.Y), CIRCLE_HEIGHT);

            if (platform.HasValue)
            {
                platformsChecked = CheckCollectiblesPlatform(platformsChecked, platform.Value);
            }
        }

        public override List<ArrayPoint> GetFormPixels(Point center, int height)
        {
            return GetCirclePixels(center, height/2);
        }

        protected override collideType GetCollideType(Point center, bool ascent, bool rightMove, int radius)
        {
            LevelRepresentation.ArrayPoint centerArray = LevelRepresentation.ConvertPointIntoArrayPoint(center, false, false);
            int highestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y - radius, false);
            int lowestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y + radius, true);

            if (!ascent)
            {
                if (levelArray[lowestY, centerArray.xArray] == LevelRepresentation.BLACK || levelArray[lowestY, centerArray.xArray] == LevelRepresentation.GREEN)
                {
                    return collideType.FLOOR;
                }
            }
            else
            {
                if (levelArray[highestY, centerArray.xArray] == LevelRepresentation.BLACK || levelArray[lowestY, centerArray.xArray] == LevelRepresentation.GREEN)
                {
                    return collideType.CEILING;
                }
            }

            return collideType.OTHER;
        }

        public void DeleteCooperationPlatforms()
        {
            int p = 0;
            while (p < platforms.Count)
            {
                if (platforms[p].type == platformType.RECTANGLE)
                {
                    platforms.Remove(platforms[p]);
                }
                else
                {
                    platforms[p].moves.Clear();
                    p++;
                }
            }

            platforms = SetPlatformsID(platforms);
            levelArray = previous_levelArray;
            SetupMoves();
        }

        public bool[] GetCollectiblesFromCooperation(Move cooperationMove)
        {
            bool[] collectiblesWithoutCooperation = new bool[nCollectibles];
            Array.Copy(possibleCollectibles, collectiblesWithoutCooperation, possibleCollectibles.Length);

            bool[] collectibles = cooperationMove.collectibles_onPath;

            platformsChecked = CheckCollectiblesPlatform(platformsChecked, cooperationMove.reachablePlatform, true);

            collectibles = Utilities.GetOrMatrix(collectibles, Utilities.GetXorMatrix(collectiblesWithoutCooperation, possibleCollectibles));

            Array.Copy(collectiblesWithoutCooperation, possibleCollectibles, possibleCollectibles.Length);

            return collectibles;
        }

    }
}
