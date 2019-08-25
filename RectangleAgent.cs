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

        // Cooperation
        private bool graphChanged;
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

            graphChanged = false;
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
                    if ((IsDifferentPlatform() || IsGetCollectible() || graphChanged) && cooperation == CooperationStatus.SINGLE)
                    {
                        targetPointX_InAir = (currentPlatform.Value.leftEdge + currentPlatform.Value.rightEdge) / 2;
                        Task.Factory.StartNew(SetNextMove);
                        graphChanged = false;
                    }

                    if (nextMove.HasValue)
                    {

                        if ((cooperation == CooperationStatus.SYNCHRONIZED) || (IsCircleInTheWay() && cooperation == CooperationStatus.SINGLE))
                        {
                            currentAction = Moves.NO_ACTION;
                            return;
                        }

                        if (-MAX_VELOCITYY <= rectangleInfo.VelocityY && rectangleInfo.VelocityY <= MAX_VELOCITYY)
                        {

                            if (cooperation == CooperationStatus.RIDE_HELP)
                            {
                                int targetX = nextMove.Value.precondition.right_direction ? (int)circleInfo.X + 50 : (int)circleInfo.X - 50;
                                currentAction = actionSelector.GetCurrentAction(rectangleInfo, targetX, 0, nextMove.Value.precondition.right_direction);
                            }

                            else if (nextMove.Value.type == movementType.GAP && rectangleInfo.Height > Math.Max((RECTANGLE_AREA / nextMove.Value.precondition.height) - 1, MIN_RECTANGLE_HEIGHT + 3))
                            {
                                currentAction = Moves.MORPH_DOWN;
                            }

                            else if ((nextMove.Value.type == movementType.RIDING && Math.Abs(rectangleInfo.X - circleInfo.X) > 60) || cooperation == CooperationStatus.RIDE_HELP)
                            {
                                currentAction = actionSelector.GetCurrentAction(rectangleInfo, (int)circleInfo.X, 0, true);
                            }

                            else if (nextMove.Value.type == movementType.TRANSITION || nextMove.Value.type == movementType.MORPH_DOWN)
                            {
                                if (rectangleInfo.Height >= Math.Max(nextMove.Value.precondition.height, 53))
                                    currentAction = Moves.MORPH_DOWN;

                                else
                                    currentAction = nextMove.Value.precondition.right_direction ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                            }

                            else if (nextMove.Value.type == movementType.MORPH_UP)
                            {
                                if (rectangleInfo.Height < Math.Min(nextMove.Value.precondition.height + PIXEL_LENGTH, 192))
                                {
                                    currentAction = Moves.MORPH_UP;
                                }

                                else
                                {
                                    currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.precondition.position.x, nextMove.Value.precondition.horizontal_velocity, nextMove.Value.precondition.right_direction);
                                }
                            }

                            else
                            {
                                currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.precondition.position.x, nextMove.Value.precondition.horizontal_velocity, nextMove.Value.precondition.right_direction);
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

                        if (cooperation == CooperationStatus.RIDE_HELP)
                        {
                            int targetX = nextMove.Value.precondition.right_direction ? (int)circleInfo.X + 50 : (int)circleInfo.X - 50;
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, targetX, 0, nextMove.Value.precondition.right_direction);
                        }

                        else if (nextMove.Value.type == movementType.TRANSITION)
                        {
                            currentAction = nextMove.Value.precondition.right_direction ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if ((nextMove.Value.type == movementType.MORPH_DOWN ) && rectangleInfo.Height > nextMove.Value.precondition.height)
                        {
                            currentAction = Moves.MORPH_DOWN;
                        }

                        else if ((nextMove.Value.type == movementType.MORPH_DOWN) && rectangleInfo.Height <= nextMove.Value.precondition.height)
                        {
                            currentAction = nextMove.Value.precondition.right_direction ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if (nextMove.Value.type == movementType.GAP)
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.precondition.position.x, nextMove.Value.precondition.horizontal_velocity, nextMove.Value.precondition.right_direction);
                        }

                        else if (nextMove.Value.type == movementType.FALL && nextMove.Value.precondition.horizontal_velocity == 0 && rectangleInfo.Height <= nextMove.Value.precondition.height)
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextMove.Value.precondition.position.x, nextMove.Value.precondition.horizontal_velocity, nextMove.Value.precondition.right_direction);
                        }

                        else if (nextMove.Value.type == movementType.MORPH_UP)
                        {
                            if (rectangleInfo.Height < Math.Min(nextMove.Value.precondition.height + PIXEL_LENGTH, MAX_RECTANGLE_HEIGHT))
                            {
                                currentAction = Moves.MORPH_UP;
                            }

                            else
                            {
                                currentAction = nextMove.Value.precondition.right_direction ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
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

                if (actionSelector.IsGoal(rectangleInfo, nextMove.Value.precondition) && Math.Abs(rectangleInfo.VelocityY) <= MAX_VELOCITYY)
                {

                    targetPointX_InAir = (nextMove.Value.reachablePlatform.leftEdge + nextMove.Value.reachablePlatform.rightEdge) / 2;

                    if (cooperation == CooperationStatus.RIDE)
                    {
                        if (rectangleInfo.Height > nextMove.Value.precondition.height + 4)
                        {
                            currentAction = Moves.MORPH_DOWN;
                        }

                        else if (rectangleInfo.Height < nextMove.Value.precondition.height - 4)
                        {
                            currentAction = Moves.MORPH_UP;
                        }
                        else
                        {
                            cooperation = CooperationStatus.SYNCHRONIZED;
                        }

                    }

                    if (cooperation == CooperationStatus.RIDE_HELP)
                    {
                        return;
                    }

                    if (nextMove.Value.type == movementType.TRANSITION)
                    {
                        currentAction = nextMove.Value.precondition.right_direction ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                    }

                    if ((nextMove.Value.type == movementType.MORPH_UP || nextMove.Value.type == movementType.COLLECT || nextMove.Value.type == movementType.GAP) &&
                        rectangleInfo.Height < Math.Min(nextMove.Value.precondition.height + PIXEL_LENGTH, 192))
                    {
                            currentAction = Moves.MORPH_UP;            
                    }

                    if (nextMove.Value.type == movementType.RIDE && rectangleInfo.Height > Math.Max(nextMove.Value.precondition.height, 53))
                    {
                         currentAction = Moves.MORPH_DOWN;
                    }

                    if ((nextMove.Value.type == movementType.MORPH_DOWN || nextMove.Value.type == movementType.FALL) && rectangleInfo.Height > nextMove.Value.precondition.height)
                    {
                        currentAction = Moves.MORPH_DOWN;
                    }

                    if (nextMove.Value.type == movementType.FALL && nextMove.Value.precondition.horizontal_velocity == 0 && rectangleInfo.Height < 190)
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

                            foreach (Platform p in graph.platforms)
                            {

                                foreach (Move m in p.moves)
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
                levelInfo.GetObtainedCollectibles(), levelInfo.initialCollectibles);

            if (nextMove.HasValue && nextMove.Value.type == movementType.RIDE && cooperation == CooperationStatus.SINGLE)
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

                if (item.Message.Equals(INDIVIDUAL_MOVE) && item.Attachment.GetType() == typeof(Move))
                {
                    Move circleMove = (Move)item.Attachment;

                    foreach (Platform p in graph.platforms)
                    {
                        foreach (Move m in p.moves)
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

                    cooperation = CooperationStatus.SINGLE;
                }

                if (item.Message.Equals(UNSYNCHRONIZED) && item.Attachment.GetType() == typeof(Move))
                {
                    Move circleMove = (Move) item.Attachment;
                    PreCondition rectangle_precondition = circleMove.precondition;

                    bool[] collectibles_onPath = Utilities.GetXorMatrix(graph.possibleCollectibles, circleMove.collectibles_onPath);
                    Platform? fromPlatform = graph.GetPlatform(rectangle_precondition.position, rectangle_precondition.height);

                    if (fromPlatform.HasValue)
                    {
                        Move newMove = new Move((Platform)fromPlatform, rectangle_precondition, rectangle_precondition.position, circleMove.type, collectibles_onPath, 0, circleMove.collideCeiling);
                        graph.AddMove((Platform)fromPlatform, newMove);
                        graphChanged = true;
                    }

                }

                if (item.Message.Equals(RIDE) && item.Attachment.GetType() == typeof(Move))
                {
                    cooperation = CooperationStatus.RIDE;

                    CleanRides();

                    Move newMove = (Move)item.Attachment;
                    newMove.type = movementType.RIDING;
                    nextMove = newMove;

                }

                if (item.Message.Equals(COOPERATION_FINISHED))
                {
                    cooperation = CooperationStatus.SINGLE;
                    CleanRides();
                }

                if (item.Message.Equals(JUMPED) && nextMove.HasValue)
                {
                    Move newMove = CopyMove((Move)nextMove);
                    newMove.type = movementType.MORPH_DOWN;
                    newMove.precondition.height = MIN_RECTANGLE_HEIGHT;
                    nextMove = newMove;
                    cooperation = CooperationStatus.RIDE;
                }

                if (item.Message.Equals(RIDE_HELP))
                {
                    cooperation = CooperationStatus.RIDE_HELP;
                }

            }

            return;
        }

        public static List<ArrayPoint> GetFormPixels(LevelRepresentation.Point center, int height)
        {
            ArrayPoint rectangleCenterArray = ConvertPointIntoArrayPoint(center, false, false);

            int rectangleHighestY = ConvertValue_PointIntoArrayPoint(center.y - (height / 2), false);
            int rectangleLowestY = ConvertValue_PointIntoArrayPoint(center.y + (height / 2), true);

            float rectangleWidth = RECTANGLE_AREA / height;
            int rectangleLeftX = ConvertValue_PointIntoArrayPoint((int)(center.x - (rectangleWidth / 2)), false);
            int rectangleRightX = ConvertValue_PointIntoArrayPoint((int)(center.x + (rectangleWidth / 2)), true);

            List<ArrayPoint> rectanglePixels = new List<ArrayPoint>();

            for (int i = rectangleHighestY; i <= rectangleLowestY; i++)
            {
                for (int j = rectangleLeftX; j <= rectangleRightX; j++)
                {
                    rectanglePixels.Add(new ArrayPoint(j, i));
                }
            }

            return rectanglePixels;
        }

        public static bool IsObstacle_onPixels(int[,] levelArray, List<ArrayPoint> checkPixels)
        {
            if (checkPixels.Count == 0)
            {
                return true;
            }

            foreach (ArrayPoint i in checkPixels)
            {
                if (levelArray[i.yArray, i.xArray] == BLACK || levelArray[i.yArray, i.xArray] == YELLOW)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanMorphUp()
        {

            List<ArrayPoint> pixelsToMorph = new List<ArrayPoint>();

            int lowestY = ConvertValue_PointIntoArrayPoint((int) rectangleInfo.Y + ((int)rectangleInfo.Height / 2), false);
            int highestY = ConvertValue_PointIntoArrayPoint((int)rectangleInfo.Y - (200 - ((int)rectangleInfo.Height / 2)), false);

            int rectangleWidth = RECTANGLE_AREA / (int)rectangleInfo.Height;

            int lowestLeft = ConvertValue_PointIntoArrayPoint((int)rectangleInfo.X - (rectangleWidth / 2), false);
            int highestLeft = ConvertValue_PointIntoArrayPoint((int)rectangleInfo.X - (MIN_RECTANGLE_HEIGHT / 2), false);

            int lowestRight = ConvertValue_PointIntoArrayPoint((int)rectangleInfo.X + (MIN_RECTANGLE_HEIGHT / 2), false);
            int highestRight = ConvertValue_PointIntoArrayPoint((int)rectangleInfo.X + (rectangleWidth / 2), false);

            for (int y = highestY; y <= lowestY; y++)
            {
                for (int x = lowestLeft; x <= highestLeft; x++)
                {
                    pixelsToMorph.Add(new ArrayPoint(x, y));
                }

                for (int x = lowestRight; x <= highestRight; x++)
                {
                    pixelsToMorph.Add(new ArrayPoint(x, y));
                }

            }

            return !IsObstacle_onPixels(levelInfo.levelArray, pixelsToMorph);
        }

        public bool IsCircleInTheWay()
        {

            if (nextMove.HasValue &&
                circleInfo.Y <= rectangleInfo.Y + (rectangleInfo.Height/2) &&
                circleInfo.Y >= rectangleInfo.Y - (rectangleInfo.Height/2))
            {

                int rectangleWidth = RECTANGLE_AREA / (int)rectangleInfo.Height;

                if (nextMove.Value.precondition.right_direction &&
                    circleInfo.X >= rectangleInfo.X &&
                    circleInfo.X <= nextMove.Value.precondition.position.x + rectangleWidth + CIRCLE_RADIUS)
                {
                    return true;
                }

                if (!nextMove.Value.precondition.right_direction &&
                    circleInfo.X <= rectangleInfo.X &&
                    circleInfo.X >= nextMove.Value.precondition.position.x - rectangleWidth - CIRCLE_RADIUS)
                {
                    return true;
                }

            }

            return false;
        }

        public void CleanRides()
        {

            if (nextMove.HasValue && (nextMove.Value.type == movementType.RIDE || nextMove.Value.type == movementType.RIDING))
            {
                nextMove = null;
            }

            foreach (Platform p in graph.platforms)
            {
                int i = 0;

                while (i < p.moves.Count)
                {
                    if (p.moves[i].type == movementType.RIDE || p.moves[i].type == movementType.RIDING)
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