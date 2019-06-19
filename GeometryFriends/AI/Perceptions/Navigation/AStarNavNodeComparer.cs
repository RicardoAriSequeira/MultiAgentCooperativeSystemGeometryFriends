
namespace GeometryFriends.AI.Perceptions.Navigation
{
    public class AStarNavNodeComparer : System.Collections.IComparer
    {
        private NavNode targetNode;
        public AStarNavNodeComparer(NavNode targetNode)
        {
            this.targetNode = targetNode;
        }

        public int Compare(object x, object y)
        {       
            return (int)(((NavNode)x).GetTotalCostTo(targetNode.Position)) - (int)(((NavNode)y).GetTotalCostTo(targetNode.Position));
        }
    }
}
