using GeometryFriends.Levels;
using System;


namespace GeometryFriends.AI.Perceptions
{
    class RectangleHeightSensor : Sensor
    {
        private Level l;

        public override Sensors Type
        {
            get
            {
                return Sensors.RectangleHeightSensor;
            }
        }


        public RectangleHeightSensor(Level l)
            : base()
        {
            this.l = l;
            
        }

        public override void Start()
        {
        }

        public void Stop()
        {
        }

        protected override void UpdateLogic()
        {
        }

        public float GetData()
        {
            return l.rectangle.Height;
        }

        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " - " + this.l.rectangle.Height;
        }
    }
}
