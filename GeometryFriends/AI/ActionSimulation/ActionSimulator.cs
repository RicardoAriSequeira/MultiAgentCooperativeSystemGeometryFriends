using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.AI.Debug;
using GeometryFriends.AI.Perceptions.Information;
using GeometryFriends.Levels.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace GeometryFriends.AI.ActionSimulation
{
    public class ActionSimulator
    {
        /// <summary>
        /// The possible types of debug information to be generated while simulating.
        /// </summary>
        public enum DebugInfoMode
        {
            Circle,
            Rectangle,
            Both
        }

        /// <summary>
        /// Delegate for handling the catching of collectibles in the simulator.
        /// </summary>
        public delegate void CollectibleCaughtSimulatorHandler(Object sender, CollectibleRepresentation collectibleCaught);
        /// <summary>
        /// Event to trigger that a collectible has been caught.
        /// </summary>
        public event CollectibleCaughtSimulatorHandler SimulatorCollectedEvent;

        /// <summary>
        /// The physics simulation instance that enables to predict what happens in the level at a given time in the future.
        /// </summary>
        protected PhysicsSimulator Simulator { get; set; }

        /// <summary>
        /// The circle character that helps maitain the same behavior in the simulation as in the level simulation.
        /// </summary>
        internal CircleCharacter AssociatedCircleCharacter { get; set; }
        
        /// <summary>
        /// The rectangle character that helps maitain the same behavior in the simulation as in the level simulation.
        /// </summary>
        internal RectangleCharacter AssociatedRectangleCharacter { get; set; }

        /// <summary>
        /// Physics simulator that retains the initial state of the simulator instance in case a reset is required.
        /// </summary>
        protected PhysicsSimulator SimulatorReset { get; set; }

        /// <summary>
        /// Circle character that retains the its state at the time of the simulator instancing so that the simulator can be reset.
        /// </summary>
        internal CircleCharacter AssociatedCircleCharacterReset { get; set; }

        /// <summary>
        /// Rectangle character that retains the its state at the time of the simulator instancing so that the simulator can be reset.
        /// </summary>
        internal RectangleCharacter AssociatedRectangleCharacterReset { get; set; }

        /// <summary>
        /// The list of collectibles caught in the latest simulation that should be removed from the simulator right after the simulation.
        /// </summary>
        protected List<Geom> CollectiblesToRemove { get; set; }

        /// <summary>
        /// The collectibles that are still uncaught in this action simulator instance.
        /// </summary>
        public List<CollectibleRepresentation> CollectiblesUncaught { get; set; }

        /// <summary>
        /// The collectibles that have been caught in this simulator instance. Does not include collectibles that might have been caught previously.
        /// </summary>
        public List<CollectibleRepresentation> CollectiblesCaught { get; set; }

        /// <summary>
        /// The number of collectibles that remain uncaught.
        /// </summary>
        public int CollectiblesUncaughtCount { 
            get 
            {
                return CollectiblesUncaught.Count;
            } 
        }

        /// <summary>
        /// The number of collectibles that have been caught (excluding thos that had already been caught before the simulator instance was created).
        /// </summary>
        public int CollectiblesCaughtCount
        {
            get
            {
                return CollectiblesCaught.Count;
            }
        }

        /// <summary>
        /// The current circle character position in the X axis.
        /// </summary>
        public float CirclePositionX { 
            get{
                return AssociatedCircleCharacter.Body.Position.X;
            } 
        }

        /// <summary>
        /// The current circle character position in the Y axis.
        /// </summary>
        public float CirclePositionY
        {
            get
            {
                return AssociatedCircleCharacter.Body.Position.Y;
            }
        }

        /// <summary>
        /// The current circle character velocity in the X axis.
        /// </summary>
        public float CircleVelocityX
        {
            get
            {
                return AssociatedCircleCharacter.Body.LinearVelocityX;
            }
        }

        /// <summary>
        /// The current circle character velocity in the Y axis.
        /// </summary>
        public float CircleVelocityY
        {
            get
            {
                return AssociatedCircleCharacter.Body.LinearVelocityY;
            }
        }

        /// <summary>
        /// The current circle character radius.
        /// </summary>
        public int CircleVelocityRadius
        {
            get
            {
                return AssociatedCircleCharacter.Radius;
            }
        }

        /// <summary>
        /// The current rectangle character position in the X axis.
        /// </summary>
        public float RectanglePositionX
        {
            get
            {
                return AssociatedRectangleCharacter.Body.Position.X;
            }
        }

        /// <summary>
        /// The current rectangle character position in the Y axis.
        /// </summary>
        public float RectanglePositionY
        {
            get
            {
                return AssociatedRectangleCharacter.Body.Position.Y;
            }
        }

        /// <summary>
        /// The current rectangle character velocity in the X axis.
        /// </summary>
        public float RectangleVelocityX
        {
            get
            {
                return AssociatedRectangleCharacter.Body.LinearVelocityX;
            }
        }

        /// <summary>
        /// The current rectangle character velocity in the Y axis.
        /// </summary>
        public float RectangleVelocityY
        {
            get
            {
                return AssociatedRectangleCharacter.Body.LinearVelocityY;
            }
        }

        /// <summary>
        /// The current rectangle character height.
        /// </summary>
        public float RectangleHeight
        {
            get
            {
                return AssociatedRectangleCharacter.Height;
            }
        }

        /// <summary>
        /// The amount of time that has been simulated in this action simulator instance.
        /// </summary>
        public float SimulatedTime { get; protected set; }

        /// <summary>
        /// The simulation step to be applied when the simulation is performed. A good balance between performance/precision is achieved with a 0.01f value.
        /// </summary>
        public float SimulatorStep { get; set; }

        /// <summary>
        /// The set of all instructions to be taken into account in a simulation.
        /// </summary>
        public List<SimulatorInstruction> Actions { get; protected set; }

        /// <summary>
        /// Specifies if the simulator should retain debug information.
        /// </summary>
        public bool DebugInfo { get; set; }

        /// <summary>
        /// Container for debug information that is generated by the simulator.
        /// </summary>
        public List<DebugInformation> SimulationHistoryDebugInformation { get; set; }

        /// <summary>
        /// The type of debug information to be generated (if DebugInfo is true) at each simulation step.
        /// </summary>
        public DebugInfoMode DebugInfoSelected { get; set; }

        /// <summary>
        /// Specifies the color of the debug information generated for the path of the circle.
        /// </summary>
        public XNAStub.Color DebugCirclePathColor { get; set; }

        /// <summary>
        /// Specifies the color of the debug information generated for the path of the rectangle.
        /// </summary>
        public XNAStub.Color DebugRectanglePathColor { get; set; }

        //auxiliary debug information variables
        Vector2 initialCirclePosition = Vector2.Zero;
        Vector2 initialRectanglePosition = Vector2.Zero;
        
        /// <summary>
        /// Create a level simulator based on the physics engine and circle and rectangle representations.
        /// </summary>
        /// <param name="simulator">The undelying physics engine instance.</param>
        /// <param name="circleCharacter">The associated circle character that executes circle character related simulation logic.</param>
        /// <param name="rectangleCharacter">The associated rectangle character that executes rectangle character related simulation logic.</param>
        internal ActionSimulator(PhysicsSimulator simulator, CircleCharacter circleCharacter, RectangleCharacter rectangleCharacter)
        {
            Setup(simulator, circleCharacter, rectangleCharacter);
        }

        /// <summary>
        /// Auxiliary setup/re-setup for the Action Simulator instance according to the given physics engine and circle and rectangle representations.
        /// </summary>
        /// <param name="simulator">The undelying physics engine instance.</param>
        /// <param name="circleCharacter">The associated circle character that executes circle character related simulation logic.</param>
        /// <param name="rectangleCharacter">The associated rectangle character that executes rectangle character related simulation logic.</param>
        internal void Setup(PhysicsSimulator simulator, CircleCharacter circleCharacter, RectangleCharacter rectangleCharacter)
        {
            Simulator = simulator;
            AssociatedCircleCharacter = circleCharacter;
            AssociatedRectangleCharacter = rectangleCharacter;

            CollectiblesToRemove = new List<Geom>();
            CollectiblesUncaught = new List<CollectibleRepresentation>();
            CollectiblesCaught = new List<CollectibleRepresentation>();
            Actions = new List<SimulatorInstruction>();
            SimulationHistoryDebugInformation = new List<DebugInformation>();

            DebugInfo = false;
            DebugInfoSelected = DebugInfoMode.Both;
            initialCirclePosition = Vector2.Zero;
            initialRectanglePosition = Vector2.Zero;
            DebugCirclePathColor = XNAStub.Color.Yellow;
            DebugRectanglePathColor = XNAStub.Color.Green;

            SimulatedTime = 0;
            SimulatorStep = 0.01f;

            //properly bind the collectibles in the simulator
            foreach (Geom item in Simulator.GeomList)
            {
                if (item.Tag.GetType() == typeof(String))
                {
                    //we can have a collectible
                    if (((String)item.Tag) == TriangleCollectible.COLLECTIBLE_ID)
                    {
                        //TODO: test indexing and geom equality
                        Vector2 centroid = item.WorldVertices.GetCentroid();
                        CollectiblesUncaught.Add(new CollectibleRepresentation(centroid.X, centroid.Y));
                        //bind the coollectibe for proper handling in the simulator
                        item.Collision += HandleCollectibleCollision;
                    }
                }
            }
            //get the instance ready to be reset
            SimulatorReset = Simulator.Clone();
            AssociatedCircleCharacterReset = null;
            AssociatedRectangleCharacterReset = null;
            foreach (Geom item in SimulatorReset.GeomList)
            {
                if (item.Tag.GetType() == typeof(String))
                {
                    string tag = (String)item.Tag;
                    if (tag == CircleCharacter.CIRCLE_CHARACTER_ID)
                    {
                        AssociatedCircleCharacterReset = AssociatedCircleCharacter.Clone(SimulatorReset, item.Body, item);
                    }
                    else if (tag == RectangleCharacter.RECTANGLE_CHARACTER_ID)
                    {
                        AssociatedRectangleCharacterReset = AssociatedRectangleCharacter.Clone(SimulatorReset, item.Body, item);
                    }
                }
            }

            if (AssociatedCircleCharacterReset == null)
                Log.LogWarning("ActionSimulator: No geometry found in the simulator that was representative of the circle character.");
            if (AssociatedCircleCharacterReset == null)
                Log.LogWarning("ActionSimulator: No geometry found in the simulator that was representative of the rectangle character.");
        }

        /// <summary>
        /// Properly trigger collectible caught events.
        /// </summary>
        /// <param name="caught">A representation of the collectible caught.</param>
        private void OnSimulatorCollectedEvent(CollectibleRepresentation caught)
        {
            if (SimulatorCollectedEvent != null)
                SimulatorCollectedEvent(this, caught);
        }

        /// <summary>
        /// Simulator specific handler for circle or rectangle collisions with collectibles.
        /// </summary>
        /// <param name="g1">The first geometry that collided.</param>
        /// <param name="g2">The second geometry that collided.</param>
        /// <param name="contactList">The list of contact points between the two geometries.</param>
        /// <returns>True if the collision is to be applied, false if it should be ignored/cancelled.</returns>
        private bool HandleCollectibleCollision(Geom g1, Geom g2, ContactList contactList)
        {
            if (g1.Tag.Equals(CircleCharacter.CIRCLE_CHARACTER_ID) || g1.Tag.Equals(RectangleCharacter.RECTANGLE_CHARACTER_ID))
            {
                //g2 is the collectible
                CollectiblesToRemove.Add(g2);
                //update reporting structures
                Vector2 centroid = g2.WorldVertices.GetCentroid();
                CollectibleRepresentation tmp = new CollectibleRepresentation(centroid.X, centroid.Y);
                CollectiblesCaught.Add(tmp);
                CollectiblesUncaught.Remove(tmp);
                OnSimulatorCollectedEvent(tmp);
            }
            else if (g2.Tag.Equals(CircleCharacter.CIRCLE_CHARACTER_ID) || g2.Tag.Equals(RectangleCharacter.RECTANGLE_CHARACTER_ID))
            {
                //g1 is the collectible
                CollectiblesToRemove.Add(g1);
                //update reporting structures
                Vector2 centroid = g1.WorldVertices.GetCentroid();
                CollectibleRepresentation tmp = new CollectibleRepresentation(centroid.X, centroid.Y);
                CollectiblesCaught.Add(tmp);
                CollectiblesUncaught.Remove(tmp);
                OnSimulatorCollectedEvent(tmp);
            }
            return false;
        }

        /// <summary>
        /// Resets the ActionSimulator instance to the state of when it was first created.
        /// </summary>
        public void ResetSimulator()
        {
            Setup(SimulatorReset, AssociatedCircleCharacterReset, AssociatedRectangleCharacterReset);
        }

        /// <summary>
        /// Advance the simulation by the given amount of time (in seconds).
        /// </summary>
        /// <param name="timeToSimulate">The amount of time to simulate (in seconds).</param>
        public void Update(float timeToSimulate)
        {
            if (timeToSimulate < 0)
                throw new InvalidSimulationTimeException(); 
            
            //advance the simulation
            SimulatedTime += timeToSimulate;
            if (timeToSimulate < SimulatorStep)
            {
                //setup debug information
                if (DebugInfo)
                    SetupDebugInformation();
                //apply agent actions
                UpdateActions(SimulatedTime);
                //update simulation
                Simulator.Update(timeToSimulate);

                //register debug information
                if (DebugInfo)
                    DebugCharactersInformation();
            }
            else
            {
                float totalTime = 0;
                float realSimulationStep;
                while(totalTime < timeToSimulate){
                    //setup debug information
                    if (DebugInfo)
                        SetupDebugInformation();
                    //set the proper simulation time
                    if (totalTime + SimulatorStep > timeToSimulate)
                        realSimulationStep = timeToSimulate - totalTime;
                    else
                        realSimulationStep = SimulatorStep;
                    //update agent actions
                    UpdateActions(SimulatedTime);
                    //simulate
                    Simulator.Update(realSimulationStep);
                    totalTime += realSimulationStep;
                
                    //register debug information
                    if (DebugInfo)
                        DebugCharactersInformation();
                }
            }            

            //remove collectibles that were collected from the simulation            
            Body body;
            foreach (Geom item in CollectiblesToRemove)
            {
                if (!item.IsDisposed)
                {
                    body = item.Body;
                    Simulator.Remove(item);
                    Simulator.Remove(body);              
                }
            }
            CollectiblesToRemove.Clear();
        }

        /// <summary>
        /// Resets the initial positioning registration for debug reporting of the game characters.
        /// </summary>
        protected void SetupDebugInformation()
        {
            switch (DebugInfoSelected)
            {
                case DebugInfoMode.Circle:
                    initialCirclePosition = new Vector2(CirclePositionX, CirclePositionY);
                    break;
                case DebugInfoMode.Rectangle:
                    initialRectanglePosition = new Vector2(RectanglePositionX, RectanglePositionY);
                    break;
                case DebugInfoMode.Both:
                    initialCirclePosition = new Vector2(CirclePositionX, CirclePositionY);
                    initialRectanglePosition = new Vector2(RectanglePositionX, RectanglePositionY);
                    break;
                default:
                    initialCirclePosition = initialRectanglePosition = Vector2.Zero;
                    break;
            }
        }

        /// <summary>
        /// Adds to the simulation history the debug information associated with the two characters
        /// </summary>
        protected void DebugCharactersInformation()
        {
            switch (DebugInfoSelected)
            {
                case DebugInfoMode.Circle:
                    //circle
                    SimulationHistoryDebugInformation.Add(DebugInformationFactory.CreateLineDebugInfo(
                        new PointF(initialCirclePosition.X, initialCirclePosition.Y),
                        new PointF(CirclePositionX, CirclePositionY),
                        DebugCirclePathColor));
                    break;
                case DebugInfoMode.Rectangle:
                    //rectangle
                    SimulationHistoryDebugInformation.Add(DebugInformationFactory.CreateLineDebugInfo(
                        new PointF(initialRectanglePosition.X, initialRectanglePosition.Y),
                        new PointF(RectanglePositionX, RectanglePositionY),
                        DebugRectanglePathColor));
                    break;
                case DebugInfoMode.Both:
                    //circle
                    SimulationHistoryDebugInformation.Add(DebugInformationFactory.CreateLineDebugInfo(
                        new PointF(initialCirclePosition.X, initialCirclePosition.Y),
                        new PointF(CirclePositionX, CirclePositionY),
                        DebugCirclePathColor));
                    //rectangle
                    SimulationHistoryDebugInformation.Add(DebugInformationFactory.CreateLineDebugInfo(
                        new PointF(initialRectanglePosition.X, initialRectanglePosition.Y),
                        new PointF(RectanglePositionX, RectanglePositionY),
                        DebugRectanglePathColor));
                    break;
                default:
                    //nothing to register
                    break;
            }
        }

        /// <summary>
        /// Applies the active character actions and removes finished actions.
        /// </summary>
        /// <param name="currentTime">The current simulation time to determine if a given actions is active or not.</param>
        protected void UpdateActions(float currentTime)
        {
            List<SimulatorInstruction> toRemove = new List<SimulatorInstruction>();

            foreach (SimulatorInstruction instruction in Actions)
            {
                if (instruction.IsActive(currentTime))
                {
                    switch (instruction.Move)
                    {
                        case Moves.NO_ACTION:
                            //nothing to do
                            break;
                        case Moves.ROLL_LEFT:
                            AssociatedCircleCharacter.SpinLeft();
                            break;
                        case Moves.ROLL_RIGHT:
                            AssociatedCircleCharacter.SpinRight();
                            break;
                        case Moves.JUMP:
                            AssociatedCircleCharacter.Jump(AgentsManager.DEFAULT_CIRCLE_AGENT_JUMP_INTESITY);
                            break;
                        case Moves.GROW:
                            AssociatedCircleCharacter.Grow();
                            break;
                        case Moves.MOVE_LEFT:
                            AssociatedRectangleCharacter.SlideLeft();
                            break;
                        case Moves.MOVE_RIGHT:
                            AssociatedRectangleCharacter.SlideRight();
                            break;
                        case Moves.MORPH_UP:
                            AssociatedRectangleCharacter.StretchVerticalUp();
                            break;
                        case Moves.MORPH_DOWN:
                            AssociatedRectangleCharacter.StretchVerticalDown();
                            break;
                        default:
                            Log.LogWarning("ActionSimulator: unhandled instruction type: " + instruction.Move);
                            break;
                    }
                }
                else if (instruction.Finished(currentTime))
                {
                    //if instruction is finished it should be removed from the list of instructions to execute
                    toRemove.Add(instruction);
                }
                //set character properties that affect character possible actions
                AssociatedCircleCharacter.SetCollisionState(false);
                AssociatedRectangleCharacter.SetCanStretch(true);
            }

            //remove all finshed instructions from the simulator instructions list
            foreach (SimulatorInstruction item in toRemove)
            {
                Actions.Remove(item);
            }
        }

        /// <summary>
        /// Adds a new action instruction to be simulated starting at the current simulator time and with the duration of a single simulation step.
        /// </summary>
        /// <param name="move">The action to be performed.</param>
        public void AddInstruction(Moves move)
        {
            Actions.Add(new SimulatorInstruction(move, SimulatedTime, SimulatorStep));
        }

        /// <summary>
        /// Adds a new action instruction to be simulated starting at the current simulator time.
        /// </summary>
        /// <param name="move">The action to be performed.</param>
        /// <param name="duration">The duration of time for the action to be active (in seconds).</param>
        public void AddInstruction(Moves move, float duration)
        {
            Actions.Add(new SimulatorInstruction(move, SimulatedTime, duration));
        }

        /// <summary>
        /// Adds a new action instruction to be simulated starting at the given startTime.
        /// </summary>
        /// <param name="move">The action to be performed.</param>
        /// <param name="duration">The duration of time for the action to be active (in seconds).</param>
        /// <param name="startTime">The simulator time when the action should start to be applied</param>
        public void AddInstruction(Moves move, float duration, float startTime)
        {
            Actions.Add(new SimulatorInstruction(move, startTime, duration));
        }

        /// <summary>
        /// Determines if both the circle and rectangle characters are ready to be simulated.
        /// </summary>
        /// <returns>True if the characters are ready for the simulation, false otherwise.</returns>
        public bool CharactersReady()
        {
            if (AssociatedCircleCharacter != null && AssociatedRectangleCharacter != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Clones the simulator. Copy from AgentsManager. Made public for accessibility
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public ActionSimulator CreateUpdatedSimulator()
        {
            PhysicsSimulator clonedPhysicsSimulator = Simulator.Clone();
            CircleCharacter circle = null;
            RectangleCharacter rectangle = null;
            foreach (Geom item in clonedPhysicsSimulator.GeomList)
            {
                if (item.Tag.GetType() == typeof(String))
                {
                    string tag = (String)item.Tag;
                    if (tag == CircleCharacter.CIRCLE_CHARACTER_ID)
                    {
                        circle = AssociatedCircleCharacter.Clone(clonedPhysicsSimulator, item.Body, item);
                    }
                    else if (tag == RectangleCharacter.RECTANGLE_CHARACTER_ID)
                    {
                        rectangle = AssociatedRectangleCharacter.Clone(clonedPhysicsSimulator, item.Body, item);
                    }
                }
            }

            if (circle == null)
                Log.LogWarning("AgentsManager: No geometry found in the simulator that was representative of the circle character.");
            if (rectangle == null)
                Log.LogWarning("AgentsManager: No geometry found in the simulator that was representative of the rectangle character.");

            return new ActionSimulator(clonedPhysicsSimulator, circle, rectangle);
        }

        public void RemoveInstructions()
        {
            Actions.Clear();
        }

        /***********************************/
    }
}
