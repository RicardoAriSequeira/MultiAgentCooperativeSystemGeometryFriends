using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Perceptions.Information;

using static GeometryFriendsAgents.Graph;
using static GeometryFriendsAgents.GameInfo;
using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A rectangle agent implementation for the GeometryFriends game that demonstrates prediction and history keeping capabilities.
    /// </summary>
    public class RectangleAgent : AbstractRectangleAgent
    {
        // Sensors Information
        private LevelRepresentation levelInfo;
        private State circle_state;
        private State rectangle_state;

        // Graph
        private GraphRectangle graph;

        // Search Algorithm
        private SubgoalAStar subgoalAStar;

        // Reinforcement Learning
        private bool training = false;
        private ReinforcementLearning RL;
        private ActionSelector actionSelector;
        private long timeRL;

        // Cooperation
        private List<AgentMessage> messages;
        private CooperationStatus cooperation;

        // Auxiliary
        private Move? nextMove;
        private Moves currentAction;
        private DateTime lastMoveTime;
        private int targetPointX_InAir;
        private bool[] previousCollectibles;
        private Platform? previousPlatform, currentPlatform;

        public RectangleAgent()
        {
            nextMove = null;
            currentPlatform = null;
            previousPlatform = null;
            lastMoveTime = DateTime.Now;
            currentAction = Moves.NO_ACTION;

            graph = new GraphRectangle();
            subgoalAStar = new SubgoalAStar();
            actionSelector = new ActionSelector();
            levelInfo = new LevelRepresentation();
            timeRL = DateTime.Now.Second;

            messages = new List<AgentMessage>();
            cooperation = CooperationStatus.SINGLE;

            //ReinforcementLearning.WriteQTable(new float[ReinforcementLearning.N_STATES, ReinforcementLearning.N_ACTIONS]);

            if (training)
            {
                RL = new ReinforcementLearning();
            }
        }

        //implements abstract rectangle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            messages.Add(new AgentMessage(IST_RECTANGLE_PLAYING));

            // Create Level Array
            levelInfo.CreateLevelArray(colI, oI, rPI, cPI);

            // Initial Information
            circle_state = new State((int)cI.X, (int)cI.Y, (int)cI.VelocityX, (int)cI.VelocityY, (int)cI.Radius * 2);
            rectangle_state = new State((int)rI.X, (int)rI.Y, (int)rI.VelocityX, (int)rI.VelocityY, (int)rI.Height);
            targetPointX_InAir = rectangle_state.x;
            previousCollectibles = levelInfo.GetObtainedCollectibles();

            // Create Graph
            graph.Setup(levelInfo.GetLevelArray(), colI.Length);
            graph.SetPossibleCollectibles(rectangle_state);

            if (training)
            {
                int targetX = 600;
                int targetV = 0;
                int targetH = 100;
                bool rightTarget = true;
                RL.Setup(targetX, targetV, targetH, rightTarget);
            }
        }

        //implements abstract rectangle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            circle_state = new State((int)cI.X, (int)cI.Y, (int)cI.VelocityX, (int)cI.VelocityY, (int)cI.Radius * 2);
            rectangle_state = new State((int)rI.X, (int)rI.Y, (int)rI.VelocityX, (int)rI.VelocityY, (int)rI.Height);

            levelInfo.collectibles = colI;
        }

        //implements abstract rectangle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return true;
        }

        //implements abstract rectangle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return "IST Rectangle";
        }

        //implements abstract rectangle interface: GeometryFriends agents manager gets the current action intended to be actuated in the enviroment for this agent
        public override Moves GetAction()
        {
            //return training ? RL.GetAction(rectangle_state) : currentAction;
            currentAction = (currentAction == Moves.ROLL_LEFT) ? Moves.MOVE_LEFT : currentAction;
            currentAction = (currentAction == Moves.ROLL_RIGHT) ? Moves.MOVE_RIGHT : currentAction;
            return currentAction;
        }

        //implements abstract rectangle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {

            if (training)
            {
                //if (timeRL == 60)
                //    timeRL = 0;

                //if ((timeRL) <= (DateTime.Now.Second) && (timeRL < 60))
                //{
                //    if (!(DateTime.Now.Second == 59))
                //    {
                //        currentAction = RL.GetAction(rectangleInfo);
                //        timeRL = timeRL + 1;
                //    }
                //    else
                //        timeRL = 60;
                //}

                //return;

                //if ((DateTime.Now - lastMoveTime).TotalMilliseconds >= 20)
                //{
                //    currentAction = RL.GetAction(rectangleInfo);
                //    lastMoveTime = DateTime.Now;
                //}

                return;
            }

            if ((DateTime.Now - lastMoveTime).TotalMilliseconds >= 20)
            {

                currentAction = Moves.NO_ACTION;
                currentPlatform = graph.GetPlatform(new LevelRepresentation.Point(rectangle_state.x, rectangle_state.y), rectangle_state.height);

                if (currentPlatform.HasValue)
                {

                    if ((IsDifferentPlatform() || IsGetCollectible() || graph.HasChanged()) && cooperation == CooperationStatus.SINGLE)
                    {
                        targetPointX_InAir = (currentPlatform.Value.leftEdge + currentPlatform.Value.rightEdge) / 2;
                        Task.Factory.StartNew(SetNextMove);
                    }

                    if (nextMove.HasValue)
                    {

                        if (cooperation == CooperationStatus.SYNCHRONIZED || IsCircleInTheWay())
                        {
                            currentAction = Moves.NO_ACTION;
                            return;
                        }

                        if (Math.Abs(rectangle_state.v_y) <= MAX_VELOCITYY)
                        {

                            if (cooperation == CooperationStatus.RIDING_HELP)
                            {

                                if (nextMove.Value.type == movementType.COOPERATION && rectangle_state.height > nextMove.Value.state.height)
                                {
                                    currentAction = Moves.MORPH_DOWN;
                                }

                                else
                                {
                                    int targetX = circle_state.x + (nextMove.Value.partner_state.v_x >= 0 ? 50 : -50);

                                    if (nextMove.Value.ceiling)
                                    {
                                        targetX = targetX + (nextMove.Value.partner_state.v_x >= 0 ? 120 : -120);
                                    }

                                    if (currentPlatform.Value.leftEdge <= targetX && targetX <= currentPlatform.Value.rightEdge)
                                    {
                                        currentAction = actionSelector.GetCurrentAction(rectangle_state, targetX, 0, nextMove.Value.ToTheRight());
                                    }

                                }

                            }

                            else if (nextMove.Value.type == movementType.FALL && Math.Abs(nextMove.Value.state.v_x) == 1 &&
                                     rectangle_state.height > Math.Max((RECTANGLE_AREA / nextMove.Value.state.height) - 1, MIN_RECTANGLE_HEIGHT + 3))
                            {
                                currentAction = Moves.MORPH_DOWN;
                            }


                            else if (nextMove.Value.type == movementType.COOPERATION && cooperation == CooperationStatus.RIDING && Math.Abs(rectangle_state.x - circle_state.x) > 60)
                            {
                                currentAction = actionSelector.GetCurrentAction(rectangle_state, circle_state.x, 0, true);
                            }

                            else if (nextMove.Value.type == movementType.COOPERATION && cooperation == CooperationStatus.UNSYNCHRONIZED && rectangle_state.height > nextMove.Value.state.height)
                            {
                                currentAction = Moves.MORPH_DOWN;
                            }

                            else if (nextMove.Value.type == movementType.COOPERATION && cooperation == CooperationStatus.UNSYNCHRONIZED && rectangle_state.height < nextMove.Value.state.height - PIXEL_LENGTH)
                            {
                                currentAction = Moves.MORPH_UP;
                            }

                            else if (nextMove.Value.type == movementType.TRANSITION && rectangle_state.height > nextMove.Value.state.height)
                            {
                                currentAction = Moves.MORPH_DOWN;
                            }

                            else if (nextMove.Value.type == movementType.TRANSITION && rectangle_state.height < nextMove.Value.state.height - PIXEL_LENGTH)
                            {
                                currentAction = Moves.MORPH_UP;
                            }

                            else if (nextMove.Value.type == movementType.TRANSITION)
                            {
                                currentAction = nextMove.Value.ToTheRight() ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                            }

                            else currentAction = actionSelector.GetCurrentAction(rectangle_state, nextMove.Value.state.x, nextMove.Value.state.v_x, nextMove.Value.ToTheRight());


                        }

                        else currentAction = actionSelector.GetCurrentAction(rectangle_state, targetPointX_InAir, 0, true);

                    }
                }

                // NOT ON A PLATFORM
                else
                {
                    if (nextMove.HasValue)
                    {

                        if (nextMove.Value.type == movementType.TRANSITION)
                            currentAction = nextMove.Value.ToTheRight() ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;

                        else if (cooperation == CooperationStatus.SINGLE)
                            currentAction = actionSelector.GetCurrentAction(rectangle_state, targetPointX_InAir, 0, true);

                    }
                }

                lastMoveTime = DateTime.Now;
            }

            if (nextMove.HasValue && cooperation != CooperationStatus.RIDING_HELP)
            {

                if (actionSelector.IsGoal(rectangle_state, nextMove.Value.state) && Math.Abs(rectangle_state.v_y) <= MAX_VELOCITYY)
                {

                    targetPointX_InAir = (nextMove.Value.to.leftEdge + nextMove.Value.to.rightEdge) / 2;

                    if (rectangle_state.height >= nextMove.Value.state.height &&
                        (nextMove.Value.type == movementType.COOPERATION || nextMove.Value.type == movementType.TRANSITION))
                    {
                        currentAction = Moves.MORPH_DOWN;
                    }

                    else if (rectangle_state.height < nextMove.Value.state.height - PIXEL_LENGTH &&
                             (cooperation == CooperationStatus.RIDING ||
                             (nextMove.Value.type == movementType.FALL && Math.Abs(nextMove.Value.state.v_x) == 1) ||
                             nextMove.Value.type == movementType.COLLECT ||
                             nextMove.Value.type == movementType.TRANSITION))
                    {
                        currentAction = Moves.MORPH_UP;
                    }

                    else if (nextMove.Value.type == movementType.TRANSITION)
                        currentAction = nextMove.Value.ToTheRight() ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;

                    else if (cooperation == CooperationStatus.RIDING)
                        cooperation = CooperationStatus.SYNCHRONIZED;

                }

                else if (cooperation == CooperationStatus.SYNCHRONIZED)
                {
                    cooperation = CooperationStatus.UNSYNCHRONIZED;
                }
            }

        }

        private bool IsGetCollectible()
        {

            bool[] currentCollectibles = levelInfo.GetObtainedCollectibles();

            if (previousCollectibles.SequenceEqual(currentCollectibles))
            {
                return false;
            }


            for (int previousC = 0; previousC < previousCollectibles.Length; previousC++)
            {

                if (!previousCollectibles[previousC])
                {

                    for (int currentC = 0; currentC < currentCollectibles.Length; currentC++)
                    {

                        if (currentCollectibles[currentC])
                        {

                            foreach (Platform p in graph.platforms)
                            {

                                foreach (Move m in p.moves)
                                {

                                    m.collectibles[currentC] = false;

                                }

                            }

                        }
                    }
                }

            }

            previousCollectibles = currentCollectibles;

            return true;
        }

        private bool IsDifferentPlatform()
        {

            if (currentPlatform.HasValue)
            {
                if (!previousPlatform.HasValue ||
                    (currentPlatform.Value.id != previousPlatform.Value.id && currentPlatform.Value.type != platformType.GAP))
                {
                    previousPlatform = currentPlatform;
                    return true;
                }

            }

            previousPlatform = currentPlatform;
            return false;
        }

        private void SetNextMove()
        {
            nextMove = null;
            nextMove = subgoalAStar.CalculateShortestPath(currentPlatform.Value, new LevelRepresentation.Point(rectangle_state.x, rectangle_state.y),
                Enumerable.Repeat<bool>(true, levelInfo.initialCollectibles.Length).ToArray(),
                levelInfo.GetObtainedCollectibles(), levelInfo.initialCollectibles);
            graph.Process();

            if (nextMove.HasValue && nextMove.Value.type == movementType.COOPERATION && cooperation == CooperationStatus.SINGLE)
            {
                cooperation = CooperationStatus.UNSYNCHRONIZED;
            }
        }

        //implements abstract rectangle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            if (training)
            {
                RL.UpdateQTable();
            }
            Log.LogInformation("RECTANGLE - Collectibles caught = " + collectiblesCaught + ", Time elapsed - " + timeElapsed);
        }

        //implememts abstract agent interface: send messages to the circle agent
        public override List<AgentMessage> GetAgentMessages()
        {
            List<AgentMessage> toSent = new List<AgentMessage>(messages);
            messages.Clear();
            return toSent;
        }

        //implememts abstract agent interface: receives messages from the circle agent
        public override void HandleAgentMessages(List<AgentMessage> newMessages)
        {
            foreach (AgentMessage item in newMessages)
            {
                switch (item.Message)
                {

                    case INDIVIDUAL_MOVE:

                        if (item.Attachment.GetType() == typeof(Move))
                        {

                            cooperation = CooperationStatus.SINGLE;
                            Move move = (Move)item.Attachment;

                            foreach (Platform p in graph.platforms)
                                foreach (Move m in p.moves)
                                    for (int i = 0; i < m.collectibles.Length; i++)
                                        m.collectibles[i] = (move.collectibles[i] && m.collectibles[i]) ? true : m.collectibles[i];
                        }

                        break;

                    case UNSYNCHRONIZED:

                        if (item.Attachment.GetType() == typeof(Move))
                        {
                            Move move = (Move)item.Attachment;
                            State st = move.partner_state;
                            Platform? from = graph.GetPlatform(new LevelRepresentation.Point(st.x, st.y), st.height);

                            if (from.HasValue)
                            {
                                bool[] cols = Utilities.GetXorMatrix(graph.possibleCollectibles, move.collectibles);
                                Move newMove = new Move(from.Value, st, st.GetPosition(), movementType.COOPERATION, cols, 0, move.ceiling, move.state);
                                graph.AddMove(from.Value, newMove);
                                graph.Change();
                            }
                        }

                        break;

                    case RIDING:

                        if (item.Attachment.GetType() == typeof(Move))
                        {
                            cooperation = CooperationStatus.RIDING;
                            CleanRides();
                            Move move = (Move)item.Attachment;
                            Move newMove = move.Copy();
                            newMove.type = movementType.COOPERATION;
                            newMove.state = move.partner_state;
                            newMove.partner_state = move.state;
                            nextMove = newMove;
                        }
                        break;

                    case COOPERATION_FINISHED:

                        cooperation = CooperationStatus.SINGLE;
                        CleanRides();

                        currentPlatform = graph.GetPlatform(new LevelRepresentation.Point(rectangle_state.x, rectangle_state.y), rectangle_state.height);

                        if (currentPlatform.HasValue)
                        {
                            targetPointX_InAir = (currentPlatform.Value.leftEdge + currentPlatform.Value.rightEdge) / 2;
                            Task.Factory.StartNew(SetNextMove);
                        }

                        break;

                    case JUMPED:

                        if (nextMove.HasValue)
                        {
                            cooperation = CooperationStatus.RIDING_HELP;
                            Move move = nextMove.Value.Copy();
                            move.type = movementType.COOPERATION;
                            move.state.height = MIN_RECTANGLE_HEIGHT;
                            nextMove = move;
                        }

                        break;

                    case RIDING_HELP:

                        cooperation = CooperationStatus.RIDING_HELP;
                        break;

                }
            }

            return;
        }


        public bool IsCircleInTheWay()
        {

            if (nextMove.HasValue &&
                circle_state.y <= rectangle_state.y + (rectangle_state.height / 2) &&
                circle_state.y >= rectangle_state.y - (rectangle_state.height / 2))
            {

                int rectangleWidth = RECTANGLE_AREA / rectangle_state.height;

                if (nextMove.Value.ToTheRight() &&
                    circle_state.x >= rectangle_state.x &&
                    circle_state.x <= nextMove.Value.state.x + (rectangleWidth / 2) + CIRCLE_RADIUS)
                {
                    return true;
                }

                if (!nextMove.Value.ToTheRight() &&
                    circle_state.x <= rectangle_state.x &&
                    circle_state.x >= nextMove.Value.state.x - (rectangleWidth / 2) - CIRCLE_RADIUS)
                {
                    return true;
                }

            }

            return false;
        }

        public void CleanRides()
        {

            if (nextMove.HasValue && nextMove.Value.type == movementType.COOPERATION)
            {
                nextMove = null;
            }

            foreach (Platform p in graph.platforms)
            {
                int i = 0;

                while (i < p.moves.Count)
                {
                    if (p.moves[i].type == movementType.COOPERATION)
                    {
                        p.moves.Remove(p.moves[i]);
                    }
                    else
                    {
                        i++;
                    }

                }
            }
        }
    }
}