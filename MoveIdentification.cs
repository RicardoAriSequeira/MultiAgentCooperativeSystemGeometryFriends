using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GeometryFriendsAgents.Graph;
using static GeometryFriendsAgents.GameInfo;
using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    static class MoveIdentification
    {

        public static void Setup_Rectangle(Graph graph)
        {
            foreach (Platform fromPlatform in graph.platforms)
            {
                TransitionRectangle(graph, fromPlatform);
                if (fromPlatform.type == platformType.GAP) continue;
                Fall(graph, fromPlatform);
                Collect(graph, fromPlatform);
            }
        }

        public static void Jump(Graph graph, Platform fromPlatform)
        {
            Parallel.For(0, (GameInfo.MAX_VELOCITYX / VELOCITYX_STEP), k =>
            {
                int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
                int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
                
                if (fromPlatform.type == platformType.RECTANGLE && k <= 5)
                {
                    from = fromPlatform.leftEdge + (fromPlatform.leftEdge - GameInfo.LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) + 90;
                    to = fromPlatform.rightEdge - (fromPlatform.rightEdge - GameInfo.LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) - 90;

                    Parallel.For(0, (to - from) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        Point movePoint = new Point(from + j * PIXEL_LENGTH * 2, fromPlatform.height - GameInfo.CIRCLE_RADIUS);
                        Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, VELOCITYX_STEP * k, true, movementType.JUMP);
                        Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, VELOCITYX_STEP * k, false, movementType.JUMP);

                        movePoint = new Point(from + j * PIXEL_LENGTH * 2, fromPlatform.height - 50 - GameInfo.CIRCLE_RADIUS);
                        Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, VELOCITYX_STEP * k, true, movementType.JUMP);
                        Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, VELOCITYX_STEP * k, false, movementType.JUMP);

                        //movePoint = new LevelRepresentation.Point(from + j * LevelRepresentation.PIXEL_LENGTH * 2, fromPlatform.height - 150 - GameInfo.CIRCLE_RADIUS);
                        //Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, Graph.VELOCITYX_STEP * k, true, Graph.movementType.JUMP);
                        //Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, Graph.VELOCITYX_STEP * k, false, Graph.movementType.JUMP);
                    });
                }

                else if (fromPlatform.type != platformType.RECTANGLE)
                {
                    Parallel.For(0, (to - from) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        Point movePoint = new Point(from + j * PIXEL_LENGTH * 2, fromPlatform.height - GameInfo.CIRCLE_RADIUS);
                        Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, VELOCITYX_STEP * k, true, movementType.JUMP);
                        Trajectory(graph, fromPlatform, movePoint, graph.MAX_HEIGHT, VELOCITYX_STEP * k, false, movementType.JUMP);
                    });
                }

            });
        }

        public static void Fall(Graph graph, Platform fromPlatform)
        {
            int height = Math.Min(graph.MAX_HEIGHT, SQUARE_HEIGHT);

            Parallel.For(0, (MAX_VELOCITYX / VELOCITYX_STEP), k =>
            {
                Point movePoint = new Point(fromPlatform.rightEdge + PIXEL_LENGTH, fromPlatform.height - (height / 2));
                Trajectory(graph, fromPlatform, movePoint, height, VELOCITYX_STEP * k, true, movementType.FALL);

                movePoint = new Point(fromPlatform.leftEdge - PIXEL_LENGTH, fromPlatform.height - (height / 2));
                Trajectory(graph, fromPlatform, movePoint, height, VELOCITYX_STEP * k, false, movementType.FALL);
            });
        }

        public static void Collect(Graph graph, Platform fromPlatform)
        {
            int from = fromPlatform.leftEdge + (fromPlatform.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
            int to = fromPlatform.rightEdge - (fromPlatform.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);

            Parallel.For(0, (to - from) / (PIXEL_LENGTH * 2) + 1, j =>
            {
                for (int height = graph.MIN_HEIGHT; height <= graph.MAX_HEIGHT; height += 8)
                {
                    Point movePoint = new Point(from + j * PIXEL_LENGTH * 2, fromPlatform.height - (height / 2));
                    List<ArrayPoint> pixels = graph.GetFormPixels(movePoint, height);

                    if (!graph.IsObstacle_onPixels(pixels))
                    {
                        bool[] collectible_onPath = graph.GetCollectibles_onPixels(pixels);
                        State new_state = new State(movePoint, height, 0, true);
                        Move new_move = new Move(fromPlatform, new_state, movePoint, movementType.COLLECT, collectible_onPath, 0, false);
                        graph.AddMove(fromPlatform, new_move);
                    }
                }
            });

        }

        public static void StairOrGap_Circle(Graph graph, Platform fromPlatform)
        {

            foreach (Platform toPlatform in graph.platforms)
            {

                bool rightMove = false;
                bool obstacleFlag = false;
                bool[] collectible_onPath = new bool[graph.nCollectibles];

                if (fromPlatform.Equals(toPlatform) || !graph.IsStairOrGap(fromPlatform, toPlatform, ref rightMove))
                {
                    continue;
                }

                int from = rightMove ? fromPlatform.rightEdge : toPlatform.rightEdge;
                int to = rightMove ? toPlatform.leftEdge : fromPlatform.leftEdge;

                for (int k = from; k <= to; k += PIXEL_LENGTH)
                {
                    List<ArrayPoint> circlePixels = GetCirclePixels(new Point(k, toPlatform.height - GameInfo.CIRCLE_RADIUS), GameInfo.CIRCLE_RADIUS);

                    if (graph.IsObstacle_onPixels(circlePixels))
                    {
                        obstacleFlag = true;
                        break;
                    }

                    collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, graph.GetCollectibles_onPixels(circlePixels));
                }

                if (!obstacleFlag)
                {
                    Point movePoint = rightMove ? new Point(fromPlatform.rightEdge, fromPlatform.height) : new Point(fromPlatform.leftEdge, fromPlatform.height);
                    Point landPoint = rightMove ? new Point(toPlatform.leftEdge, toPlatform.height) : new Point(toPlatform.rightEdge, toPlatform.height);

                    State new_state = new State(movePoint, GameInfo.SQUARE_HEIGHT, 0, rightMove);
                    Move new_move = new Move(toPlatform, new_state, landPoint, movementType.TRANSITION, collectible_onPath, (fromPlatform.height - toPlatform.height) + Math.Abs(movePoint.x - landPoint.x), false);
                    graph.AddMove(fromPlatform, new_move);
                }
            }
        }

        public static void TransitionRectangle(Graph graph, Platform from)
        {
            foreach (Platform to in graph.platforms)
            {
                if (!from.Equals(to))
                {
                    GapOrMorph(graph, from, to);
                    Stair_Rectangle(graph, from, to);
                    SmallGap_Rectangle(graph, from, to);
                }
            }
        }

        public static void SmallGap_Rectangle(Graph graph, Platform from, Platform to)
        {

            bool right_direction = false;
            bool obstacleFlag = false;
            bool[] collectible_onPath = new bool[graph.nCollectibles];

            if (from.Equals(to) || !graph.IsStairOrGap(from, to, ref right_direction) || from.height == to.height)  return;

            int start = right_direction ? from.rightEdge : to.rightEdge;
            int end = right_direction ? to.leftEdge : from.leftEdge;

            for (int k = start; k <= end; k += PIXEL_LENGTH)
            {
                List<ArrayPoint> rectanglePixels = graph.GetFormPixels(new Point(k, to.height - SQUARE_HEIGHT), SQUARE_HEIGHT);

                if (graph.IsObstacle_onPixels(rectanglePixels))
                {
                    obstacleFlag = true;
                    break;
                }

                collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, graph.GetCollectibles_onPixels(rectanglePixels));
            }

            if (!obstacleFlag)
            {
                Point movePoint = new Point(right_direction ? from.rightEdge : from.leftEdge, from.height);
                Point landPoint = new Point(right_direction ? to.leftEdge : to.rightEdge, to.height);

                State new_state = new State(movePoint, SQUARE_HEIGHT, 0, right_direction);
                Move new_move = new Move(to, new_state, landPoint, movementType.TRANSITION, collectible_onPath, (from.height - to.height) + Math.Abs(movePoint.x - landPoint.x), false);
                graph.AddMove(from, new_move);
            }
        }

        public static void GapOrMorph(Graph graph, Platform from, Platform to)
        {

            if (from.height != to.height || (from.rightEdge != to.leftEdge && from.leftEdge != to.rightEdge)) return;

            // CALCULATION OF START AND END POSITIONS
            bool right_direction = (from.rightEdge == to.leftEdge);
            int required_height = Math.Min(from.allowedHeight, to.allowedHeight);
            int half_width = (RECTANGLE_AREA / required_height) / 2;
            int start_x = right_direction ? to.rightEdge - half_width : from.leftEdge + half_width;
            int end_x = right_direction ? to.leftEdge + half_width : to.rightEdge - half_width;
            Point start = new Point(start_x, from.height - (required_height / 2));
            Point end = new Point(end_x, to.height - (required_height / 2));

            // TRANSITION PLATFORMS
            State new_state = new State(start, required_height, 0, right_direction);
            Move new_move = new Move(to, new_state, end, movementType.TRANSITION, new bool[graph.nCollectibles], Math.Abs(end.x - start.x), false);
            graph.AddMove(from, new_move);

            // FALL THROUGH GAP
            if (to.type == platformType.GAP)
            {
                int gap_size = (to.rightEdge - to.leftEdge);
                int fall_height = RECTANGLE_AREA / Math.Min(gap_size - (2 * PIXEL_LENGTH), MIN_RECTANGLE_HEIGHT);
                Point movePoint = new Point(to.leftEdge + (gap_size / 2), to.height - (fall_height / 2));
                Trajectory(graph, from, movePoint, fall_height, 0, right_direction, movementType.GAP);
            }

        }

        public static void Stair_Rectangle(Graph graph, Platform fromPlatform, Platform toPlatform)
        {

            int stairHeight = fromPlatform.height - toPlatform.height;

            if (1 <= stairHeight && stairHeight <= 90)
            {

                if (toPlatform.leftEdge - 33 <= fromPlatform.rightEdge && fromPlatform.rightEdge <= toPlatform.leftEdge)
                {
                    Point start = new Point(fromPlatform.rightEdge, fromPlatform.height);
                    Point end = new Point(toPlatform.leftEdge, toPlatform.height);

                    int pathLength = (fromPlatform.height - toPlatform.height) + Math.Abs(start.x - end.x);
                    int requiredHeight = 3 * stairHeight + PIXEL_LENGTH;

                    State new_state = new State(start, requiredHeight, 150, true);
                    Move new_move = new Move(toPlatform, new_state, end, movementType.MORPH_UP, new bool[graph.nCollectibles], pathLength, false);
                    graph.AddMove(fromPlatform, new_move);
                }

                else if (toPlatform.rightEdge <= fromPlatform.leftEdge && fromPlatform.leftEdge <= toPlatform.rightEdge + 33)
                {
                    Point start = new Point(fromPlatform.leftEdge, fromPlatform.height);
                    Point end = new Point(toPlatform.rightEdge, toPlatform.height);

                    int pathLength = (fromPlatform.height - toPlatform.height) + Math.Abs(start.x - end.x);
                    int requiredHeight = 3 * stairHeight + PIXEL_LENGTH;

                    State new_state = new State(start, requiredHeight, 150, false);
                    Move new_move = new Move(toPlatform, new_state, end, movementType.MORPH_UP, new bool[graph.nCollectibles], pathLength, false);
                    graph.AddMove(fromPlatform, new_move);
                }
            }
        }

        public static void Trajectory(Graph graph, Platform fromPlatform, Point movePoint, int height, int velocityX, bool right_direction, movementType movementType)
        {

            if (!graph.IsEnoughLengthToAccelerate(fromPlatform, movePoint, velocityX, right_direction))
                return;

            float pathLength = 0;
            Point prevCollidePoint;
            bool collideCeiling = false;
            Point collidePoint = movePoint;
            collideType collideType = collideType.OTHER;
            bool[] collectible_onPath = new bool[graph.nCollectibles];
            int fake_radius = Math.Min(graph.AREA / height, height) / 2;
            float collideVelocityX = right_direction ? velocityX : -velocityX;
            float collideVelocityY = (movementType == movementType.JUMP) ? JUMP_VELOCITYY : FALL_VELOCITYY;

            do
            {
                prevCollidePoint = collidePoint;

                graph.GetPathInfo(collidePoint, collideVelocityX, collideVelocityY, ref collidePoint, ref collideType, ref collideVelocityX, ref collideVelocityY, ref collectible_onPath, ref pathLength, fake_radius);

                collideCeiling = (collideType == collideType.CEILING) ? true: collideCeiling;

                if (collideType == collideType.COOPERATION)
                {
                    Platform? toPlatform = graph.GetPlatform(collidePoint, height);

                    if (toPlatform.HasValue)
                    {
                        if (movementType == movementType.FALL)
                            movePoint.x = right_direction ? movePoint.x - PIXEL_LENGTH : movePoint.x + PIXEL_LENGTH;

                        int distance_to_land = Math.Abs(collidePoint.x - movePoint.x);

                        if (distance_to_land >= 140 || fromPlatform.id == toPlatform.Value.id)
                        {
                            int rectangle_position_x = Math.Min(Math.Max(collidePoint.x, 136), 1064);
                            Point rectangle_position = new Point(rectangle_position_x, collidePoint.y + CIRCLE_RADIUS + (MIN_RECTANGLE_HEIGHT / 2));
                            State rectangle_state = new State(rectangle_position, MIN_RECTANGLE_HEIGHT, 0, true);

                            State new_state = new State(movePoint, height, velocityX, right_direction);
                            Move new_move = new Move(toPlatform.Value, new_state, collidePoint, movementType, collectible_onPath, (int)pathLength, collideCeiling, rectangle_state);
                            graph.AddMove(fromPlatform, new_move);
                        }

                    }

                }

                if (prevCollidePoint.Equals(collidePoint))
                    break;
            }
            while (collideType != collideType.FLOOR);

            if (collideType == collideType.FLOOR)
            {

                if (fake_radius != CIRCLE_RADIUS)
                    collidePoint.y -= Math.Max((height/2) - fake_radius - PIXEL_LENGTH, 0);

                Platform? toPlatform = graph.GetPlatform(collidePoint, height);

                if (toPlatform.HasValue)
                {

                    if (movementType == movementType.GAP)
                    {
                        State new_state = new State(movePoint, height, 0, right_direction);
                        Move new_move = new Move((Platform)toPlatform, new_state, collidePoint, movementType, collectible_onPath, (int)pathLength, collideCeiling);
                        graph.AddMove(fromPlatform, new_move);
                    }

                    else
                    {
                        if (movementType == movementType.FALL)
                            movePoint.x = right_direction ? movePoint.x - PIXEL_LENGTH : movePoint.x + PIXEL_LENGTH;

                        State new_state = new State(movePoint, height, velocityX, right_direction);
                        Move new_move = new Move(toPlatform.Value, new_state, collidePoint, movementType, collectible_onPath, (int)pathLength, collideCeiling);
                        graph.AddMove(fromPlatform, new_move);
                    }

                }
            }

        }

    }
}
