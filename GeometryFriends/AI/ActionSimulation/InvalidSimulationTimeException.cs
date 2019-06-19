using System;

namespace GeometryFriends.AI.ActionSimulation
{
    internal class InvalidSimulationTimeException : Exception
    {
        internal InvalidSimulationTimeException() : base("Cannot update the simulator with a negative time to simulate value.")
        {
        }
    }
}
