using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Levels;
using System;


namespace GeometryFriends.AI.Perceptions
{
    class RectanglePositionSensor : Sensor
    {
        private Level l;

        public override Sensors Type
        {
            get
            {
                return Sensors.RectanglePositionSensor;
            }
        }


        public RectanglePositionSensor(Level l)
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

        public Vector2 GetData()
        {
            return l.rectangle.Body.Position;
        }

        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " - " + this.l.rectangle.Body.Position;
        }
    }
}
