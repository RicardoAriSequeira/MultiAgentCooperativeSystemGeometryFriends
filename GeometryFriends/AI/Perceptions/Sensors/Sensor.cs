using System;

namespace GeometryFriends.AI.Perceptions
{
    abstract class Sensor
    {       
        protected long lastUpdateTime;

        public abstract Sensors Type
        {
            get;
        }

        public Sensor(){}

        public abstract void Start();

        public void UpdateSensor()
        {   
            UpdateLogic();
            
            this.lastUpdateTime = DateTime.Now.Ticks;
        }

        protected abstract void UpdateLogic();
       
        public abstract String Debug();
    }
}
