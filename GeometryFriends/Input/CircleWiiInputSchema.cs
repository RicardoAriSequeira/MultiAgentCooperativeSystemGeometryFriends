
namespace GeometryFriends.Input
{
    internal abstract class CircleWiiInputSchema : WiiInputSchema
    {
        public override WiimoteNumber ControllerNumber
        {
            get
            {
                return WiimoteNumber.WII_1;
            }
        }

        public abstract float Jump(InputState input);

        public abstract bool Grow(InputState input);
    }
}
