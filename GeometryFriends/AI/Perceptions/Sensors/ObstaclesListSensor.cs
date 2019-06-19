using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using GeometryFriends.XNAStub;
using System;
using System.Collections.Generic;

namespace GeometryFriends.AI.Perceptions
{
    class ObstaclesListSensor : Sensor
    {
        private List<RectanglePlatform> allObstList;
        private List<RectanglePlatform> obstList;
        private Level l;
        public override Sensors Type
        {
            get
            {
                return Sensors.ObstaclesListSensor;
            }
        }

        public ObstaclesListSensor(Level l)
            : base()
        {
            this.l = l;
            obstList = new List<RectanglePlatform>();
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
                if (o.getColor() == Color.Black)
                {
                    obstList.Add(o);
                }
            }
            
            return obstList;

        }

        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " contains " + this.l.GetObstacles().Count + " elements" ;
        }


    }
}
