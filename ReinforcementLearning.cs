using System.Globalization;
using System.IO;

using GeometryFriends.AI;
using System.Collections.Generic;
using System;
using GeometryFriends.AI.Perceptions.Information;

using static GeometryFriendsAgents.Graph;
using static GeometryFriendsAgents.GameInfo;

namespace GeometryFriendsAgents
{
    class ReinforcementLearning
    {
        private const int ACCELERATE = 0;
        private const int DEACCELERATE = 1;
        private const int MORPH_UP = 2;
        private const int MORPH_DOWN = 3;

        private const int DISCRETIZATION_D = 4;
        private const int DISCRETIZATION_V = 10;

        private const int MAX_D = 200;
        private const int MAX_V = MAX_VELOCITYX;

        private const int MAX_DISCRETIZED_D = MAX_D * 2 / DISCRETIZATION_D;
        private const int MAX_DISCRETIZED_V = MAX_V * 2 / DISCRETIZATION_V;
        private const int MAX_DISCRETIZED_H = 3;

        private const int N_ACTIONS = 2;
        private const int N_TARGET_V = 4;
        private const int N_STATES = MAX_DISCRETIZED_V * MAX_DISCRETIZED_D * MAX_DISCRETIZED_H;

        public static int N_ROWS_QMAP = N_STATES;
        public static int N_COLUMNS_QMAP = N_ACTIONS * N_TARGET_V;

        // PARAMETERS
        private const float E_GREEDY = 0.6F;
        private const float LEARNING_RATE = 0.1F;
        private const float DISCOUNT_RATE = 0.999F;

        private State goal;
        private bool training;
        private float[,] QTable;
        private int previous_s = -1;
        private int previous_a = -1;

        public void Setup(int targetV, int targetH, bool training)
        {
            QTable = Utilities.ReadCsvFile(N_ROWS_QMAP, N_COLUMNS_QMAP, "Agents\\QTableRectangle.csv");
            goal = new State(600, 0, targetV, 0, targetH);
            this.training = training;
        }

        public bool IsGoal(State st)
        {
            int goal_v_discretized = goal.v_x / DISCRETIZATION_V;

            int d_discretized = Math.Abs((goal.x - st.x) / DISCRETIZATION_D);
            int v_discretized = st.v_x / DISCRETIZATION_V;
            bool h_discretized = (goal.height - 4 <= st.height && st.height <= goal.height);

            if (v_discretized == goal_v_discretized && d_discretized == 0 && h_discretized)
            {
                return true;
            }

            return false;
        }

        public int GetS(State st)
        {
            bool right = (goal.v_x >= 0);

            // discretized target velocity
            int discretized_V = ((right ? st.v_x : -st.v_x) + MAX_V) / DISCRETIZATION_V;
            discretized_V = Math.Min(Math.Max(discretized_V, 0), MAX_DISCRETIZED_V - 1);

            // discretized distance to target
            int discretized_D = ((right ? st.x - goal.x : goal.x - st.x) + MAX_D) / DISCRETIZATION_D;
            discretized_D = Math.Min(Math.Max(discretized_D, 0), MAX_DISCRETIZED_D - 1);

            int discretized_H = 0;

            if (50 <= st.height && st.height <= 54)
            {
                discretized_H = 0;
            }
            else if (96 <= st.height && st.height <= 100)
            {
                discretized_H = 1;
            }
            else if (192 <= st.height && st.height <= 196)
            {
                discretized_H = 2;
            }

            // state number
            return discretized_V + discretized_D * MAX_DISCRETIZED_V + discretized_H * MAX_DISCRETIZED_V * MAX_DISCRETIZED_D;
        }

        public Moves GetAction(State st, bool is_goal)
        {
            bool right_direction = (goal.v_x >= 0);
            int s = GetS(st);
            int greedy_a = GetGreedyAction(s);

            int action_num;

            float distanceX = right_direction ? st.x - goal.x : goal.x - st.x;

            if (st.height > goal.height)
            {
                action_num = MORPH_DOWN;
            }
            else if (st.height < goal.height - 4)
            {
                action_num = MORPH_UP;
            }
            else if (distanceX <= -MAX_D)
            {
                action_num = ACCELERATE;
            }
            else if (distanceX >= MAX_D)
            {
                action_num = DEACCELERATE;
            }
            else if (!training)
            {
                action_num = greedy_a;
            }
            else {
                Random rnd = new Random();
                int random_number = rnd.Next(100);
                bool random = (random_number < (E_GREEDY * 100));
                action_num = random ? GetRandomAction() : greedy_a;
            }

            Moves action;

            if (action_num == ACCELERATE)
            {
                action = right_direction ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
            }
            else if (action_num == DEACCELERATE)
            {
                action = right_direction ? Moves.MOVE_LEFT : Moves.MOVE_RIGHT;
            }
            else if (action_num == MORPH_UP)
            {
                action = Moves.MORPH_UP;
            }
            else if (action_num == MORPH_DOWN)
            {
                action = Moves.MORPH_DOWN;
            }
            else
            {
                action = Moves.NO_ACTION;
            }

            if (previous_s >= 0 && previous_a >= 0 && training)
            {
                UpdateQTableEntry(previous_s, previous_a, s, is_goal, greedy_a);
            }

            previous_s = s;
            previous_a = action_num;

            return action;
        }

        private void UpdateQTableEntry(int s, int a, int new_s, bool is_goal, int greedy_a)
        {
            int column = ((Math.Abs(goal.v_x) / (DISCRETIZATION_V * 2)) * 2) + a;
            int greedy_column = ((Math.Abs(goal.v_x) / (DISCRETIZATION_V * 2)) * 2) + greedy_a;
            int reward = (is_goal ? 200 : -1);
            float max_q = QTable[new_s, greedy_column];
            float new_entry = LEARNING_RATE * (reward + (DISCOUNT_RATE * max_q) - QTable[s, column]);
            QTable[s, column] = Math.Max(QTable[s, column] + new_entry, -49.99998F);
        }

        private int GetGreedyAction(int s)
        {
            int maxColumnNum = 0;
            float maxValue = float.MinValue;

            int from = (Math.Abs(goal.v_x) / (DISCRETIZATION_V * 2)) * 2;
            int to = from + N_ACTIONS;

            for (int a = from; a < to; a++)
            {
                if (maxValue < QTable[s, a])
                {
                    maxValue = QTable[s, a];
                    maxColumnNum = a;
                }
            }

            return maxColumnNum - from;
        }

        private int GetRandomAction()
        {
            Random rnd = new Random();
            return rnd.Next(2);
        }

        public void UpdateQTable(/*State st*/)
        {
            if (training)
            {
                //bool is_goal = IsGoal(st);
                //int s = GetS(st);
                //int greedy_a = GetGreedyAction(s);

                //if (previous_s >= 0 && previous_a >= 0 && s != previous_s)
                //{
                //    UpdateQTableEntry(previous_s, previous_a, s, is_goal, greedy_a);
                //}

                WriteQTable(QTable);
            }

        }

        public static void WriteQTable(float[,] QTable)
        {

            NumberFormatInfo nfi = new NumberFormatInfo
            {
                NumberDecimalSeparator = ".",
                NumberDecimalDigits = 5
            };

            using (var w = new StreamWriter("Agents\\QTableRectangle.csv"))
            {
                for (int row = 0; row < N_ROWS_QMAP; row++)
                {
                    string line = string.Format(nfi, "{0:N5}", QTable[row, 0]);
                    //string line = string.Format(nfi, "-49.99998", QTable[row, 0]);

                    for (int column = 1; column < N_COLUMNS_QMAP; column++)
                    {
                        line = string.Format(nfi, "{0},{1:N5}", line, QTable[row, column]);
                        //line = string.Format(nfi, "{0},-49.99998", line, QTable[row, column]);
                    }

                    w.WriteLine(line);

                }

                w.Flush();

            }

        }

        public static void InitializeQTable()
        {
            float[,] new_qtable = new float[N_ROWS_QMAP, N_COLUMNS_QMAP];

            //for (int row = 0; row < N_ROWS_QMAP; row++)
            //{
            //    for (int column = 0; column < N_COLUMNS_QMAP; column++)
            //    {
            //        new_qtable[row, column] = -49.99998F;
            //    }
            //}

            WriteQTable(new_qtable);
        }

    }
}
