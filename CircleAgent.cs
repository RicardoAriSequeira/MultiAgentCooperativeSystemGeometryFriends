﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using GeometryFriends.AI;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Perceptions.Information;
using static GeometryFriendsAgents.GameInfo;
using static GeometryFriendsAgents.Graph;
using static GeometryFriendsAgents.LevelRepresentation;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A circle agent implementation for the GeometryFriends game that demonstrates prediction and history keeping capabilities.
    /// </summary>
    public class CircleAgent : AbstractCircleAgent
    {
        //Sensors Information
        private LevelRepresentation levelInfo;
        private State circle_state;
        private State rectangle_state;

        // Graph
        private GraphCircle graph;

        // Search Algorithm
        private SubgoalAStar subgoalAStar;

        // Reinforcement Learning
        private ActionSelector actionSelector;

        // Cooperation
        private bool rectangle_ping;
        private CooperationStatus cooperation;
        private List<AgentMessage> messages;

        // Auxiliary Variables
        private Moves currentAction;
        private Move? nextMove;
        private DateTime lastMoveTime;
        private int targetPointX_InAir;
        private bool[] previousCollectibles;
        private Platform? previousPlatform, currentPlatform;

        public CircleAgent()
        {
            nextMove = null;
            currentPlatform = null;
            previousPlatform = null;
            lastMoveTime = DateTime.Now;
            currentAction = Moves.NO_ACTION;

            graph = new GraphCircle();
            subgoalAStar = new SubgoalAStar();
            actionSelector = new ActionSelector();
            levelInfo = new LevelRepresentation();

            rectangle_ping = false;
            messages = new List<AgentMessage>();
            cooperation = CooperationStatus.SINGLE;
        }

        //implements abstract circle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, System.Drawing.Rectangle area, double timeLimit)
        {
        
            messages.Add(new AgentMessage(IST_CIRCLE_PLAYING));

            // Create Level Array
            levelInfo.CreateLevelArray(colI, oI, rPI, cPI);

            // Initial Information
            targetPointX_InAir = (int)cI.X;
            previousCollectibles = levelInfo.GetObtainedCollectibles();
            rectangle_state = new State((int)rI.X, (int)rI.Y, (int)rI.VelocityX, (int)rI.VelocityY, (int)rI.Height, rI.VelocityX >= 0);
            circle_state = new State((int)cI.X, (int)cI.Y, (int)cI.VelocityX, (int)cI.VelocityY, (int)cI.Radius * 2, cI.VelocityX >= 0);

            // Create Graph
            graph.initial_rectangle_state = rectangle_state;
            graph.SetupGraph(levelInfo.GetLevelArray(), colI.Length);
            graph.SetPossibleCollectibles(circle_state);

            if (rI.X < 0 || rI.Y < 0)
            {
                graph.DeleteCooperationPlatforms();
            }
        }

        //implements abstract circle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        /*WARNING: this method is called independently from the agent update - Update(TimeSpan elapsedGameTime) - so care should be taken when using complex 
         * structures that are modified in both (e.g. see operation on the "remaining" collection)      
         */
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            circle_state = new State((int)cI.X, (int)cI.Y, (int)cI.VelocityX, (int)cI.VelocityY, (int)cI.Radius * 2, cI.VelocityX >= 0);
            rectangle_state = new State((int)rI.X, (int)rI.Y, (int)rI.VelocityX, (int)rI.VelocityY, (int)rI.Height, rI.VelocityX >= 0);
            levelInfo.collectibles = colI;
        }

        //implements abstract circle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return true;
        }

        //implements abstract circle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return "IST Circle";
        }

        //implements abstract circle interface: GeometryFriends agents manager gets the current action intended to be actuated in the enviroment for this agent
        public override Moves GetAction()
        {
            return currentAction;
        }

        //implements abstract circle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {

            if ((cooperation == CooperationStatus.UNSYNCHRONIZED || cooperation == CooperationStatus.RIDE || cooperation == CooperationStatus.RIDE_HELP) &&
                actionSelector.IsGoal(rectangle_state, nextMove.Value.partner_state) &&
                Math.Abs(rectangle_state.height - nextMove.Value.partner_state.height) < 8)
            {
                cooperation = CooperationStatus.SYNCHRONIZED;
            }

            if ((DateTime.Now - lastMoveTime).TotalMilliseconds >= 20)
            {

                currentPlatform = graph.GetPlatform(new Point(circle_state.x, circle_state.y), CIRCLE_HEIGHT, circle_state.v_y);

                if (!currentPlatform.HasValue && IsRidingRectangle())
                {
                    currentPlatform = previousPlatform;  
                }

                if (currentPlatform.HasValue)
                {

                    if (IsDifferentPlatform() || IsGetCollectible())
                    {
                        targetPointX_InAir = (currentPlatform.Value.leftEdge + currentPlatform.Value.rightEdge) / 2;
                        Task.Factory.StartNew(SetNextEdge);
                    }

                    if (nextMove.HasValue)
                    {

                        if (cooperation == CooperationStatus.RIDE)
                        {
                            currentAction = actionSelector.GetCurrentAction(circle_state, rectangle_state.x, 0, true);
                        }

                        else if (cooperation == CooperationStatus.UNSYNCHRONIZED)
                        {
                            currentAction = Moves.NO_ACTION;
                        }

                        else if (Math.Abs(circle_state.v_y) <= MAX_VELOCITYY)
                        {
                            if (nextMove.Value.type == movementType.TRANSITION)
                            {
                                currentAction = nextMove.Value.state.right_direction ? Moves.ROLL_RIGHT : Moves.ROLL_LEFT;
                            }
                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(circle_state, nextMove.Value.state.x, nextMove.Value.state.v_x, nextMove.Value.state.right_direction);
                            }
                        }
                        else
                        {
                            currentAction = actionSelector.GetCurrentAction(circle_state, targetPointX_InAir, 0, true);
                        }

                    }

                    else if (cooperation != CooperationStatus.SINGLE && currentPlatform.Value.type != platformType.RECTANGLE)
                    {
                        cooperation = CooperationStatus.SINGLE;
                        messages.Add(new AgentMessage(COOPERATION_FINISHED));
                    }
                }

                else
                {
                    if (nextMove.HasValue)
                    {
                        if ((cooperation == CooperationStatus.SYNCHRONIZED && nextMove.Value.reachablePlatform.type == platformType.RECTANGLE) || cooperation == CooperationStatus.RIDE_HELP)
                        {
                            currentAction = actionSelector.GetCurrentAction(circle_state, rectangle_state.x, 0, true);
                        }
                        else if (nextMove.Value.type == movementType.JUMP || nextMove.Value.type == movementType.FALL)
                        {
                            currentAction = Moves.NO_ACTION;
                        }
                        else if (nextMove.Value.type == movementType.TRANSITION)
                        {
                            currentAction = nextMove.Value.state.right_direction ? Moves.ROLL_RIGHT : Moves.ROLL_LEFT;
                        }
                        else
                        {
                            if (nextMove.Value.collideCeiling && circle_state.v_y < 0)
                            {
                                currentAction = Moves.NO_ACTION;
                            }
                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(circle_state, targetPointX_InAir, 0, true);
                            }
                        }
                    }
                }

                lastMoveTime = DateTime.Now;
            }

            if (nextMove.HasValue)
            {

                if (cooperation == CooperationStatus.RIDE_HELP && currentPlatform.HasValue && currentPlatform.Value.type == platformType.RECTANGLE && IsRidingRectangle())
                {
                    cooperation = CooperationStatus.RIDE;
                }

                if (actionSelector.IsGoal(circle_state, nextMove.Value.state) && Math.Abs(circle_state.v_y) <= MAX_VELOCITYY)
                {
                    targetPointX_InAir = (nextMove.Value.reachablePlatform.leftEdge + nextMove.Value.reachablePlatform.rightEdge) / 2;

                    if (nextMove.Value.type == movementType.JUMP && cooperation == CooperationStatus.SINGLE)
                    {
                        currentAction = Moves.JUMP;
                    }

                    if (nextMove.Value.type == movementType.JUMP && cooperation == CooperationStatus.SYNCHRONIZED)
                    {
                        currentAction = Moves.JUMP;

                        if (nextMove.Value.collideCeiling)
                        {
                            cooperation = CooperationStatus.RIDE_HELP;
                            messages.Add(new AgentMessage(RIDE_HELP));
                        }

                        else if (nextMove.Value.reachablePlatform.type == platformType.RECTANGLE)
                        {
                            messages.Add(new AgentMessage(JUMPED));
                        }

                    }
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

                            foreach (Graph.Platform p in graph.platforms)
                            {

                                foreach (Graph.Move m in p.moves)
                                {

                                    m.collectibles_onPath[currentC] = false;

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
                if (!previousPlatform.HasValue)
                {
                    previousPlatform = currentPlatform;
                    return true;
                }
                else if (currentPlatform.Value.id != previousPlatform.Value.id)
                {
                    previousPlatform = currentPlatform;
                    return true;
                }
            }

            previousPlatform = currentPlatform;
            return false;
        }

        private void SetNextEdge()
        {

            Move? previousMove = nextMove;

            nextMove = null;
            nextMove = subgoalAStar.CalculateShortestPath(currentPlatform.Value, new Point((int)circle_state.x, (int)circle_state.y),
                Enumerable.Repeat<bool>(true, levelInfo.initialCollectibles.Length).ToArray(),
                levelInfo.GetObtainedCollectibles(), levelInfo.initialCollectibles);

            if (nextMove.HasValue)
            {

                if (previousMove.HasValue &&
                (previousMove.Value.type != nextMove.Value.type ||
                previousMove.Value.state.x != nextMove.Value.state.x ||
                previousMove.Value.landPoint.x != nextMove.Value.landPoint.x) &&
                cooperation == CooperationStatus.UNSYNCHRONIZED)
                {
                    cooperation = CooperationStatus.SINGLE;
                    messages.Add(new AgentMessage(COOPERATION_FINISHED));
                }

                if (nextMove.Value.reachablePlatform.type == platformType.RECTANGLE || IsRidingRectangle())
                {
                    Cooperate();
                }

                if (currentPlatform.HasValue && currentPlatform.Value.type != platformType.RECTANGLE && nextMove.Value.reachablePlatform.type != platformType.RECTANGLE)
                {
                    cooperation = CooperationStatus.SINGLE;
                    messages.Add(new AgentMessage(COOPERATION_FINISHED));
                }

                if (nextMove.Value.reachablePlatform.type != platformType.RECTANGLE && cooperation == CooperationStatus.SINGLE)
                {
                    messages.Add(new AgentMessage(INDIVIDUAL_MOVE, nextMove));
                }

            }

        }

        private void Cooperate()
        {

            State partner_state = new State();

            Move rectangle_move = CopyMove((Move)nextMove);
            rectangle_move.collectibles_onPath = graph.GetCollectiblesFromCooperation(rectangle_move);

            Platform fromPlatform = currentPlatform.HasValue ? (Platform) currentPlatform : (Platform) previousPlatform;

            if (fromPlatform.type == platformType.RECTANGLE)
            {
                int rectangle_height = 50 + (fromPlatform.height - (rectangle_move.state.y + CIRCLE_RADIUS));

                int rectangle_x = Math.Min(Math.Max(rectangle_move.state.x, 137), 1063);

                if (rectangle_move.type == movementType.FALL)
                {
                    rectangle_x += rectangle_move.state.right_direction ? (-120) : 120;
                }

                partner_state = new State(rectangle_x, rectangle_move.state.y, 0, 0, rectangle_height, rectangle_move.state.right_direction);
                rectangle_move.state = partner_state;

                messages.Add(new AgentMessage(RIDE, rectangle_move));
                cooperation = CooperationStatus.RIDE;
            }
            else
            {

                bool rectangleIsRight = (rectangle_state.x - circle_state.x > 0);

                if (rectangleIsRight != rectangle_move.state.right_direction)
                {
                    foreach (Move m in fromPlatform.moves)
                    {
                        if (m.reachablePlatform.id == rectangle_move.reachablePlatform.id && m.type == movementType.JUMP && m.state.right_direction != rectangle_move.state.right_direction)
                        {
                            nextMove = m;
                            rectangle_move = CopyMove(m);
                            rectangle_move.collectibles_onPath = graph.GetCollectiblesFromCooperation(m);
                            break;
                        }
                    }
                }

                int rectangle_height = 50 + (rectangle_move.reachablePlatform.height - (rectangle_move.landPoint.y + CIRCLE_RADIUS));
                int rectangle_x = Math.Min(Math.Max(rectangle_move.landPoint.x, 137), 1063);
                int rectangle_y = rectangle_move.landPoint.y + CIRCLE_RADIUS + (rectangle_height / 2);
                partner_state = new State(rectangle_x, rectangle_y, 0, 0, rectangle_height, rectangle_move.state.right_direction);

                rectangle_move.type = movementType.RIDE;
                rectangle_move.state = partner_state;

                messages.Add(new AgentMessage(UNSYNCHRONIZED, rectangle_move));
                cooperation = CooperationStatus.UNSYNCHRONIZED;

            }

            Move new_nextMove = CopyMove((Move)nextMove);
            new_nextMove.partner_state = partner_state;
            nextMove = new_nextMove;

        }

        //implements abstract circle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Console.WriteLine("CIRCLE - Collectibles caught = {0}, Time elapsed - {1}", collectiblesCaught, timeElapsed);
        }

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
                if (item.Message.Equals(IST_RECTANGLE_PLAYING))
                    rectangle_ping = true;
            }

            if (!rectangle_ping)
                graph.DeleteCooperationPlatforms();

            rectangle_ping = true;
        }

        public bool IsRidingRectangle()
        {

            int rectangleWidth = RECTANGLE_AREA / (int)rectangle_state.height;

            if (Math.Abs(circle_state.v_y) < MAX_VELOCITYY &&
                (circle_state.x >= rectangle_state.x - (rectangleWidth / 2)) &&
                (circle_state.x <= rectangle_state.x + (rectangleWidth / 2)) &&
                 Math.Abs(rectangle_state.y - (rectangle_state.height / 2) - CIRCLE_RADIUS - circle_state.y) <= 8)
            {
                return true;
            }

            return false;
        }

    }
}

