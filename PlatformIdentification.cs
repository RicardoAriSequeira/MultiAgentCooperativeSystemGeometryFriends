using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GeometryFriendsAgents.Graph;
using static GeometryFriendsAgents.GameInfo;
using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    static class PlatformIdentification
    {

        public static platformType[,] GetPlatformArray_Circle(int[,] levelArray)
        {
            platformType[,] platformArray = new platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    Point circleCenter = ConvertArrayPointIntoPoint(new ArrayPoint(x, y));
                    circleCenter.y -= GameInfo.CIRCLE_RADIUS;
                    List<ArrayPoint> circlePixels = GetCirclePixels(circleCenter, GameInfo.CIRCLE_RADIUS);

                    if (!CircleAgent.IsObstacle_onPixels(levelArray, circlePixels))
                    {
                        if (levelArray[y, x - 1] == BLACK || levelArray[y, x] == BLACK)
                        {
                            platformArray[y, x] = platformType.BLACK;
                        }
                        else if (levelArray[y, x - 1] == GREEN || levelArray[y, x] == GREEN)
                        {
                            platformArray[y, x] = platformType.GREEN;
                        }
                    }
                });
            }

            return platformArray;

        }

        public static platformType[,] GetPlatformArray_Rectangle(int[,] levelArray)
        {
            platformType[,] platformArray = new platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    if (levelArray[y, x] == BLACK || levelArray[y, x] == YELLOW)
                    {

                        // RECTANGLE WITH HEIGHT 100
                        Point rectangleCenter = ConvertArrayPointIntoPoint(new ArrayPoint(x, y));
                        rectangleCenter.y -= SQUARE_HEIGHT / 2;
                        List<ArrayPoint> rectanglePixels = RectangleAgent.GetFormPixels(rectangleCenter, SQUARE_HEIGHT);

                        if (RectangleAgent.IsObstacle_onPixels(levelArray, rectanglePixels))
                        {

                            // RECTANGLE WITH HEIGHT 50
                            rectangleCenter = ConvertArrayPointIntoPoint(new ArrayPoint(x, y));
                            rectangleCenter.y -= MIN_RECTANGLE_HEIGHT / 2;
                            rectanglePixels = RectangleAgent.GetFormPixels(rectangleCenter, MIN_RECTANGLE_HEIGHT);

                            if (RectangleAgent.IsObstacle_onPixels(levelArray, rectanglePixels))
                            {

                                // RECTANGLE WITH HEIGHT 200
                                rectangleCenter = ConvertArrayPointIntoPoint(new ArrayPoint(x, y));
                                rectangleCenter.y -= MAX_RECTANGLE_HEIGHT / 2;
                                rectanglePixels = RectangleAgent.GetFormPixels(rectangleCenter, MAX_RECTANGLE_HEIGHT);

                                if (RectangleAgent.IsObstacle_onPixels(levelArray,rectanglePixels))
                                {
                                    return;
                                }
                            }
                        }

                        platformArray[y, x] = (levelArray[y, x] == BLACK) ? platformType.BLACK : platformType.YELLOW;
                    }

                });
            }

            return platformArray;

        }

        public static List<Platform> SetPlatforms_Circle(int[,] levelArray)
        {
            List<Platform> platforms = new List<Platform>();

            platformType[,] platformArray = GetPlatformArray_Circle(levelArray);

            Parallel.For(0, levelArray.GetLength(0), i =>
            {
                platformType currentPlatform = platformType.NO_PLATFORM;
                int leftEdge = 0;

                for (int j = 0; j < platformArray.GetLength(1); j++)
                {
                    if (currentPlatform == platformType.NO_PLATFORM)
                    {
                        if (platformArray[i, j] == platformType.BLACK || platformArray[i, j] == platformType.GREEN)
                        {
                            leftEdge = ConvertValue_ArrayPointIntoPoint(j);
                            currentPlatform = platformArray[i, j];
                        }
                    }
                    else
                    {
                        if (platformArray[i, j] != currentPlatform)
                        {
                            int rightEdge = ConvertValue_ArrayPointIntoPoint(j - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platforms)
                                {
                                    platforms.Add(new Platform(platformType.BLACK, ConvertValue_ArrayPointIntoPoint(i), leftEdge, rightEdge, new List<Move>(), MAX_CIRCLE_HEIGHT));
                                }
                            }

                            currentPlatform = platformArray[i, j];
                        }
                    }
                }
            });

            return SetPlatformsID(platforms);

        }

        public static List<Platform> SetPlatforms_Rectangle(int[,] levelArray)
        {

            List<Platform> platforms = new List<Platform>();
            platformType[,] platformArray = GetPlatformArray_Rectangle(levelArray);

            Parallel.For(8, 96, y =>
            {

                int leftEdge = 0, gap_size = 0;
                int allowed_height = MAX_RECTANGLE_HEIGHT;

                platformType currentPlatform = platformArray[y, 4];

                for (int x = 4; x < 155; x++)
                {

                    // CALCULATION OF NEW ALLOWED HEIGHT
                    int new_allowed_height = allowed_height;
                    if (platformArray[y, x] != platformType.NO_PLATFORM)
                    {
                        new_allowed_height = MIN_RECTANGLE_HEIGHT;
                        foreach (int h in RECTANGLE_HEIGHTS)
                        {
                            if (y - (h / PIXEL_LENGTH) < 0 || levelArray[y - (h / PIXEL_LENGTH), x] == BLACK || levelArray[y - (h / PIXEL_LENGTH), x] == YELLOW)
                                break;
                            new_allowed_height = h;
                        }
                    }

                    // ADD NEW PLATFORM BECAUSE OF NEW TYPE OR NEW ALLOWED HEIGHT
                    if (platformArray[y,x] != currentPlatform || new_allowed_height != allowed_height)
                    {
                        int rightEdge = ConvertValue_ArrayPointIntoPoint(x - 1);          
                        if (rightEdge > leftEdge)
                        {
                            // GAP
                            if (7 <= gap_size && gap_size <= 19)
                                lock (platforms)
                                    platforms.Add(new Platform(platformType.GAP, ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), MIN_RECTANGLE_HEIGHT));
                            // PLATFORM
                            else if (currentPlatform != platformType.NO_PLATFORM)
                                lock (platforms)
                                    platforms.Add(new Platform(currentPlatform, ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Move>(), allowed_height));
                        }
                        leftEdge = ConvertValue_ArrayPointIntoPoint(x - 1);
                        //leftEdge = (new_allowed_height != allowed_height) ? ConvertValue_ArrayPointIntoPoint(x - 1) : ConvertValue_ArrayPointIntoPoint(x);
                    }

                    // UPDATE OF VARIABLES
                    allowed_height = new_allowed_height;
                    currentPlatform = platformArray[y, x];
                    gap_size = (levelArray[y, x] == OPEN || levelArray[y, x] == GREEN) ? gap_size + 1 : 0;

                }
            });

            return SetPlatformsID(platforms);
        }

        public static List<Platform> JoinPlatforms(List<Platform> platforms1, List<Platform> platforms2)
        {

            foreach (Platform p in platforms2)
            {
                platforms1.Add(new Platform(platformType.RECTANGLE, p.height - GameInfo.MIN_RECTANGLE_HEIGHT, p.leftEdge, p.rightEdge, p.moves, p.allowedHeight, p.id));
            }

            return SetPlatformsID(platforms1);

        }

        public static List<Platform> SetPlatformsID(List<Platform> platforms)
        {
            platforms.Sort((a, b) => {
                int result = a.height - b.height;
                return result != 0 ? result : a.leftEdge - b.leftEdge;
            });

            Parallel.For(0, platforms.Count, i =>
            {
                Platform tempPlatfom = platforms[i];
                tempPlatfom.id = i + 1;
                platforms[i] = tempPlatfom;
            });

            return platforms;
        }

        public static List<Platform> DeleteCooperationPlatforms(List<Platform> platforms)
        {

            foreach (Platform p in platforms)
            {
                foreach (Move m in p.moves)
                {
                    if (m.reachablePlatform.type == platformType.RECTANGLE)
                    {
                        var itemToRemove = p.moves.Single(r => r.type == m.type &&
                                                                r.reachablePlatform.id == m.reachablePlatform.id &&
                                                                r.precondition.position.x == m.precondition.position.x &&
                                                                r.precondition.position.y == m.precondition.position.y);
                        p.moves.Remove(itemToRemove);
                    }
                }
            }

            foreach (Platform p in platforms)
            {
                if (p.type == platformType.RECTANGLE)
                {
                    var itemToRemove = platforms.Single(r => r.id == p.id && r.type == p.type);
                    platforms.Remove(itemToRemove);
                }
            }

            return platforms;

        }

        public static List<Platform> DeleteUnreachablePlatforms(List<Platform> rectanglePlatforms, RectangleRepresentation initialRectangle)
        {
            Platform? currentPlatform = GetPlatform(rectanglePlatforms, new Point((int)initialRectangle.X, (int)initialRectangle.Y), initialRectangle.Height);

            if (currentPlatform.HasValue)
            {
                bool[] platformsChecked = VisitPlatform(new bool[rectanglePlatforms.Count], currentPlatform.Value);

                for (int p = platformsChecked.Length - 1; p >= 0; p--)
                {
                    if (!platformsChecked[p])
                    {
                        rectanglePlatforms.Remove(rectanglePlatforms[p]);
                    }
                }
            }

            foreach (Platform p in rectanglePlatforms)
            {
                p.moves.Clear();
            }

            return rectanglePlatforms;
        }

        private static bool[] VisitPlatform(bool[] platformsChecked, Platform platform)
        {
            platformsChecked[platform.id - 1] = true;

            foreach (Move m in platform.moves)
            {
                if (!platformsChecked[m.reachablePlatform.id - 1])
                {
                    platformsChecked = VisitPlatform(platformsChecked, m.reachablePlatform);
                }
            }

            return platformsChecked;
        }

        private static Platform? GetPlatform(List<Platform> platformsList, Point center, float height, int velocityY = 0)
        {

            foreach (Platform i in platformsList)
            {
                if (i.leftEdge <= center.x && center.x <= i.rightEdge && (i.height - center.y >= (height / 2) - 8) && (i.height - center.y <= (height / 2) + 8))
                {
                    return i;
                }
            }

            return null;
        }
    }
}
