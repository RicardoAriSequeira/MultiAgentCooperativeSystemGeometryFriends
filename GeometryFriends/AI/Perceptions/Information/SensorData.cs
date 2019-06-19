using GeometryFriends.AI.ActionSimulation;

namespace GeometryFriends.AI.Perceptions.Information
{
    // This class is merely used for passing data upon starting a thread.
    internal class SensorData
    {
        public int nC;
        public RectangleRepresentation rI;
        public CircleRepresentation cI;
        public CollectibleRepresentation[] colI;
        public ActionSimulator simulator;

        public SensorData(int nC, RectangleRepresentation rI, CircleRepresentation cI, CollectibleRepresentation[] colI, ActionSimulator simulator)
        {
            this.nC = nC;
            this.rI = rI;
            this.cI = cI;
            this.colI = colI;
            this.simulator = simulator;
        }
    }
}
