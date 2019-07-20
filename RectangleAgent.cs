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

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A rectangle agent implementation for the GeometryFriends game that demonstrates prediction and history keeping capabilities.
    /// </summary>
    public class RectangleAgent : AbstractRectangleAgent
    {
        // Sensors Information
        private LevelRepresentation levelInfo;
        private CircleRepresentation circleInfo;
        private RectangleRepresentation rectangleInfo;

        // Graph
        private GraphRectangle graph;

        // Search Algorithm
        private SubgoalAStar subgoalAStar;

        // Reinforcement Learning
        private ActionSelector actionSelector;

        // Messages
        private bool cooperation = false;
        private List<AgentMessage> messages;

        // Auxiliary
        private Moves currentAction;
        private Graph.Move? nextEdge;
        private DateTime lastMoveTime;
        private int targetPointX_InAir;
        private bool[] previousCollectibles;
        private Graph.Platform? previousPlatform, currentPlatform;

        public RectangleAgent()
        {
            nextEdge = null;
            currentPlatform = null;
            previousPlatform = null;
            lastMoveTime = DateTime.Now;
            currentAction = Moves.NO_ACTION;

            graph = new GraphRectangle();
            subgoalAStar = new SubgoalAStar();
            actionSelector = new ActionSelector();
            levelInfo = new LevelRepresentation();

            messages = new List<AgentMessage>();
            messages.Add(new AgentMessage(GameInfo.IST_RECTANGLE_PLAYING));
        }

        //implements abstract rectangle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            // Create Level Array
            levelInfo.CreateLevelArray(colI, oI, rPI, cPI, LevelRepresentation.YELLOW);

            // Create Graph
            graph.SetupGraph(levelInfo.GetLevelArray(), colI.Length);
            messages.Add(new AgentMessage(GameInfo.IST_RECTANGLE_GRAPH_COMPLETED, graph));

            // Initial Information
            circleInfo = cI;
            rectangleInfo = rI;
            targetPointX_InAir = (int)rectangleInfo.X;
            previousCollectibles = levelInfo.GetObtainedCollectibles();
        }

        //implements abstract rectangle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            rectangleInfo = rI;
            circleInfo = cI;
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
            return currentAction;
        }

        //implements abstract rectangle interface: updates the agent state logic and predictions
        public override void Update(TimeSpan elapsedGameTime)
        {

            if ((DateTime.Now - lastMoveTime).TotalMilliseconds >= 20)
            {

                currentPlatform = graph.GetPlatform(new LevelRepresentation.Point((int)rectangleInfo.X, (int)rectangleInfo.Y), rectangleInfo.Height);

                // rectangle on a platform
                if (currentPlatform.HasValue)
                {
                    if (IsDifferentPlatform() || IsGetCollectible())
                    {
                        targetPointX_InAir = (currentPlatform.Value.leftEdge + currentPlatform.Value.rightEdge) / 2;
                        Task.Factory.StartNew(SetNextEdge);
                    }

                    if (nextEdge.HasValue)
                    {
                        if (-GameInfo.MAX_VELOCITYY <= rectangleInfo.VelocityY && rectangleInfo.VelocityY <= GameInfo.MAX_VELOCITYY)
                        {

                            if (nextEdge.Value.movementType == Graph.movementType.GAP &&
                                rectangleInfo.Height > Math.Max((GameInfo.RECTANGLE_AREA / nextEdge.Value.height) - 1, GameInfo.MIN_RECTANGLE_HEIGHT + 3))
                            {
                                currentAction = Moves.MORPH_DOWN;
                            }

                            else if (nextEdge.Value.movementType == Graph.movementType.STAIR_GAP || 
                                nextEdge.Value.movementType == Graph.movementType.MORPH_DOWN)
                            {
                                if (rectangleInfo.Height >= nextEdge.Value.height - LevelRepresentation.PIXEL_LENGTH)
                                {
                                    currentAction = Moves.MORPH_DOWN;
                                }

                                else
                                {
                                    currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                                }
                            }

                            else if (nextEdge.Value.movementType == Graph.movementType.MORPH_UP)
                            {
                                if (rectangleInfo.Height < Math.Min(nextEdge.Value.height + LevelRepresentation.PIXEL_LENGTH, GameInfo.MAX_RECTANGLE_HEIGHT))
                                {
                                    currentAction = Moves.MORPH_UP;
                                }

                                else
                                {
                                    currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                                }
                            }

                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextEdge.Value.movePoint.x, nextEdge.Value.velocityX, nextEdge.Value.rightMove);
                            }

                        }
                        else
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, targetPointX_InAir, 0, true);
                        }
                    }
                }

                // rectangle is not on a platform
                else
                {
                    if (nextEdge.HasValue)
                    {

                        if (nextEdge.Value.movementType == Graph.movementType.STAIR_GAP)
                        {
                            currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if ((nextEdge.Value.movementType == Graph.movementType.MORPH_DOWN ) && rectangleInfo.Height > nextEdge.Value.height)
                        {
                            currentAction = Moves.MORPH_DOWN;
                        }

                        else if ((nextEdge.Value.movementType == Graph.movementType.MORPH_DOWN) && rectangleInfo.Height <= nextEdge.Value.height)
                        {
                            currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if (nextEdge.Value.movementType == Graph.movementType.GAP)
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextEdge.Value.movePoint.x, nextEdge.Value.velocityX, nextEdge.Value.rightMove);
                        }

                        else if (nextEdge.Value.movementType == Graph.movementType.FALL && nextEdge.Value.velocityX == 0 && rectangleInfo.Height <= nextEdge.Value.height)
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextEdge.Value.movePoint.x, nextEdge.Value.velocityX, nextEdge.Value.rightMove);
                        }

                        else if (nextEdge.Value.movementType == Graph.movementType.MORPH_UP)
                        {
                            if (rectangleInfo.Height < Math.Min(nextEdge.Value.height + LevelRepresentation.PIXEL_LENGTH, GameInfo.MAX_RECTANGLE_HEIGHT))
                            {
                                currentAction = Moves.MORPH_UP;
                            }

                            else
                            {
                                currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                            }
                        }

                        else
                        {
                            if (nextEdge.Value.collideCeiling && rectangleInfo.VelocityY < 0)
                            {
                                currentAction = Moves.NO_ACTION;
                            }
                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(rectangleInfo, targetPointX_InAir, 0, true);
                            }
                        }
                    }
                }

                if (!nextEdge.HasValue)
                {
                    currentAction = actionSelector.GetCurrentAction(rectangleInfo, (int)rectangleInfo.X, 0, false);
                }

                lastMoveTime = DateTime.Now;
            }

            if (nextEdge.HasValue)
            {
                if (!actionSelector.IsGoal(rectangleInfo, nextEdge.Value.movePoint.x, nextEdge.Value.velocityX, nextEdge.Value.rightMove))
                {
                    return;
                }

                if (-GameInfo.MAX_VELOCITYY <= rectangleInfo.VelocityY && rectangleInfo.VelocityY <= GameInfo.MAX_VELOCITYY)
                {

                    targetPointX_InAir = (nextEdge.Value.reachablePlatform.leftEdge + nextEdge.Value.reachablePlatform.rightEdge) / 2;

                    if (nextEdge.Value.movementType == Graph.movementType.STAIR_GAP)
                    {
                        currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                    }

                    if (nextEdge.Value.movementType == Graph.movementType.COLLECT ||
                        nextEdge.Value.movementType == Graph.movementType.GAP)
                    {
                        if (rectangleInfo.Height < nextEdge.Value.height)
                        {
                            currentAction = Moves.MORPH_UP;
                        }                      
                    }

                    if ((nextEdge.Value.movementType == Graph.movementType.MORPH_DOWN || nextEdge.Value.movementType == Graph.movementType.FALL) && rectangleInfo.Height > nextEdge.Value.height)
                    {
                        currentAction = Moves.MORPH_DOWN;
                    }

                    if (nextEdge.Value.movementType == Graph.movementType.FALL && nextEdge.Value.velocityX == 0 && rectangleInfo.Height < 190)
                    {
                        currentAction = Moves.MORPH_UP;
                    }

                }
            }
        }

        private bool IsGetCollectible()
        {

            bool[] currentCollectibles = levelInfo.GetObtainedCollectibles();

            if (previousCollectibles.SequenceEqual(currentCollectibles)) {
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
                else if (currentPlatform.Value.id != previousPlatform.Value.id &&currentPlatform.Value.type != Graph.platformType.GAP)
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
            nextEdge = subgoalAStar.CalculateShortestPath(currentPlatform.Value, new LevelRepresentation.Point((int)rectangleInfo.X, (int)rectangleInfo.Y),
                Enumerable.Repeat<bool>(true, levelInfo.initialCollectibles.Length).ToArray(),
                levelInfo.GetObtainedCollectibles(), levelInfo.initialCollectibles);
        }

        //implements abstract rectangle interface: signals the agent the end of the current level
        public override void EndGame(int collectiblesCaught, int timeElapsed)
        {
            Log.LogInformation("RECTANGLE - Collectibles caught = " + collectiblesCaught + ", Time elapsed - " + timeElapsed);
        }

        //implememts abstract agent interface: send messages to the circle agent
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

                if (item.Message.Equals(GameInfo.IST_CIRCLE_PLAYING))
                {
                    this.cooperation = true;
                }

                else if (item.Message.Equals(GameInfo.IST_CIRCLE_GRAPH_COMPLETED))
                {
                    continue;
                }

            }

            return;
        }

        public static List<LevelRepresentation.ArrayPoint> GetFormPixels(LevelRepresentation.Point center, int height)
        {
            LevelRepresentation.ArrayPoint rectangleCenterArray = LevelRepresentation.ConvertPointIntoArrayPoint(center, false, false);

            int rectangleHighestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y - (height / 2), false);
            int rectangleLowestY = LevelRepresentation.ConvertValue_PointIntoArrayPoint(center.y + (height / 2), true);

            float rectangleWidth = GameInfo.RECTANGLE_AREA / height;
            int rectangleLeftX = LevelRepresentation.ConvertValue_PointIntoArrayPoint((int)(center.x - (rectangleWidth / 2)), false);
            int rectangleRightX = LevelRepresentation.ConvertValue_PointIntoArrayPoint((int)(center.x + (rectangleWidth / 2)), true);

            List<LevelRepresentation.ArrayPoint> rectanglePixels = new List<LevelRepresentation.ArrayPoint>();

            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new LevelRepresentation.ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }

        public static bool IsObstacle_onPixels(int[,] levelArray, List<LevelRepresentation.ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (LevelRepresentation.ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == LevelRepresentation.BLACK || levelArray[i.yArray, i.xArray] == LevelRepresentation.YELLOW)
                {
                    return true;
                }
            }

            return false;
        }
    }
}