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
        bool[] checked_platforms;
        int[,] previous_levelArray;
        public State initial_rectangle_state;

        public GraphCircle() : base(CIRCLE_AREA, new int[1] { CIRCLE_HEIGHT }, GREEN) { }

        public override void SetupPlatforms()
        {
            OBSTACLE_COLOUR = YELLOW;

            platforms = SetPlatforms_Rectangle(levelArray);
            Setup_Rectangle(this);
            platforms = DeleteUnreachablePlatforms(platforms, initial_rectangle_state);

            OBSTACLE_COLOUR = GREEN;

            List<Platform> platformsCircle = SetPlatforms_Circle(levelArray);
            platforms = JoinPlatforms(platformsCircle, platforms);
            platforms = SetPlatformsID(platforms);

            previous_levelArray = new int[levelArray.GetLength(0), levelArray.GetLength(1)];
            Array.Copy(levelArray, previous_levelArray, levelArray.Length);
            levelArray = SetCooperation(levelArray, platforms);

            checked_platforms = new bool[platforms.Count];
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

        public void SetPossibleCollectibles(State st)
        {

            Platform? platform = GetPlatform(st.GetPosition(), st.height);

            if (platform.HasValue)
            {
                checked_platforms = CheckCollectiblesPlatform(checked_platforms, platform.Value);
            }
        }

        public override List<ArrayPoint> GetFormPixels(Point center, int height)
        {
            return GetCirclePixels(center, height/2);
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

            checked_platforms = CheckCollectiblesPlatform(checked_platforms, cooperationMove.reachablePlatform, true);

            collectibles = Utilities.GetOrMatrix(collectibles, Utilities.GetXorMatrix(collectiblesWithoutCooperation, possibleCollectibles));

            Array.Copy(collectiblesWithoutCooperation, possibleCollectibles, possibleCollectibles.Length);

            return collectibles;
        }

    }
}
