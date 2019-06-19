using FarseerGames.FarseerPhysics.Mathematics;
using System;

namespace GeometryFriends.AI.Perceptions
{
    class IdleCircleTimeSensor : Sensor
    {
        public override Sensors Type
        {
            get
            {
                return Sensors.IdleCircleTimeSensor;
            }
        }
        private SensorsManager sensorsManager;
        private long timeSinceLastMove;
        private Vector2 lastPosition;

        public IdleCircleTimeSensor(SensorsManager sm) : base()
        {
            this.sensorsManager = sm;
        }

        public override void Start()
        {
            timeSinceLastMove = 0;
            lastUpdateTime = 0;
            this.lastPosition = sensorsManager.CirclePosition;            
        }

        protected override void UpdateLogic()
        {
            if (sensorsManager != null)
            {
                if (sensorsManager.CirclePosition.Equals(this.lastPosition))
                {
                    timeSinceLastMove += DateTime.Now.Ticks - lastUpdateTime;
                }
                else
                {
                    timeSinceLastMove = 0;
                }
            }

            lastUpdateTime = DateTime.Now.Ticks;
        }

        public long GetData()
        {            
            return timeSinceLastMove;
        }

        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " - " + this.timeSinceLastMove;
        }
    }
}
