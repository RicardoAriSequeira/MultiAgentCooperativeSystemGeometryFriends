using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    static class MoveIdentification
    {

        public static void Collect(Graph graph, Graph.Platform fromPlatform)
        {
            int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);
            int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (LevelRepresentation.PIXEL_LENGTH * 2);

            Parallel.For(0, (to - from) / (LevelRepresentation.PIXEL_LENGTH * 2) + 1, j =>
            {
                for (int height = graph.MIN_HEIGHT; height <= graph.MAX_HEIGHT; height += 8)
                {
                    LevelRepresentation.Point movePoint = new LevelRepresentation.Point(from + j * LevelRepresentation.PIXEL_LENGTH * 2, fromPlatform.height - (height / 2));
                    List<LevelRepresentation.ArrayPoint> pixels = graph.GetFormPixels(movePoint, height);

                    if (!graph.IsObstacle_onPixels(pixels))
                    {
                        bool[] collectible_onPath = graph.GetCollectibles_onPixels(pixels);
                        graph.AddMove(fromPlatform, new Graph.Move(fromPlatform, movePoint, movePoint, 0, true, Graph.movementType.COLLECT, collectible_onPath, 0, false, height));
                    }
                }
            });

        }

        public static bool Fall(Graph graph, Graph.Platform fromPlatform, LevelRepresentation.Point movePoint, int height, int velocityX, bool rightMove, Graph.movementType movementType)
        {

            if (!graph.IsEnoughLengthToAccelerate(fromPlatform, movePoint, velocityX, rightMove))
            {
                return false;
            }

            bool[] collectible_onPath = new bool[graph.nCollectibles];
            float pathLength = 0;

            LevelRepresentation.Point collidePoint = movePoint;
            LevelRepresentation.Point prevCollidePoint;

            Graph.collideType collideType = Graph.collideType.OTHER;
            float collideVelocityX = rightMove ? velocityX : -velocityX;
            float collideVelocityY = (movementType == Graph.movementType.JUMP) ? GameInfo.JUMP_VELOCITYY : GameInfo.FALL_VELOCITYY;
            bool collideCeiling = false;

            do
            {
                prevCollidePoint = collidePoint;

                graph.GetPathInfo(collidePoint, collideVelocityX, collideVelocityY, ref collidePoint, ref collideType, ref collideVelocityX, ref collideVelocityY, ref collectible_onPath, ref pathLength, (Math.Min(graph.AREA / height, height) / 2));

                if (collideType == Graph.collideType.CEILING)
                {
                    collideCeiling = true;
                }

                if (prevCollidePoint.Equals(collidePoint))
                {
                    break;
                }
            }
            while (!(collideType == Graph.collideType.FLOOR));

            if (collideType == Graph.collideType.FLOOR)
            {

                Graph.Platform? toPlatform = graph.GetPlatform(collidePoint, height);

                if (toPlatform.HasValue)
                {
                    if (movementType == Graph.movementType.FALL)
                    {
                        movePoint.x = rightMove ? movePoint.x - LevelRepresentation.PIXEL_LENGTH : movePoint.x + LevelRepresentation.PIXEL_LENGTH;
                    }

                    graph.AddMove(fromPlatform, new Graph.Move(toPlatform.Value, movePoint, collidePoint, velocityX, rightMove, movementType, collectible_onPath, (int)pathLength, collideCeiling, height));

                    return true;
                }
            }

            return false;
        }
    }
}
