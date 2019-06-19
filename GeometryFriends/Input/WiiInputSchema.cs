
namespace GeometryFriends.Input
{
    internal abstract class WiiInputSchema
    {
        public abstract WiimoteNumber ControllerNumber
        {
            get;
        }

        public abstract bool MoveRight(InputState input);
        public abstract bool MoveLeft(InputState input);
    }
}
