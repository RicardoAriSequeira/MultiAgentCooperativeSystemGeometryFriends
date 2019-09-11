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
            Parallel.For((-MAX_VELOCITYX / VELOCITYX_STEP) + 1, (MAX_VELOCITYX / VELOCITYX_STEP), k =>
            {

                if (from.type == platformType.RECTANGLE && Math.Abs(k) <= 5)
                {
                    int start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) + 90;
                    int end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) - 90;

                    Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        State state = new State(start + j * PIXEL_LENGTH * 2, from.height - CIRCLE_RADIUS, VELOCITYX_STEP * k, 0, CIRCLE_HEIGHT);
                        Trajectory(graph, from, state, movementType.JUMP);

                        if (CanRectangleMorphUp(graph.levelArray, new Point(start + j * PIXEL_LENGTH * 2, from.height)))
                        {
                            state = new State(start + j * PIXEL_LENGTH * 2, from.height - 50 - CIRCLE_RADIUS, VELOCITYX_STEP * k, 0, CIRCLE_HEIGHT);
                            Trajectory(graph, from, state, movementType.JUMP);
                        }

                    });
                }

                if (from.type != platformType.RECTANGLE)
                {
                    int start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
                    int end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);

                    Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        State state = new State(start + j * PIXEL_LENGTH * 2, from.height - CIRCLE_RADIUS, VELOCITYX_STEP * k, 0, CIRCLE_HEIGHT);
                        Trajectory(graph, from, state, movementType.JUMP);
                    });
                }

            });
        }

        public static void Fall(Graph graph, Platform from, int height)
        {

            Parallel.For(0, (MAX_VELOCITYX / VELOCITYX_STEP), k =>
            {

                if (from.type == platformType.RECTANGLE && k >= 2 && k <= 5)
                {
                    int start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) + 90;
                    int end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2) - 90;

                    Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        // RIGHT FALL FROM HORIZONTAL RECTANGLE
                        State st = new State(0, from.height - CIRCLE_RADIUS, VELOCITYX_STEP * k, 0, CIRCLE_HEIGHT);
                        st.x = (start + j * PIXEL_LENGTH * 2) + (MAX_RECTANGLE_HEIGHT/2) + PIXEL_LENGTH;
                        Trajectory(graph, from, st, movementType.FALL);

                        // LEFT FALL FROM HORIZONTAL RECTANGLE
                        st = new State(0, from.height - CIRCLE_RADIUS, - (VELOCITYX_STEP * k), 0, CIRCLE_HEIGHT);
                        st.x = (start + j * PIXEL_LENGTH * 2) - (MAX_RECTANGLE_HEIGHT / 2) - PIXEL_LENGTH;
                        Trajectory(graph, from, st, movementType.FALL);
                    });

                    start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
                    end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);

                    Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
                    {
                        // RIGHT FALL FROM SQUARE
                        State st = new State(0, from.height - 50 - CIRCLE_RADIUS, VELOCITYX_STEP * k, 0, CIRCLE_HEIGHT);
                        st.x = (start + j * PIXEL_LENGTH * 2) + (SQUARE_HEIGHT / 2) + PIXEL_LENGTH;
                        Trajectory(graph, from, st, movementType.FALL);

                        // LEFT FALL FROM SQUARE
                        st = new State(0, from.height - 50 - CIRCLE_RADIUS, -(VELOCITYX_STEP * k), 0, CIRCLE_HEIGHT);
                        st.x = (start + j * PIXEL_LENGTH * 2) - (SQUARE_HEIGHT / 2) - PIXEL_LENGTH;
                        Trajectory(graph, from, st, movementType.FALL);

                    });

                }
                
                if (from.type != platformType.RECTANGLE)
                {
                    // RIGHT FALL FROM NORMAL PLATFORM
                    State state = new State(from.rightEdge + PIXEL_LENGTH, from.height - (height / 2), VELOCITYX_STEP * k, 0, height);
                    Trajectory(graph, from, state, movementType.FALL);

                    // LEFT FALL FROM NORMAL PLATFORM
                    state = new State(from.leftEdge - PIXEL_LENGTH, from.height - (height / 2), -(VELOCITYX_STEP * k), 0, height);
                    Trajectory(graph, from, state, movementType.FALL);
                }

            });
        }

        public static void Collect(Graph graph, Platform from)
        {
            int start = from.leftEdge + (from.leftEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);
            int end = from.rightEdge - (from.rightEdge - LEVEL_ORIGINAL) % (PIXEL_LENGTH * 2);

            Parallel.ForEach(graph.possible_heights, h =>
            {
                Parallel.For(0, (end - start) / (PIXEL_LENGTH * 2) + 1, j =>
                {
                   Point movePoint = new Point(start + j * PIXEL_LENGTH * 2, from.height - (h / 2));
                   List<ArrayPoint> pixels = graph.GetFormPixels(movePoint, h);

                   if (!graph.ObstacleOnPixels(pixels))
                   {
                       bool[] collectible_onPath = graph.CollectiblesOnPixels(pixels);
                       State new_state = new State(movePoint.x, movePoint.y, 0, 0, h);
                       Move new_move = new Move(from, new_state, movePoint, movementType.COLLECT, collectible_onPath, 0, false);
                       graph.AddMove(from, new_move);
                   }
               });
            });

        }

        public static void Transition(Graph graph, Platform from, Platform to, int height)
        {
            bool right_direction = false;
            bool[] collectibles = new bool[graph.nCollectibles];

            if (from.Equals(to) || !graph.IsStairOrGap(from, to, ref right_direction)) return;

            int start = right_direction ? from.rightEdge : to.rightEdge;
            int end = right_direction ? to.leftEdge : from.leftEdge;

            for (int k = start; k <= end; k += PIXEL_LENGTH)
            {
                List<ArrayPoint> pixels = graph.GetFormPixels(new Point(k, to.height - height), height);
                if (graph.ObstacleOnPixels(pixels)) return;
                collectibles = Utilities.GetOrMatrix(collectibles, graph.CollectiblesOnPixels(pixels));
            }

            Point movePoint = new Point(right_direction ? from.rightEdge : from.leftEdge, from.height);
            Point landPoint = new Point(right_direction ? to.leftEdge : to.rightEdge, to.height);

            int length = (from.height - to.height) + Math.Abs(movePoint.x - landPoint.x);
            State state = new State(movePoint.x, movePoint.y, 0, 0, height);
            State partner_state = new State(movePoint.x, movePoint.y, 0, 0, MIN_RECTANGLE_HEIGHT);
            Move move = new Move(to, state, landPoint, movementType.TRANSITION, collectibles, length, false, partner_state);
            graph.AddMove(from, move);
        }

        public static void TransitionRectangle(Graph graph, Platform from)
        {
            foreach (Platform to in graph.platforms)
            {
                if (!from.Equals(to))
                {
                    BigStair(graph, from, to);
                    if (from.height != to.height)
                        Transition(graph, from, to, SQUARE_HEIGHT);
                    else
                        BigGapOrMorph(graph, from, to);
                }
            }
        }

        public static void BigGapOrMorph(Graph graph, Platform from, Platform to)
        {

            if (from.rightEdge != to.leftEdge && from.leftEdge != to.rightEdge) return;

            // CALCULATION OF START AND END POSITIONS
            bool right_direction = (from.rightEdge == to.leftEdge);
            int required_height = (Math.Min(from.allowedHeight, to.allowedHeight) < SQUARE_HEIGHT) ? MIN_RECTANGLE_HEIGHT : SQUARE_HEIGHT;
            int half_width = (RECTANGLE_AREA / required_height) / 2;
            int start_x = right_direction ? to.rightEdge - half_width : from.leftEdge + half_width;
            int end_x = right_direction ? to.leftEdge + half_width : to.rightEdge - half_width;
            Point start = new Point(start_x, from.height - (required_height / 2));
            Point end = new Point(end_x, to.height - (required_height / 2));

            // TRANSITION PLATFORMS
            State new_state = new State(start.x, start.y, right_direction ? 1 : -1, 0, required_height);
            Move new_move = new Move(to, new_state, end, movementType.TRANSITION, new bool[graph.nCollectibles], Math.Abs(end.x - start.x), false);
            graph.AddMove(from, new_move);

            // FALL THROUGH GAP
            if (to.type == platformType.GAP)
            {
                int gap_size = (to.rightEdge - to.leftEdge);
                int fall_height = RECTANGLE_AREA / Math.Min(gap_size - (2 * PIXEL_LENGTH), MIN_RECTANGLE_HEIGHT);
                start = new Point(to.leftEdge + (gap_size / 2), to.height - (fall_height / 2));
                State state = new State(start.x, start.y, right_direction ? 1 : -1, 0, fall_height);
                Trajectory(graph, from, state, movementType.FALL);
            }

        }

        public static void BigStair(Graph graph, Platform from, Platform to)
        {

            bool right_direction = false;
            int stairHeight = from.height - to.height;

            if (1 <= stairHeight && stairHeight <= 90)
            {

                if (to.leftEdge - 33 <= from.rightEdge && from.rightEdge <= to.leftEdge)

                    right_direction = true;

                else if (to.rightEdge <= from.leftEdge && from.leftEdge <= to.rightEdge + 33)

                    right_direction = false;

                else return;

                Point start = new Point(right_direction ? from.rightEdge : from.leftEdge, from.height);
                Point end = new Point(right_direction ? to.leftEdge : to.rightEdge, to.height);
                int length = (from.height - to.height) + Math.Abs(start.x - end.x);

                State new_state = new State(start.x, start.y, right_direction? 150 : -150, 0, MAX_RECTANGLE_HEIGHT);
                Move new_move = new Move(to, new_state, end, movementType.TRANSITION, new bool[graph.nCollectibles], length, false);
                graph.AddMove(from, new_move);
            }
        }

        public static void Trajectory(Graph graph, Platform from, State state, movementType type)
        {

            if (!EnoughLength(from, state)) return;

            float length = 0;
            bool ceiling = false;
            collideType collision_type = collideType.OTHER;
            bool[] collectibles = new bool[graph.nCollectibles];
            int fake_radius = Math.Min(graph.area / state.height, state.height) / 2;
            float velocity_y = (type == movementType.JUMP) ? JUMP_VELOCITYY : FALL_VELOCITYY;
            float velocity_x = state.v_x;

            Point start = new Point(state.x, state.y);
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
                        if (type == movementType.FALL && Math.Abs(state.v_x) > 1)
                            start.x = (state.v_x > 0) ? start.x - PIXEL_LENGTH : start.x + PIXEL_LENGTH;

                        State? partner_state = null;

                        if (from.type == platformType.RECTANGLE && type == movementType.FALL)
                        {
                            int rectangle_height = 50 + (from.height - (state.y + CIRCLE_RADIUS));
                            int rectangle_x = Math.Min(Math.Max(state.x, 137), 1063) + ((state.v_x >= 0) ? (-130) : 130);
                            int rectangle_y = state.y + CIRCLE_RADIUS + (rectangle_height / 2);
                            partner_state = new State(rectangle_x, rectangle_y, 0, 0, rectangle_height);
                        }

                        else if (from.type == platformType.RECTANGLE && type == movementType.JUMP)
                        {
                            int rectangle_height = 50 + (from.height - (state.y + CIRCLE_RADIUS));
                            int rectangle_x = Math.Min(Math.Max(state.x, 136), 1064);
                            int rectangle_y = state.y + CIRCLE_RADIUS + (rectangle_height / 2);
                            partner_state = new State(rectangle_x, rectangle_y, 0, 0, rectangle_height);
                        }

                        else if (collision_type == collideType.RECTANGLE)
                        {
                            int rectangle_x = Math.Min(Math.Max(collision.x, 136), 1064);
                            int rectangle_y = collision.y + CIRCLE_RADIUS + (MIN_RECTANGLE_HEIGHT / 2);
                            partner_state = new State(rectangle_x, rectangle_y, 0, 0, MIN_RECTANGLE_HEIGHT);
                        }

                        State new_state = new State(start.x, start.y, state.v_x, state.v_y, state.height);
                        Move new_move = new Move(to.Value, new_state, collision, type, collectibles, (int)length, ceiling, partner_state);
                        graph.AddMove(from, new_move);

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
                if (velocityY < 0 && (graph.levelArray[lowestY, centerArray.xArray] == BLACK || graph.levelArray[lowestY, centerArray.xArray] == graph.obstacle_colour))
                {
                    collide_type = collideType.FLOOR;
                }

                else if (velocityY < 0 && graph.levelArray[lowestY, centerArray.xArray] == RECTANGLE)
                {
                    collide_type = collideType.RECTANGLE;
                }

                else if (graph.levelArray[highestY, centerArray.xArray] == BLACK || graph.levelArray[highestY, centerArray.xArray] == graph.obstacle_colour)
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
