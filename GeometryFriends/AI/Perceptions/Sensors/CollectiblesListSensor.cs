using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using System;
using System.Collections.Generic;

namespace GeometryFriends.AI.Perceptions
{
    class CollectiblesListSensor : Sensor
    {
        private Level l;

        protected List<TriangleCollectible> collectiblesLeft;

        public override Sensors Type
        {
            get
            {
                return Sensors.CollectiblesListSensor;
            }
        }

        public CollectiblesListSensor(Level l)
            : base()
        {
            this.l = l;      
        }

        public override void Start()
        {
            this.collectiblesLeft = new List<TriangleCollectible>();
            this.collectiblesLeft.AddRange(l.GetCollectibles());
        }

        public void Stop()
        { 
        }

        protected override void UpdateLogic()
        {
            if (collectiblesLeft != null)
            {
                for (int i = collectiblesLeft.Count - 1; i >= 0; i--)
                {
                    if (collectiblesLeft[i].IsCollected())
                    {
                        collectiblesLeft.RemoveAt(i);
                    }
                }
            }
        }

        public List<TriangleCollectible> GetData()
        {
            return this.collectiblesLeft;
        }

        public override String Debug()
        {
            return "Sensor: " + this.ToString() + " contains " + this.l.GetCollectibles().Count + " elements" ;
        }

    }
}
