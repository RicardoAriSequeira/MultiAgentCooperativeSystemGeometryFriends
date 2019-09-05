using System;
using GeometryFriends.AI;
using static GeometryFriendsAgents.Graph;

namespace GeometryFriendsAgents
{
    class ActionSelector
    {
        private const int NUM_POSSIBLE_MOVES = 2;
        private const int ACCELERATE = 0;
        private const int DEACCELERATE = 1;

        private const int MAX_H = GameInfo.MAX_RECTANGLE_HEIGHT - GameInfo.MIN_RECTANGLE_HEIGHT;
        private const int MAX_D = 200;
        private const int MAX_V = GameInfo.MAX_VELOCITYX;

        private const int DISCRETIZATION_V = 10;
        private const int DISCRETIZATION_H = LevelRepresentation.PIXEL_LENGTH;
        private const int DISCRETIZATION_D = 4;

        private const int MAX_DISCRETIZED_V = MAX_V * 2 / DISCRETIZATION_V;
        private const int MAX_DISCRETIZED_D = MAX_D * 2 / DISCRETIZATION_D;
        private const int MAX_DISCRETIZED_H = MAX_H * 2 / DISCRETIZATION_H;

        private const int NUM_STATE = MAX_DISCRETIZED_V * MAX_DISCRETIZED_D;
        private const int NUM_TARGET_V = MAX_V / (DISCRETIZATION_V * 2);

        private const int NUM_ROW_QMAP = NUM_STATE;
        private const int NUM_COLUMN_QMAP = NUM_POSSIBLE_MOVES * NUM_TARGET_V;

        private float[,] Qmap;   

        public ActionSelector()
        {
            Qmap = Utilities.ReadCsvFile(NUM_ROW_QMAP, NUM_COLUMN_QMAP, "Agents\\Qmap.csv");
        }

        public bool IsGoal(State st, State goal)
        {
            int target_velocity = Math.Abs(goal.v_x);

            float distanceX = (goal.v_x >= 0) ? st.x - goal.x : goal.x - st.x;

            if (-DISCRETIZATION_D * 2 < distanceX && distanceX <= 0)
            {
                float relativeVelocityX = (goal.v_x >= 0) ? st.v_x : -st.v_x;

                if (target_velocity == 0)
                {
                    if (target_velocity - DISCRETIZATION_V <= relativeVelocityX && relativeVelocityX < target_velocity + DISCRETIZATION_V)
                    {
                        return true;
                    }
                }
                else
                {
                    if (target_velocity <= relativeVelocityX && relativeVelocityX < target_velocity + DISCRETIZATION_V * 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public Moves GetCurrentAction(State st, int targetPointX, int targetVelocityX, bool rightMove)
        {
            int stateNum = GetStateNum(st, targetPointX, rightMove);

            int currentActionNum;

            float distanceX = rightMove ? st.x - targetPointX : targetPointX - st.x;

            if (distanceX <= -MAX_D)
            {
                currentActionNum = ACCELERATE;
            }
            else if (distanceX >= MAX_D)
            {
                currentActionNum = DEACCELERATE;
            }
            else
            {
                currentActionNum = GetOptimalActionNum(stateNum, targetVelocityX);
            }
            
            Moves currentAction;

            if (currentActionNum == ACCELERATE)
            {
                currentAction = rightMove ? Moves.ROLL_RIGHT : Moves.ROLL_LEFT;
            }
            else
            {
                currentAction = rightMove ? Moves.ROLL_LEFT : Moves.ROLL_RIGHT;
            }

            return currentAction;
        }

        public int GetStateNum(State st, int targetPointX, bool right)
        {
            // discretized target velocity
            int discretized_V = ((right ? st.v_x : -st.v_x) + MAX_V) / DISCRETIZATION_V;
            discretized_V = Math.Min(Math.Max(discretized_V, 0), MAX_DISCRETIZED_V - 1);

            // discretized distance to target
            int discretized_D = ((right ? st.x - targetPointX : targetPointX - st.x) + MAX_D) / DISCRETIZATION_D;
            discretized_D = Math.Min(Math.Max(discretized_D, 0), MAX_DISCRETIZED_D - 1);

            // state number
            return discretized_V + discretized_D * MAX_DISCRETIZED_V;
        }

        private int GetOptimalActionNum(int stateNum, int targetVelocityX)
        {
            int maxColumnNum = 0;
            float maxValue = float.MinValue;

            int from = (Math.Abs(targetVelocityX) / (DISCRETIZATION_V * 2)) * 2;
            int to = from + NUM_POSSIBLE_MOVES;

            for (int i = from; i < to; i++)
            {
                if (maxValue < Qmap[stateNum, i])
                {
                    maxValue = Qmap[stateNum, i];
                    maxColumnNum = i;
                }
            }

            return maxColumnNum - from;
        }
    }
}
