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
                Fall(graph, fromPlatform, SQUARE_HEIGHT);
                Collect(graph, fromPlatform);
            }
        }

        public static void Jump(Graph graph, Platform from)
        {
            Parallel.For(0, (MAX_VELOCITYX / VELOCITYX_STEP), k =>
            {
                int start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
                int end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
                
                if (from.type == platformType.RECTANGLE && k <= 5)
                {
                    start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) + 90;
                    end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) - 90;

                    Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        Point movePoint = new Point(start + j * PIXEL_LENGTH * 2, from.height - CIRCLE_RADIUS);
                        State state = new State(movePoint, CIRCLE_HEIGHT, VELOCITYX_STEP * k, true);
                        Trajectory(graph, from, state, movementType.JUMP);
                        state.right_direction = false;
                        Trajectory(graph, from, state, movementType.JUMP);

                        movePoint = new Point(start + j * PIXEL_LENGTH * 2, from.height - 50 - CIRCLE_RADIUS);
                        state = new State(movePoint, CIRCLE_HEIGHT, VELOCITYX_STEP * k, true);
                        Trajectory(graph, from, state, movementType.JUMP);
                        state.right_direction = false;
                        Trajectory(graph, from, state, movementType.JUMP);

                    });
                }

                else if (from.type != platformType.RECTANGLE)
                {
                    Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        Point movePoint = new Point(start + j * PIXEL_LENGTH * 2, from.height - CIRCLE_RADIUS);
                        State state = new State(movePoint, CIRCLE_HEIGHT, VELOCITYX_STEP * k, true);
                        Trajectory(graph, from, state, movementType.JUMP);
                        state.right_direction = false;
                        Trajectory(graph, from, state, movementType.JUMP);
                    });
                }

            });
        }

        public static void Fall(Graph graph, Platform from, int height)
        {
            Parallel.For(0, (MAX_VELOCITYX / VELOCITYX_STEP), k =>
            {
                Point start = new Point(from.rightEdge + PIXEL_LENGTH, from.height - (height / 2));
                State state = new State(start, height, VELOCITYX_STEP * k, true);
                Trajectory(graph, from, state, movementType.FALL);

                start = new Point(from.leftEdge - PIXEL_LENGTH, from.height - (height / 2));
                state = new State(start, height, VELOCITYX_STEP * k, false);
                Trajectory(graph, from, state, movementType.FALL);
            });
        }

        public static void Collect(Graph graph, Platform from)
        {
            int start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
            int end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);

            Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
            {
                Parallel.ForEach(graph.POSSIBLE_HEIGHTS, h =>
               {
                   Point movePoint = new Point(start + j * PIXEL_LENGTH * 2, from.height - (h / 2));
                   List<ArrayPoint> pixels = graph.GetFormPixels(movePoint, h);

                   if (!graph.ObstacleOnPixels(pixels))
                   {
                       bool[] collectible_onPath = graph.CollectiblesOnPixels(pixels);
                       State new_state = new State(movePoint, h, 0, true);
                       Move new_move = new Move(from, new_state, movePoint, movementType.COLLECT, collectible_onPath, 0, false);
                       graph.AddMove(from, new_move);
                   }
               });
            });

        }

        public static void StairOrGap(Graph graph, Platform from, Platform to, int height)
        {
            bool right_direction = false, obstacle = false; ;
            bool[] collectible_onPath = new bool[graph.nCollectibles];

            if (from.Equals(to) || !graph.IsStairOrGap(from, to, ref right_direction)) return;

            int start = right_direction ? from.rightEdge : to.rightEdge;
            int end = right_direction ? to.leftEdge : from.leftEdge;

            for (int k = start; k <= end; k += PIXEL_LENGTH)
            {
                List<ArrayPoint> pixels = graph.GetFormPixels(new Point(k, to.height - height), height);

                if (graph.ObstacleOnPixels(pixels))
                {
                    obstacle = true;
                    break;
                }

                collectible_onPath = Utilities.GetOrMatrix(collectible_onPath, graph.CollectiblesOnPixels(pixels));
            }

            if (!obstacle)
            {
                Point movePoint = right_direction ? new Point(from.rightEdge, from.height) : new Point(from.leftEdge, from.height);
                Point landPoint = right_direction ? new Point(to.leftEdge, to.height) : new Point(to.rightEdge, to.height);

                int length = (from.height - to.height) + Math.Abs(movePoint.x - landPoint.x);
                State new_state = new State(movePoint, height, 0, right_direction);
                Move new_move = new Move(to, new_state, landPoint, movementType.TRANSITION, collectible_onPath, length, false);
                graph.AddMove(from, new_move);
            }
        }

        public static void TransitionRectangle(Graph graph, Platform from)
        {
            foreach (Platform to in graph.platforms)
            {
                if (!from.Equals(to))
                {
                    BigGapOrMorph(graph, from, to);
                    BigStair(graph, from, to);
                    if (from.height != to.height)
                        StairOrGap(graph, from, to, SQUARE_HEIGHT);
                }
            }
        }

        public static void BigGapOrMorph(Graph graph, Platform from, Platform to)
        {

            if (from.height != to.height || (from.rightEdge != to.leftEdge && from.leftEdge != to.rightEdge)) return;

            // CALCULATION OF START AND END POSITIONS
            bool right_direction = (from.rightEdge == to.leftEdge);
            int required_height = Math.Min(Math.Min(from.allowedHeight, to.allowedHeight), SQUARE_HEIGHT);
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
                start = new Point(to.leftEdge + (gap_size / 2), to.height - (fall_height / 2));
                State state = new State(start, fall_height, 0, right_direction);
                Trajectory(graph, from, state, movementType.GAP);
            }

        }

        public static void BigStair(Graph graph, Platform fromPlatform, Platform toPlatform)
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
                    Move new_move = new Move(toPlatform, new_state, end, movementType.TRANSITION, new bool[graph.nCollectibles], pathLength, false);
                    graph.AddMove(fromPlatform, new_move);
                }

                else if (toPlatform.rightEdge <= fromPlatform.leftEdge && fromPlatform.leftEdge <= toPlatform.rightEdge + 33)
                {
                    Point start = new Point(fromPlatform.leftEdge, fromPlatform.height);
                    Point end = new Point(toPlatform.rightEdge, toPlatform.height);

                    int pathLength = (fromPlatform.height - toPlatform.height) + Math.Abs(start.x - end.x);
                    int requiredHeight = 3 * stairHeight + PIXEL_LENGTH;

                    State new_state = new State(start, requiredHeight, 150, false);
                    Move new_move = new Move(toPlatform, new_state, end, movementType.TRANSITION, new bool[graph.nCollectibles], pathLength, false);
                    graph.AddMove(fromPlatform, new_move);
                }
            }
        }

        public static void Trajectory(Graph graph, Platform from, State state, movementType movementType)
        {

            if (!graph.EnoughLength(from, state)) return;

            float length = 0;
            bool ceiling = false;
            collideType collision_type = collideType.OTHER;
            bool[] collectibles = new bool[graph.nCollectibles];
            int fake_radius = Math.Min(graph.AREA / state.height, state.height) / 2;
            float velocity_y = (movementType == movementType.JUMP) ? JUMP_VELOCITYY : FALL_VELOCITYY;
            float velocity_x = state.right_direction ? state.horizontal_velocity : -state.horizontal_velocity;

            Point start = new Point(state.position.x, state.position.y);
            Point collision = start, prev_collision = collision;

            do
            {
                Point center = collision;

                for (float time = TIME_STEP; true; time += TIME_STEP)
                {
                    Point previousCenter = center;
                    center = GetCurrentCenter(collision, velocity_x, velocity_y, time);
                    List<ArrayPoint> pixels = GetCirclePixels(center, fake_radius);

                    if (Collision(graph, pixels, center, velocity_y - GRAVITY * (time - TIME_STEP), fake_radius, ref collision_type))
                    {
                        prev_collision = collision;
                        collision = previousCenter;
                        velocity_x = (collision_type == collideType.CEILING) ? velocity_x / 3 : 0;
                        velocity_y = (collision_type == collideType.CEILING) ? - (velocity_y - GRAVITY * (time - TIME_STEP)) / 3 : 0;
                        break;
                    }

                    collectibles = Utilities.GetOrMatrix(collectibles, CollectiblesOnPixels(graph.levelArray, pixels, graph.nCollectibles));
                    length += (float)Math.Sqrt(Math.Pow(center.x - previousCenter.x, 2) + Math.Pow(center.y - previousCenter.y, 2));

                }

                ceiling = (collision_type == collideType.CEILING) ? true : ceiling;

                if (collision_type == collideType.FLOOR || collision_type == collideType.RECTANGLE)
                {

                    if (fake_radius != CIRCLE_RADIUS && fake_radius != SQUARE_HEIGHT / 2)
                        collision.y -= Math.Max((state.height / 2) - fake_radius - PIXEL_LENGTH, 0);

                    Platform? to = graph.GetPlatform(collision, state.height);

                    if (to.HasValue)
                    {
                        if (movementType == movementType.FALL)
                            start.x = state.right_direction ? start.x - PIXEL_LENGTH : start.x + PIXEL_LENGTH;

                        State? partner_state = null;
                        bool possible_rectangle = collision_type == collideType.RECTANGLE && (Math.Abs(collision.x - start.x) >= 140 || from.id == to.Value.id);

                        if (collision_type == collideType.FLOOR || possible_rectangle)
                        {
                            if (possible_rectangle)
                            {
                                int rectangle_x = Math.Min(Math.Max(collision.x, 136), 1064);
                                Point rectangle_position = new Point(rectangle_x, collision.y + CIRCLE_RADIUS + (MIN_RECTANGLE_HEIGHT / 2));
                                partner_state = new State(rectangle_position, MIN_RECTANGLE_HEIGHT, 0, true);
                            }

                            State new_state = new State(start, state.height, state.horizontal_velocity, state.right_direction);
                            Move new_move = new Move(to.Value, new_state, collision, movementType, collectibles, (int)length, ceiling, partner_state);
                            graph.AddMove(from, new_move);
                        }
                    }
                }

            } while (collision_type != collideType.FLOOR && !prev_collision.Equals(collision));

        }

        public static bool Collision(Graph graph, List<ArrayPoint> pixels, Point center, float velocityY, int radius, ref collideType collide_type)
        {
            ArrayPoint centerArray = ConvertPointIntoArrayPoint(center, false, false);
            int highestY = PointToArrayPoint(center.y - radius, false);
            int lowestY = PointToArrayPoint(center.y + radius, true);

            if (graph.ObstacleOnPixels(pixels) || (velocityY < 0 && collide_type != collideType.RECTANGLE && graph.levelArray[lowestY, centerArray.xArray] == RECTANGLE))
            {
                if (velocityY < 0 && (graph.levelArray[lowestY, centerArray.xArray] == BLACK || graph.levelArray[lowestY, centerArray.xArray] == graph.OBSTACLE_COLOUR))
                {
                    collide_type = collideType.FLOOR;
                }

                else if (velocityY < 0 && graph.levelArray[lowestY, centerArray.xArray] == RECTANGLE)
                {
                    collide_type = collideType.RECTANGLE;
                }

                else if (graph.levelArray[highestY, centerArray.xArray] == BLACK || graph.levelArray[highestY, centerArray.xArray] == graph.OBSTACLE_COLOUR)
                {
                    collide_type = collideType.CEILING;
                }

                else collide_type = collideType.OTHER;

                return true;
            }

            return false;
        }

    }
}
