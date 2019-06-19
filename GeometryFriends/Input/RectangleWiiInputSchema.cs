
namespace GeometryFriends.Input
{
    internal abstract class RectangleWiiInputSchema : WiiInputSchema
    {
        public override WiimoteNumber ControllerNumber
        {
            get
            {
                return WiimoteNumber.WII_2;
            }
        }

        public abstract bool MorphUp(InputState input);
        public abstract bool MorphDown(InputState input);        
    }
}
