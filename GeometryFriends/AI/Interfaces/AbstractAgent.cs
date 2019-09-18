using GeometryFriends.AI.ActionSimulation;
using GeometryFriends.AI.Communication;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Perceptions.Information;
using System;
using System.Collections.Generic;

namespace GeometryFriends.AI.Interfaces
{
    public abstract class AbstractAgent
    {
        //flag to load agents
        /// <summary>
        /// Determines if the agent has been implemented or not. (e.g. there might be an implementation for circle but just a dummy implementation for the rectangle)
        /// </summary>
        /// <returns>True if there is a concrete implemetation false if it is a dummy implementation.</returns>
        public abstract bool ImplementedAgent();

        /// <summary>
        /// Setup the agent with all the releventa level related information.
        /// </summary>
        /// <param name="nI">Number of obstacles, rectangle platforms, circle platforms and collectibles.</param>
        /// <param name="sI">Rectangle informations (X, Y, VelocityX, VelocityY, Height)</param>
        /// <param name="cI">Circle information (X, Y, VelocityX, VelocityY, Radius)</param>
        /// <param name="oI">Obstacles and platforms information (X, Y, Height, Width)</param>
        /// <param name="sPI">Rectangle platforms information (X, Y, Height, Width)</param>
        /// <param name="cPI">Circle platforms information (X, Y, Height, Width)</param>
        /// <param name="colI">Collectibles to catch information (X, Y)</param>
        /// <param name="area">The area in which the level occurs.</param>
        /// <param name="timeLimit">The time limit to finish the level.</param>
        [Obsolete("Setup(int[] nI, float[] sI, float[] cI, float[] oI, float[] sPI, float[] cPI, float[] colI, Rectangle area, double timeLimit) is deprecated, please use Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, Rectangle area, double timeLimit) instead.", true)]
        public virtual void Setup(int[] nI, float[] sI, float[] cI, float[] oI, float[] sPI, float[] cPI, float[] colI, System.Drawing.Rectangle area, double timeLimit){}

        /// <summary>
        /// Setup the agent with all the releventa level related information.
        /// </summary>
        /// <param name="nI">Number of obstacles, rectangle platforms, circle platforms and collectibles.</param>
        /// <param name="rI">Rectangle informations (X, Y, VelocityX, VelocityY, Height)</param>
        /// <param name="cI">Circle information (X, Y, VelocityX, VelocityY, Radius)</param>
        /// <param name="oI">Obstacles and platforms information (X, Y, Height, Width)</param>
        /// <param name="rPI">Rectangle platforms information (X, Y, Height, Width)</param>
        /// <param name="cPI">Circle platforms information (X, Y, Height, Width)</param>
        /// <param name="colI">Collectibles to catch information (X, Y)</param>
        /// <param name="area">The area in which the level occurs.</param>
        /// <param name="timeLimit">The time limit to finish the level.</param>
        public abstract void Setup(CountInformation nI, RectangleRepresentation rI, CircleRepresentation cI, ObstacleRepresentation[] oI, ObstacleRepresentation[] rPI, ObstacleRepresentation[] cPI, CollectibleRepresentation[] colI, System.Drawing.Rectangle area, double timeLimit);

        /// <summary>
        /// Get the action to be performed by the agent.
        /// </summary>
        /// <returns>The move representing the action to be performed.</returns>
        public abstract Moves GetAction();
        
        /// <summary>
        /// Update agent logic.
        /// </summary>
        /// <param name="elapsedGameTime">The amount of game time that has elapsed</param>
        public abstract void Update(TimeSpan elapsedGameTime);
        
        /// <summary>
        /// Updates the agent snapshot of the level simulator for predicting actions in the level without actually performing them.
        /// </summary>
        /// <param name="updatedSimulator">The new simulator object containint the most recent snapshot of the level.</param>
        public virtual void ActionSimulatorUpdated(ActionSimulator updatedSimulator){}

        /// <summary>
        /// Gets the name of the agent.
        /// </summary>
        /// <returns>The name of the agent.</returns>
        public abstract string AgentName();

        /// <summary>
        /// Determines if the agent gave up from trying to solve the level.
        /// </summary>
        /// <returns>True if the agent gave up and the level should be ended, false if the agent has not given up.</returns>
        bool given_up = false;

        public virtual bool HasAgentGivenUp()
        {
            return given_up;
        }

        public virtual void GiveUp()
        {
            given_up = true;
        }

        /// <summary>
        /// Signals the agent that the game has ended.
        /// </summary>
        /// <param name="collectiblesCaught">The number of collectibles that have been caught</param>
        /// <param name="timeElapsed">The gametime elapsed in the level.</param>
        public virtual void EndGame(int collectiblesCaught, int timeElapsed){}

        /// <summary>
        /// Get the debug information to be visually represented for debugging purposes only.
        /// </summary>
        public virtual DebugInformation[] GetDebugInformation(){
            return null;
        }

        /// <summary>
        /// Get the communcation messages pending to be transmitted.
        /// </summary>
        /// <returns>The set of messages that the agent wants to transmit.</returns>
        public virtual List<AgentMessage> GetAgentMessages() {
            return new List<AgentMessage>();
        }

        /// <summary>
        /// Handle all the incoming messages pending to be received.
        /// </summary>
        /// <param name="newMessages">The set of incoming messages to be handled.</param>
        public virtual void HandleAgentMessages(List<AgentMessage> newMessages){}

        /// <summary>
        /// Get a screenshot of the current game area.
        /// </summary>
        /// <returns> A reference to the bitmap containing the screenshot. </returns>
        public System.Drawing.Bitmap getScreenshot()
        {
            return XNAStub.Game.GamePlatform.GraphicsDevice.getScreenshot();
        }
    }
}
