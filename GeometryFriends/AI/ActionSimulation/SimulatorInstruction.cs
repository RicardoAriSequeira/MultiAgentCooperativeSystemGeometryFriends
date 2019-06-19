
namespace GeometryFriends.AI.ActionSimulation
{
    public class SimulatorInstruction
    {
        /// <summary>
        /// The move conceptualized in this simulator instruction.
        /// </summary>
        public Moves Move { get; internal set; }
        /// <summary>
        /// The start time when this instruction should start to be carried out.
        /// </summary>
        public float StartTime { get; internal set; }
        /// <summary>
        /// The duration for which the associated instruction should last.
        /// </summary>
        public float Duration { get; internal set; }
        
        private float endTime;

        /// <summary>
        /// Constructor for a simulator instruction to be applied during simulations.
        /// </summary>
        /// <param name="move">The movie associated with this instrction.</param>
        /// <param name="startTime">The time when this instruction should start to be applied.</param>
        /// <param name="duration">The duration of time of the associated instruction.</param>
        internal SimulatorInstruction(Moves move, float startTime, float duration)
        {
            Move = move;
            StartTime = startTime;
            Duration = duration;

            endTime = startTime + duration;
        }

        /// <summary>
        /// Determines if the instruction should be applied for the given simulator time.
        /// </summary>
        /// <param name="currentTime">The current simulator time.</param>
        /// <returns>True if it active, false otherwise.</returns>
        public bool IsActive(float currentTime)
        {
            if (currentTime >= StartTime && currentTime < endTime)
                return true;
            return false;
        }

        /// <summary>
        /// Determines if the instruction has already finished being applied at the given simulator time.
        /// </summary>
        /// <param name="currentTime">The current simulator time.</param>
        /// <returns>True if it is finished, false otherwise.</returns>
        public bool Finished(float currentTime)
        {
            if (currentTime >= endTime)
                return true;
            return false;
        }
    }
}
