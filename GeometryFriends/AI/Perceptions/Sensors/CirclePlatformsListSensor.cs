using GeometryFriends.DrawingSystem;
using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using System;
using System.Collections.Generic;

namespace GeometryFriends.AI.Perceptions
{
    class CirclePlatformsListSensor : Sensor
    {
        private List<RectanglePlatform> allObstList;
        private List<RectanglePlatform> platformsList;
        private Level l;

        public override Sensors Type
        {
            get
            {
                return Sensors.CirclePlatformsListSensor;
            }
        }

        public CirclePlatformsListSensor(Level l)
            : base()
        {
            this.l = l;
            platformsList = new List<RectanglePlatform>();
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


        public List<RectanglePlatform> GetData()
        {
            allObstList = new List<RectanglePlatform>(l.GetObstacles());

            foreach (RectanglePlatform o in allObstList)
            {
                if (o.getColor() == GameColors.YELLOW_OBSTACLE_COLOR)
                {
                    platformsList.Add(o);
                }
            }

            return platformsList;

        }
        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " contains " + this.l.GetObstacles().Count + " elements";
        }
    }
}
