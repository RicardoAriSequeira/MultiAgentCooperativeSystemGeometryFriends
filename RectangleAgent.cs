using GeometryFriends;
using GeometryFriends.AI;
using GeometryFriends.AI.ActionSimulation;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Interfaces;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GeometryFriendsAgents
{
    /// <summary>
    /// A rectangle agent implementation for the GeometryFriends game that demonstrates simple random action selection.
    /// </summary>
    public class RectangleAgent : AbstractRectangleAgent
    {
        //agent implementation specificiation
        private bool implementedAgent;
        private string agentName = "IST Rectangle";

        //auxiliary variables for agent action
        private Moves currentAction;
        private List<Moves> possibleMoves;
        private DateTime lastMoveTime;
        private Random rnd;

        private PlatformsRectangle platform;
        private LevelArray levelArray;
        private int targetPointX_InAir;
        private SubgoalAStar subgoalAStar;
        private bool[] previousCollectibles;
        private Platforms.Move? nextEdge;
        private ActionSelector actionSelector;
        private ActionSimulator predictor;
        //private DebugInformation[] debugInfo = null;
        private Platforms.Platform? currentPlatform;
        private Platforms.Platform? previousPlatform;

        //Sensors Information
        private CountInformation numbersInfo;
        private RectangleRepresentation rectangleInfo;
        private CircleRepresentation circleInfo;
        private ObstacleRepresentation[] blackObstacles;
        private ObstacleRepresentation[] greenObstacles;
        private ObstacleRepresentation[] yellowObstacles;
        private CollectibleRepresentation[] collectibles;

        private List<AgentMessage> messages;

        //Area of the game screen
        protected Rectangle area;

        public RectangleAgent()
        {
            //Change flag if agent is not to be used
            implementedAgent = true;

            lastMoveTime = DateTime.Now;
            currentAction = Moves.NO_ACTION;
            rnd = new Random();

            levelArray = new LevelArray();
            platform = new PlatformsRectangle();
            subgoalAStar = new SubgoalAStar();
            actionSelector = new ActionSelector();

            //prepare the possible moves  
            possibleMoves = new List<Moves>();
            possibleMoves.Add(Moves.MOVE_LEFT);
            possibleMoves.Add(Moves.MOVE_RIGHT);
            possibleMoves.Add(Moves.MORPH_UP);
            possibleMoves.Add(Moves.MORPH_DOWN);

            previousPlatform = null;
            currentPlatform = null;

            //messages exchange
            messages = new List<AgentMessage>();
        }

        //implements abstract rectangle interface: used to setup the initial information so that the agent has basic knowledge about the level
        public override void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit)
        {
            numbersInfo = nI;
            rectangleInfo = rI;
            circleInfo = cI;
            blackObstacles = oI;
            greenObstacles = rPI;
            yellowObstacles = cPI;
            collectibles = colI;
            this.area = area;

            //send a message to the rectangle informing that the circle setup is complete and show how to pass an attachment: a pen object
            //messages.Add(new AgentMessage("Setup complete, testing to send an object as an attachment.", new Pen(Color.BlanchedAlmond)));

            nextEdge = null;
            targetPointX_InAir = (int)circleInfo.X;

            levelArray.CreateLevelArray(collectibles, blackObstacles, greenObstacles, yellowObstacles);
            platform.SetUp(levelArray.GetLevelArray(), levelArray.initialCollectiblesInfo.Length);

            previousCollectibles = levelArray.GetObtainedCollectibles(colI);

            //DebugSensorsInfo();
        }

        //implements abstract rectangle interface: registers updates from the agent's sensors that it is up to date with the latest environment information
        public override void SensorsUpdated(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI)
        {
            rectangleInfo = rI;
            circleInfo = cI;
            collectibles = colI;
        }

        public override void ActionSimulatorUpdated(ActionSimulator updatedSimulator)
        {
            predictor = updatedSimulator;
        }

        //implements abstract rectangle interface: signals if the agent is actually implemented or not
        public override bool ImplementedAgent()
        {
            return implementedAgent;
        }

        //implements abstract rectangle interface: provides the name of the agent to the agents manager in GeometryFriends
        public override string AgentName()
        {
            return agentName;
        }

        //simple algorithm for choosing a random action for the rectangle agent
        private void RandomAction()
        {
            /*
             Rectangle Actions
             MOVE_LEFT = 5
             MOVE_RIGHT = 6
             MORPH_UP = 7
             MORPH_DOWN = 8
            */

            currentAction = possibleMoves[rnd.Next(possibleMoves.Count)];

            //send a message to the circle agent telling what action it chose
            messages.Add(new AgentMessage("Going to :" + currentAction));
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

                currentPlatform = platform.GetPlatform(new LevelArray.Point((int)rectangleInfo.X, (int)rectangleInfo.Y), rectangleInfo.Height);

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

                            if (nextEdge.Value.movementType == Platforms.movementType.STAIR_GAP || nextEdge.Value.movementType == Platforms.movementType.MORPH_DOWN)
                            {
                                if (rectangleInfo.Height >= nextEdge.Value.height - LevelArray.PIXEL_LENGTH)
                                {
                                    currentAction = Moves.MORPH_DOWN;
                                }

                                else
                                {
                                    currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                                }
                            }

                            else if (nextEdge.Value.movementType == Platforms.movementType.MORPH_UP)
                            {
                                if (rectangleInfo.Height <= nextEdge.Value.height)
                                {
                                    currentAction = Moves.MORPH_UP;
                                }

                                else
                                {
                                    currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                                }
                            }

                            else if (nextEdge.Value.movementType == Platforms.movementType.FALL && nextEdge.Value.velocityX == 0 && rectangleInfo.Height >= 55)
                            {
                                currentAction = Moves.MORPH_DOWN;
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

                        if (nextEdge.Value.movementType == Platforms.movementType.STAIR_GAP)
                        {
                            currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if ((nextEdge.Value.movementType == Platforms.movementType.MORPH_DOWN ) && rectangleInfo.Height > nextEdge.Value.height)
                        {
                            currentAction = Moves.MORPH_DOWN;
                        }

                        else if ((nextEdge.Value.movementType == Platforms.movementType.MORPH_DOWN) && rectangleInfo.Height <= nextEdge.Value.height)
                        {
                            currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                        }

                        else if (nextEdge.Value.movementType == Platforms.movementType.FALL && nextEdge.Value.velocityX == 0 && rectangleInfo.Height <= 55)
                        {
                            currentAction = actionSelector.GetCurrentAction(rectangleInfo, nextEdge.Value.movePoint.x, nextEdge.Value.velocityX, nextEdge.Value.rightMove);
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
                //DebugSensorsInfo();
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

                    if (nextEdge.Value.movementType == Platforms.movementType.STAIR_GAP)
                    {
                        currentAction = nextEdge.Value.rightMove ? Moves.MOVE_RIGHT : Moves.MOVE_LEFT;
                    }

                    if (nextEdge.Value.movementType == Platforms.movementType.COLLECT)
                    {
                        if (rectangleInfo.Height < nextEdge.Value.height)
                        {
                            currentAction = Moves.MORPH_UP;
                        }                      
                    }

                    if ((nextEdge.Value.movementType == Platforms.movementType.MORPH_DOWN || nextEdge.Value.movementType == Platforms.movementType.FALL) && rectangleInfo.Height > nextEdge.Value.height)
                    {
                        currentAction = Moves.MORPH_DOWN;
                    }

                    if (nextEdge.Value.movementType == Platforms.movementType.FALL && nextEdge.Value.velocityX == 0 && rectangleInfo.Height < 190)
                    {
                        currentAction = Moves.MORPH_UP;
                    }

                }
            }
        }

        private bool IsGetCollectible()
        {

            bool[] currentCollectibles = levelArray.GetObtainedCollectibles(collectibles);

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

                            foreach (Platforms.Platform p in platform.platformList)
                            {

                                foreach (Platforms.Move m in p.moves)
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
            nextEdge = subgoalAStar.CalculateShortestPath(currentPlatform.Value, new LevelArray.Point((int)rectangleInfo.X, (int)rectangleInfo.Y),
                Enumerable.Repeat<bool>(true, levelArray.initialCollectiblesInfo.Length).ToArray(),
                levelArray.GetObtainedCollectibles(collectibles), levelArray.initialCollectiblesInfo);
        }

        //typically used console debugging used in previous implementations of GeometryFriends
        protected void DebugSensorsInfo()
        {
            Log.LogInformation("Rectangle Aagent - " + numbersInfo.ToString());

            Log.LogInformation("Rectangle Aagent - " + rectangleInfo.ToString());

            Log.LogInformation("Rectangle Aagent - " + circleInfo.ToString());

            foreach (ObstacleRepresentation i in blackObstacles)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString("Obstacle"));
            }

            foreach (ObstacleRepresentation i in greenObstacles)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString("Rectangle Platform"));
            }

            foreach (ObstacleRepresentation i in yellowObstacles)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString("Circle Platform"));
            }

            foreach (CollectibleRepresentation i in collectibles)
            {
                Log.LogInformation("Rectangle Aagent - " + i.ToString());
            }
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
                Log.LogInformation("Rectangle: received message from circle: " + item.Message);
                if (item.Attachment != null)
                {
                    Log.LogInformation("Received message has attachment: " + item.Attachment.ToString());
                    if (item.Attachment.GetType() == typeof(Pen))
                    {
                        Log.LogInformation("The attachment is a pen, let's see its color: " + ((Pen)item.Attachment).Color.ToString());
                    }
                }
            }
        }
    }
}