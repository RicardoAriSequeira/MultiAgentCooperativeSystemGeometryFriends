using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;

using GeometryFriends.AI;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Perceptions.Information;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A circle agent implementation for the GeometryFriends game that demonstrates prediction and history keeping capabilities.
    /// </summary>
    public class CircleAgent : AbstractCircleAgent
    {
        //Sensors Information
        private LevelRepresentation levelInfo;
        private CircleRepresentation circleInfo;
        private RectangleRepresentation rectangleInfo;

        // Graph
        private GraphCircle graph;

        // Search Algorithm
        private SubgoalAStar subgoalAStar;

        // Reinforcement Learning
        private ActionSelector actionSelector;

        // Messages
        private bool cooperation = false;
        private List<AgentMessage> messages;

        // Auxiliary Variables
        private Moves currentAction;
        private Graph.Move? nextEdge;
        private DateTime lastMoveTime;
        private int targetPointX_InAir;
        private bool[] previousCollectibles;
        private Graph.Platform? previousPlatform, currentPlatform;

        public CircleAgent()
        {
            nextEdge = null;
            currentPlatform = null;
            previousPlatform = null;
            lastMoveTime = DateTime.Now;
            currentAction = Moves.NO_ACTION;

            graph = new GraphCircle();
            subgoalAStar = new SubgoalAStar();
            actionSelector = new ActionSelector();
            levelInfo = new LevelRepresentation();

            messages = new List<AgentMessage>();
            messages.Add(new AgentMessage(GameInfo.IST_CIRCLE_PLAYING));
        }

        //implements abstract circle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            // Create Level Array
            levelInfo.CreateLevelArray(colI, oI, rPI, cPI, LevelRepresentation.GREEN);

            // Create Graph
            graph.SetupGraph(levelInfo.GetLevelArray(), colI.Length);
            messages.Add(new AgentMessage(GameInfo.IST_CIRCLE_GRAPH_COMPLETED, graph));

            // Initial Information
            circleInfo = cI;
            rectangleInfo = rI;
            targetPointX_InAir = (int)circleInfo.X;
            previousCollectibles = levelInfo.GetObtainedCollectibles();
        }

        //implements abstract circle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        /*WARNING: this method is called independently from the agent update - Update(TimeSpan elapsedGameTime) - so care should be taken when using complex 
         * structures that are modified in both (e.g. see operation on the "remaining" collection)      
         */
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            circleInfo = cI;
            rectangleInfo = rI;
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

            if ((DateTime.Now - lastMoveTime).TotalMilliseconds >= 20)
            {

                currentPlatform = graph.GetPlatform(new LevelRepresentation.Point((int)circleInfo.X, (int)circleInfo.Y), GameInfo.MAX_CIRCLE_HEIGHT);

                if (currentPlatform.HasValue)
                {
                    if (IsDifferentPlatform() || IsGetCollectible())
                    {
                        targetPointX_InAir = (currentPlatform.Value.leftEdge + currentPlatform.Value.rightEdge) / 2;
                        Task.Factory.StartNew(SetNextEdge);
                    }

                    if (nextEdge.HasValue)
                    {
                        if (-GameInfo.MAX_VELOCITYY <= circleInfo.VelocityY && circleInfo.VelocityY <= GameInfo.MAX_VELOCITYY)
                        {
                            if (nextEdge.Value.movementType == Graph.movementType.STAIR_GAP)
                            {
                                currentAction = nextEdge.Value.rightMove ? Moves.ROLL_RIGHT : Moves.ROLL_LEFT;
                            }
                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(circleInfo, nextEdge.Value.movePoint.x, nextEdge.Value.velocityX, nextEdge.Value.rightMove);
                            }
                        }
                        else
                        {
                            currentAction = actionSelector.GetCurrentAction(circleInfo, targetPointX_InAir, 0, true);
                        }
                    }
                }

                else
                {
                    if (nextEdge.HasValue)
                    {
                        if (nextEdge.Value.movementType == Graph.movementType.STAIR_GAP)
                        {
                            currentAction = nextEdge.Value.rightMove ? Moves.ROLL_RIGHT : Moves.ROLL_LEFT;
                        }
                        else
                        {
                            if (nextEdge.Value.collideCeiling && circleInfo.VelocityY < 0)
                            {
                                currentAction = Moves.NO_ACTION;
                            }
                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(circleInfo, targetPointX_InAir, 0, true);
                            }
                        }
                    }
                }

                if (!nextEdge.HasValue)
                {
                    currentAction = actionSelector.GetCurrentAction(circleInfo, (int)circleInfo.X, 0, false);
                }

                lastMoveTime = DateTime.Now;
            }

            if (nextEdge.HasValue)
            {
                if (!actionSelector.IsGoal(circleInfo, nextEdge.Value.movePoint.x, nextEdge.Value.velocityX, nextEdge.Value.rightMove))
                {
                    return;
                }

                if (-GameInfo.MAX_VELOCITYY <= circleInfo.VelocityY && circleInfo.VelocityY <= GameInfo.MAX_VELOCITYY)
                {
                    targetPointX_InAir = (nextEdge.Value.reachablePlatform.leftEdge + nextEdge.Value.reachablePlatform.rightEdge) / 2;

                    if (nextEdge.Value.movementType == Graph.movementType.JUMP)
                    {
                        currentAction = Moves.JUMP;
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
            nextEdge = null;
            nextEdge = subgoalAStar.CalculateShortestPath(currentPlatform.Value, new LevelRepresentation.Point((int)circleInfo.X, (int)circleInfo.Y),
                Enumerable.Repeat<bool>(true, levelInfo.initialCollectibles.Length).ToArray(),
                levelInfo.GetObtainedCollectibles(), levelInfo.initialCollectibles);
        }

        //implements abstract circle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Console.WriteLine("CIRCLE - Collectibles caught = {0}, Time elapsed - {1}", collectiblesCaught, timeElapsed);
        }

        public override List<GeometryFriends.AI.Communication.AgentMessage> GetAgentMessages()
        {
            List<AgentMessage> toSent = new List<AgentMessage>(messages);
            messages.Clear();
            return toSent;
        }

        //implememts abstract agent interface: receives messages from the circle agent
        public override void HandleAgentMessages(List<GeometryFriends.AI.Communication.AgentMessage> newMessages)
        {
            foreach (AgentMessage item in newMessages)
            {
                //Log.LogInformation("Rectangle: received message from circle: " + item.Message);
                //if (item.Attachment != null)
                //{
                //    Log.LogInformation("Received message has attachment: " + item.Attachment.ToString());
                //    if (item.Attachment.GetType() == typeof(Pen))
                //    {
                //        Log.LogInformation("The attachment is a pen, let's see its color: " + ((Pen)item.Attachment).Color.ToString());
                //    }
                //}

                if (item.Message.Equals(GameInfo.IST_RECTANGLE_PLAYING))
                {
                    this.cooperation = true;
                }

                else if (item.Message.Equals(GameInfo.IST_RECTANGLE_GRAPH_COMPLETED))
                {
                    continue;
                }

            }

            return;
        }

        public static bool IsObstacle_onPixels(int[,] levelArray, List<LevelRepresentation.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelRepresentation.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelRepresentation.BLACK || levelArray[i.yArray, i.xArray] == LevelRepresentation.GREEN)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

