using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Levels;
using System;

namespace GeometryFriends.AI.Perceptions
{
    class CircleVelocitySensor : Sensor
    {

        private Level l;

        public override Sensors Type
        {
            get
            {
                return Sensors.CircleVelocitySensor;
            }
        }

        public CircleVelocitySensor(Level l)
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
            return l.circle.Body.LinearVelocity;
        }

        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " - " + this.l.circle.Body.LinearVelocity;
        }
    }
}