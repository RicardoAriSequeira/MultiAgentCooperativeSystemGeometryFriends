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
        private bool training = false;
        private ReinforcementLearning RL;
        private ActionSelector actionSelector;
        private long timeRL;

        // Messages
        private GameInfo.CooperationStatus cooperation;
        private List<AgentMessage> messages;

        // Auxiliary
        private Moves currentAction;
        private Graph.Move? nextMove;
        private DateTime lastMoveTime;
        private int targetPointX_InAir;
        private bool[] previousCollectibles;
        private Graph.Platform? previousPlatform, currentPlatform;

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
            cooperation = GameInfo.CooperationStatus.SINGLE_MODE;

            messages = new List<AgentMessage>();

            //ReinforcementLearning.WriteQTable(new float[ReinforcementLearning.N_STATES, ReinforcementLearning.N_ACTIONS]);

            if (training)
            {
                RL = new ReinforcementLearning();
            }
        }

        //implements abstract rectangle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            messages.Add(new AgentMessage(GameInfo.IST_RECTANGLE_PLAYING));

            // Create Level Array
            levelInfo.CreateLevelArray(colI, oI, rPI, cPI, LevelRepresentation.YELLOW);

            // Create Graph
            graph.SetupGraph(levelInfo.GetLevelArray(), colI.Length);
            graph.SetPossibleCollectibles(rI);

            // Initial Information
            circleInfo = cI;
            rectangleInfo = rI;
            targetPointX_InAir = (int)rectangleInfo.X;
            previousCollectibles = levelInfo.GetObtainedCollectibles();

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
            return training ? RL.GetAction(rectangleInfo) : currentAction;
            //return training ? RL.GetBestAction(rectangleInfo) : currentAction;
            //return currentAction;   
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

                currentPlatform = graph.GetPlatform(new LevelRepresentation.Point((int)rectangleInfo.X, (int)rectangleInfo.Y), rectangleInfo.Height);

                if (currentPlatform.HasValue)
                {
                    if (IsDifferentPlatform() || IsGetCollectible() || cooperation == GameInfo.CooperationStatus.NOT_COOPERATING)
                    {
                        targetPointX_InAir = (currentPlatform.Value.leftEdge + currentPlatform.Value.rightEdge) / 2;
                        Task.Factory.StartNew(SetNextMove);

                        if (cooperation == GameInfo.CooperationStatus.NOT_COOPERATING)
                            cooperation = GameInfo.CooperationStatus.SINGLE_MODE;

                    }

                    if (nextMove.HasValue)
                    {

                        if (nextMove.Value.type == Graph.movementType.RIDE && cooperation == GameInfo.CooperationStatus.SINGLE_MODE)
                        {
                            cooperation = GameInfo.CooperationStatus.AWAITING_RIDE;
                        }

                        if (-GameInfo.MAX_VELOCITYY <= rectangleInfo.VelocityY && rectangleInfo.VelocityY <= GameInfo.MAX_VELOCITYY)
                        {

                            if (nextMove.Value.type == Graph.movementType.GAP &&
                                rectangleInfo.Height > Math.Max((GameInfo.RECTANGLE_AREA / nextMove.Value.height) - 1, GameInfo.MIN_RECTANGLE_HEIGHT + 3))
                            {
                                currentAction = Moves.MORPH_DOWN;
                            }

                            else if (nextMove.Value.type == Graph.movementType.RIDING && Math.Abs(rectangleInfo.X - circleInfo.X) > 60)
                            {
                                currentAction = actionSelector.GetCurrentAction(circleInfo, (int)circleInfo.X, 0, true);
                                //currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.movePoint.x, nextMove.Value.velocityX, nextMove.Value.rightMove);
                            }

                            else if (nextMove.Value.type == Graph.movementType.STAIR_GAP ||
                                nextMove.Value.type == Graph.movementType.MORPH_DOWN)
                            {
                                if (rectangleInfo.Height >= nextMove.Value.height - LevelRepresentation.PIXEL_LENGTH)
                                {
                                    currentAction = Moves.MORPH_DOWN;
                                }

                                else
                                {
                                    currentAction = nextMove.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                                }
                            }

                            else if (nextMove.Value.type == Graph.movementType.MORPH_UP)
                            {
                                if (rectangleInfo.Height < Math.Min(nextMove.Value.height + LevelRepresentation.PIXEL_LENGTH, 192))
                                {
                                    currentAction = Moves.MORPH_UP;
                                }

                                else
                                {
                                    currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.movePoint.x, nextMove.Value.velocityX, nextMove.Value.rightMove);
                                    //currentAction = nextMove.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                                }
                            }

                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.movePoint.x, nextMove.Value.velocityX, nextMove.Value.rightMove);
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
                    if (nextMove.HasValue)
                    {

                        if (nextMove.Value.type == Graph.movementType.STAIR_GAP)
                        {
                            currentAction = nextMove.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if ((nextMove.Value.type == Graph.movementType.MORPH_DOWN ) && rectangleInfo.Height > nextMove.Value.height)
                        {
                            currentAction = Moves.MORPH_DOWN;
                        }

                        else if ((nextMove.Value.type == Graph.movementType.MORPH_DOWN) && rectangleInfo.Height <= nextMove.Value.height)
                        {
                            currentAction = nextMove.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if (nextMove.Value.type == Graph.movementType.GAP)
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.movePoint.x, nextMove.Value.velocityX, nextMove.Value.rightMove);
                        }

                        else if (nextMove.Value.type == Graph.movementType.FALL && nextMove.Value.velocityX == 0 && rectangleInfo.Height <= nextMove.Value.height)
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.movePoint.x, nextMove.Value.velocityX, nextMove.Value.rightMove);
                        }

                        else if (nextMove.Value.type == Graph.movementType.MORPH_UP)
                        {
                            if (rectangleInfo.Height < Math.Min(nextMove.Value.height + LevelRepresentation.PIXEL_LENGTH, GameInfo.MAX_RECTANGLE_HEIGHT))
                            {
                                currentAction = Moves.MORPH_UP;
                            }

                            else
                            {
                                currentAction = nextMove.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                            }
                        }

                        else
                        {
                            if (nextMove.Value.collideCeiling && rectangleInfo.VelocityY < 0)
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

                if (!nextMove.HasValue)
                {
                    currentAction = actionSelector.GetCurrentAction(rectangleInfo, (int)rectangleInfo.X, 0, false);
                }

                lastMoveTime = DateTime.Now;
            }

            if (nextMove.HasValue)
            {

                if (!actionSelector.IsGoal(rectangleInfo, nextMove.Value.movePoint.x, nextMove.Value.velocityX, nextMove.Value.rightMove))
                {
                    return;
                }

                if (-GameInfo.MAX_VELOCITYY <= rectangleInfo.VelocityY && rectangleInfo.VelocityY <= GameInfo.MAX_VELOCITYY)
                {

                    targetPointX_InAir = (nextMove.Value.reachablePlatform.leftEdge + nextMove.Value.reachablePlatform.rightEdge) / 2;

                    if (cooperation == GameInfo.CooperationStatus.RIDING)
                    {
                        messages.Add(new AgentMessage(GameInfo.IN_POSITION));
                    }

                    if (nextMove.Value.type == Graph.movementType.STAIR_GAP)
                    {
                        currentAction = nextMove.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                    }

                    if (nextMove.Value.type == Graph.movementType.MORPH_UP ||
                        nextMove.Value.type == Graph.movementType.COLLECT ||
                        nextMove.Value.type == Graph.movementType.GAP)
                    {
                        if (rectangleInfo.Height < Math.Min(nextMove.Value.height + LevelRepresentation.PIXEL_LENGTH, 192))
                        {
                            currentAction = Moves.MORPH_UP;
                        }

                        else if (cooperation == GameInfo.CooperationStatus.AWAITING_MORPH)
                        {
                            cooperation = GameInfo.CooperationStatus.MORPH_READY;
                            messages.Add(new AgentMessage(GameInfo.MORPH_READY));
                        }
                            
                    }

                    if ((nextMove.Value.type == Graph.movementType.MORPH_DOWN ||
                         nextMove.Value.type == Graph.movementType.FALL ||
                         nextMove.Value.type == Graph.movementType.RIDE) &&
                        rectangleInfo.Height > nextMove.Value.height)
                    {
                        currentAction = Moves.MORPH_DOWN;
                    }

                    if (nextMove.Value.type == Graph.movementType.FALL && nextMove.Value.velocityX == 0 && rectangleInfo.Height < 190)
                    {
                        currentAction = Moves.MORPH_UP;
                    }

                    if (nextMove.HasValue && (nextMove.Value.type == Graph.movementType.RIDE) &&
                        cooperation == GameInfo.CooperationStatus.AWAITING_RIDE &&
                        Math.Abs(nextMove.Value.height - rectangleInfo.Height) < 5)
                    {
                        cooperation = GameInfo.CooperationStatus.RIDE_READY;
                        messages.Add(new AgentMessage(GameInfo.RIDE_READY));
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

        private void SetNextMove()
        {
            nextMove = null;
            nextMove = subgoalAStar.CalculateShortestPath(currentPlatform.Value, new LevelRepresentation.Point((int)rectangleInfo.X, (int)rectangleInfo.Y),
                Enumerable.Repeat<bool>(true, levelInfo.initialCollectibles.Length).ToArray(),
                levelInfo.GetObtainedCollectibles(), levelInfo.initialCollectibles);        }

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

                if (item.Message.Equals(GameInfo.INDIVIDUAL_MOVE) &&
                        item.Attachment.GetType() == typeof(Graph.Move))
                {
                    Graph.Move circleMove = (Graph.Move)item.Attachment;

                    foreach (Graph.Platform p in graph.platforms)
                    {
                        foreach (Graph.Move m in p.moves)
                        {
                            for (int i = 0; i < m.collectibles_onPath.Length; i++)
                            {
                                if (circleMove.collectibles_onPath[i] && m.collectibles_onPath[i])
                                {
                                    m.collectibles_onPath[i] = false;
                                }
                            }
                        }
                    }
                }

                if (item.Message.Equals(GameInfo.AWAITING_MORPH) &&
                        item.Attachment.GetType() == typeof(Graph.Move))
                {
                    Graph.Move newMove = Graph.CopyMove((Graph.Move)item.Attachment);
                    newMove.type = Graph.movementType.MORPH_UP;
                    newMove.velocityX = 0;
                    nextMove = newMove;
                    cooperation = GameInfo.CooperationStatus.AWAITING_MORPH;
                }

                if (item.Message.Equals(GameInfo.AWAITING_RIDE) &&
                        item.Attachment.GetType() == typeof(Graph.Move))
                {
                    Graph.Move circleMove = (Graph.Move) item.Attachment;

                    bool[] collectibles_onPath = Utilities.GetXorMatrix(graph.possibleCollectibles, circleMove.collectibles_onPath);
                    int movePoint_x = Math.Min(Math.Max(circleMove.landPoint.x, 136), 1064);
                    LevelRepresentation.Point movePoint = new LevelRepresentation.Point(movePoint_x, circleMove.landPoint.y + GameInfo.CIRCLE_RADIUS + (GameInfo.MIN_RECTANGLE_HEIGHT/2));
                    Graph.Platform? fromPlatform = graph.GetPlatform(movePoint, GameInfo.MIN_RECTANGLE_HEIGHT);

                    if (fromPlatform.HasValue)
                    {
                        Graph.Move newMove = new Graph.Move((Graph.Platform)fromPlatform, movePoint, movePoint, 0, true, circleMove.type, collectibles_onPath, 0, false, GameInfo.MIN_RECTANGLE_HEIGHT);
                        graph.AddMove((Graph.Platform)fromPlatform, newMove);
                    }

                    cooperation = GameInfo.CooperationStatus.NOT_COOPERATING;

                }

                if (item.Message.Equals(GameInfo.RIDING) &&
                        item.Attachment.GetType() == typeof(Graph.Move))
                {
                    foreach (Graph.Platform p in graph.platforms)
                    {
                        int i = 0;

                        while (i < p.moves.Count)
                        {
                            if (p.moves[i].type == Graph.movementType.RIDE)
                            {
                                p.moves.Remove(p.moves[i]);
                            }
                            else
                            {
                                i++;
                            }

                        }
                    }

                    cooperation = GameInfo.CooperationStatus.RIDING;
                    Graph.Move newMove = (Graph.Move)item.Attachment;
                    newMove.type = Graph.movementType.RIDING;
                    newMove.velocityX = 0;
                    nextMove = newMove;
                }

                if (item.Message.Equals(GameInfo.COOPERATION_FINISHED))
                {
                    cooperation = GameInfo.CooperationStatus.NOT_COOPERATING;
                }

                if (item.Message.Equals(GameInfo.JUMPED) && nextMove.HasValue)
                {
                    Graph.Move newMove = Graph.CopyMove((Graph.Move)nextMove);
                    newMove.type = Graph.movementType.MORPH_DOWN;
                    newMove.height = GameInfo.MIN_RECTANGLE_HEIGHT;
                    nextMove = newMove;
                    cooperation = GameInfo.CooperationStatus.RIDING;
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