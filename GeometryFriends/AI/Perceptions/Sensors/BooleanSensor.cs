
namespace GeometryFriends.AI.Perceptions
{
    abstract class BooleanSensor : Sensor
    {
        protected bool result;
        public bool Value
        {
            get
            {
                return this.result;
            }
            protected set
            {
                result = value;
            }
        }

        protected abstract bool GetResult();

    }
}
