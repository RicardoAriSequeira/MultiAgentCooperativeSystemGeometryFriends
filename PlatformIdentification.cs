using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    static class PlatformIdentification
    {

        public static Graph.platformType[,] GetPlatformArray_Circle(int[,] levelArray)
        {
            Graph.platformType[,] platformArray = new Graph.platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    LevelRepresentation.Point circleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                    circleCenter.y -= GameInfo.CIRCLE_RADIUS;
                    List<LevelRepresentation.ArrayPoint> circlePixels = Graph.GetCirclePixels(circleCenter, GameInfo.CIRCLE_RADIUS);

                    if (!CircleAgent.IsObstacle_onPixels(levelArray, circlePixels))
                    {
                        if (levelArray[y, x - 1] == LevelRepresentation.BLACK || levelArray[y, x] == LevelRepresentation.BLACK)
                        {
                            platformArray[y, x] = Graph.platformType.BLACK;
                        }
                        else if (levelArray[y, x - 1] == LevelRepresentation.GREEN || levelArray[y, x] == LevelRepresentation.GREEN)
                        {
                            platformArray[y, x] = Graph.platformType.GREEN;
                        }
                    }
                });
            }

            return platformArray;

        }

        public static Graph.platformType[,] GetPlatformArray_Rectangle(int[,] levelArray)
        {
            Graph.platformType[,] platformArray = new Graph.platformType[levelArray.GetLength(0), levelArray.GetLength(1)];

            for (int y = 0; y < levelArray.GetLength(0); y++)
            {
                Parallel.For(0, levelArray.GetLength(1), x =>
                {

                    if (levelArray[y, x] == LevelRepresentation.BLACK || levelArray[y, x] == LevelRepresentation.YELLOW)
                    {

                        // RECTANGLE WITH HEIGHT 100
                        LevelRepresentation.Point rectangleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                        rectangleCenter.y -= GameInfo.SQUARE_HEIGHT / 2;
                        List<LevelRepresentation.ArrayPoint> rectanglePixels = RectangleAgent.GetFormPixels(rectangleCenter, GameInfo.SQUARE_HEIGHT);

                        if (RectangleAgent.IsObstacle_onPixels(levelArray, rectanglePixels))
                        {

                            // RECTANGLE WITH HEIGHT 50
                            rectangleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                            rectangleCenter.y -= GameInfo.MIN_RECTANGLE_HEIGHT / 2;
                            rectanglePixels = RectangleAgent.GetFormPixels(rectangleCenter, GameInfo.MIN_RECTANGLE_HEIGHT);

                            if (RectangleAgent.IsObstacle_onPixels(levelArray, rectanglePixels))
                            {

                                // RECTANGLE WITH HEIGHT 200
                                rectangleCenter = LevelRepresentation.ConvertArrayPointIntoPoint(new LevelRepresentation.ArrayPoint(x, y));
                                rectangleCenter.y -= GameInfo.MAX_RECTANGLE_HEIGHT / 2;
                                rectanglePixels = RectangleAgent.GetFormPixels(rectangleCenter, GameInfo.MAX_RECTANGLE_HEIGHT);

                                if (RectangleAgent.IsObstacle_onPixels(levelArray,rectanglePixels))
                                {
                                    return;
                                }
                            }
                        }

                        platformArray[y, x] = (levelArray[y, x] == LevelRepresentation.BLACK) ? Graph.platformType.BLACK : Graph.platformType.YELLOW;
                    }

                });
            }

            return platformArray;

        }

        public static List<Graph.Platform> SetPlatforms_Circle(int[,] levelArray)
        {
            List<Graph.Platform> platforms = new List<Graph.Platform>();

            Graph.platformType[,] platformArray = GetPlatformArray_Circle(levelArray);

            Parallel.For(0, levelArray.GetLength(0), i =>
            {
                Graph.platformType currentPlatform = Graph.platformType.NO_PLATFORM;
                int leftEdge = 0;

                for (int j = 0; j < platformArray.GetLength(1); j++)
                {
                    if (currentPlatform == Graph.platformType.NO_PLATFORM)
                    {
                        if (platformArray[i, j] == Graph.platformType.BLACK || platformArray[i, j] == Graph.platformType.GREEN)
                        {
                            leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(j);
                            currentPlatform = platformArray[i, j];
                        }
                    }
                    else
                    {
                        if (platformArray[i, j] != currentPlatform)
                        {
                            int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(j - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platforms)
                                {
                                    platforms.Add(new Graph.Platform(Graph.platformType.BLACK, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(i), leftEdge, rightEdge, new List<Graph.Move>(), GameInfo.MAX_CIRCLE_HEIGHT / LevelRepresentation.PIXEL_LENGTH));
                                }
                            }

                            currentPlatform = platformArray[i, j];
                        }
                    }
                }
            });

            return platforms;

        }

        public static List<Graph.Platform> SetPlatforms_Rectangle(int[,] levelArray)
        {

            List<Graph.Platform> platforms = new List<Graph.Platform>();

            Graph.platformType[,] platformArray = GetPlatformArray_Rectangle(levelArray);

            Parallel.For(0, platformArray.GetLength(0), y =>
            {

                int min_height_pixels = GameInfo.MIN_RECTANGLE_HEIGHT / LevelRepresentation.PIXEL_LENGTH;
                int max_height_pixels = Math.Min((GameInfo.MAX_RECTANGLE_HEIGHT / LevelRepresentation.PIXEL_LENGTH), y + LevelRepresentation.MARGIN + min_height_pixels);

                int leftEdge = 0, allowedHeight = max_height_pixels, gap_size = 0;
                Graph.platformType currentPlatform = Graph.platformType.NO_PLATFORM;

                for (int x = 0; x < platformArray.GetLength(1); x++)
                {

                    if (currentPlatform == Graph.platformType.NO_PLATFORM)
                    {
                        if (platformArray[y, x] == Graph.platformType.BLACK || platformArray[y, x] == Graph.platformType.YELLOW)
                        {

                            if (7 <= gap_size && gap_size <= 19)
                            {
                                int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

                                if (rightEdge >= leftEdge)
                                {

                                    int gap_allowed_height = (GameInfo.RECTANGLE_AREA / (Math.Min((gap_size + 8) * LevelRepresentation.PIXEL_LENGTH, GameInfo.MAX_RECTANGLE_HEIGHT))) / LevelRepresentation.PIXEL_LENGTH;

                                    lock (platforms)
                                    {
                                        platforms.Add(new Graph.Platform(Graph.platformType.GAP, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Graph.Move>(), gap_allowed_height));
                                    }
                                }
                            }

                            gap_size = 0;
                            currentPlatform = platformArray[y, x];

                            for (int h = min_height_pixels; h <= max_height_pixels; h++)
                            {
                                if (levelArray[y - h, x] == LevelRepresentation.BLACK || levelArray[y - h, x] == LevelRepresentation.YELLOW)
                                {
                                    allowedHeight = h;
                                    break;
                                }
                            }

                            leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x);
                        }

                        else if (levelArray[y, x] == LevelRepresentation.GREEN || levelArray[y, x] == LevelRepresentation.OPEN)
                        {
                            gap_size++;
                        }
                    }

                    else
                    {
                        if (platformArray[y, x] != currentPlatform)
                        {
                            int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

                            if (rightEdge >= leftEdge)
                            {
                                lock (platforms)
                                {
                                    platforms.Add(new Graph.Platform(currentPlatform, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Graph.Move>(), allowedHeight));
                                }
                            }

                            allowedHeight = max_height_pixels;
                            currentPlatform = platformArray[y, x];
                            leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x);

                        }

                        if (platformArray[y, x] != Graph.platformType.NO_PLATFORM && y > LevelRepresentation.MARGIN + min_height_pixels)
                        {

                            for (int h = min_height_pixels; h <= max_height_pixels; h++)
                            {

                                if (levelArray[y - h, x] == LevelRepresentation.BLACK ||
                                    levelArray[y - h, x] == LevelRepresentation.YELLOW ||
                                    (h == max_height_pixels && allowedHeight != max_height_pixels && levelArray[y - h, x] == LevelRepresentation.OPEN))
                                {

                                    if (h != allowedHeight)
                                    {

                                        int rightEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

                                        if (rightEdge >= leftEdge)
                                        {
                                            lock (platforms)
                                            {
                                                platforms.Add(new Graph.Platform(currentPlatform, LevelRepresentation.ConvertValue_ArrayPointIntoPoint(y), leftEdge, rightEdge, new List<Graph.Move>(), allowedHeight));
                                            }
                                        }

                                        allowedHeight = h;
                                        leftEdge = LevelRepresentation.ConvertValue_ArrayPointIntoPoint(x - 1);

                                    }

                                    break;

                                }
                            }
                        }
                    }
                }
            });

            return platforms;
        }

        public static List<Graph.Platform> JoinPlatforms(List<Graph.Platform> platforms1, List<Graph.Platform> platforms2)
        {

            foreach (Graph.Platform p2 in platforms2)
            {

                bool repeatedPlatform = false;

                foreach (Graph.Platform p1 in platforms1)
                {
                    if (p2.type == p1.type &&
                        p2.height == p1.height &&
                        (p2.leftEdge >= p1.leftEdge - LevelRepresentation.PIXEL_LENGTH) &&
                        (p2.leftEdge <= p1.leftEdge + LevelRepresentation.PIXEL_LENGTH) &&
                        (p2.rightEdge >= p1.rightEdge - LevelRepresentation.PIXEL_LENGTH) &&
                        (p2.rightEdge <= p1.rightEdge + LevelRepresentation.PIXEL_LENGTH))
                    {
                        repeatedPlatform = true;
                        break;
                    }
                }

                if (!repeatedPlatform)
                {
                    platforms1.Add(p2);
                }
            }

            return platforms1;
        }

        public static List<Graph.Platform> SetPlatformsID(List<Graph.Platform> platforms)
        {
            platforms.Sort((a, b) => {
                int result = a.height - b.height;
                return result != 0 ? result : a.leftEdge - b.leftEdge;
            });

            Parallel.For(0, platforms.Count, i =>
            {
                Graph.Platform tempPlatfom = platforms[i];
                tempPlatfom.id = i + 1;
                platforms[i] = tempPlatfom;
            });

            return platforms;
        }

    }
}
