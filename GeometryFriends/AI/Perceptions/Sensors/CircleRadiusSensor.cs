using GeometryFriends.Levels;
using System;

namespace GeometryFriends.AI.Perceptions
{
    class CircleRadiusSensor : Sensor
    {
        private Level l;

        public override Sensors Type
        {
            get
            {
                return Sensors.CircleRadiusSensor;
            }
        }

        public CircleRadiusSensor(Level l) : base()
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

        public int GetData()
        {
            return l.circle.Radius;
        }

        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " - " + this.l.circle.Radius;
        }
    }
}
