//  Author(s):
//  João Catarino <joaopereiracatarino@gmail.com

using FarseerGames.FarseerPhysics.Mathematics;
using GeometryFriends.Levels;
using GeometryFriends.Levels.Shared;
using System;
using System.Collections.Generic;

namespace GeometryFriends.AI.Perceptions
{

    enum Sensors
    {
        CirclePositionSensor,
        CircleVelocitySensor,
        CircleMovementVectorSensor,
        CircleRadiusSensor,
        IdleCircleTimeSensor,

        RectanglePositionSensor,
        RectangleVelocitySensor,
        RectangleHeightSensor,

        RectanglePlatformsListSensor,
        CirclePlatformsListSensor,
        CollectiblesListSensor,
        ObstaclesListSensor,

        ReachableByRectangleSensor
    }


    public class SensorsManager
    {
        private Dictionary<Sensors, Sensor> _sensors;
        private SinglePlayerLevel _level;
        private DateTime agentTime;

        internal SensorsManager(SinglePlayerLevel lvl)
        {
            this._level = lvl;
            _sensors = new Dictionary<Sensors, Sensor>();
            this.CreateSensors();
            this.StartSensors();        
        }

        public Vector2 CirclePosition
        {
            get
            {
                return ((CirclePositionSensor)this._sensors[Sensors.CirclePositionSensor]).GetData();
            }
        }

        public Vector2 RectanglePosition
        {
            get
            {
                return ((RectanglePositionSensor)this._sensors[Sensors.RectanglePositionSensor]).GetData();
            }
        }

        public Vector2 RectangleVelocity
        {
            get
            {
                return ((RectangleVelocitySensor)this._sensors[Sensors.RectangleVelocitySensor]).GetData();
            }
        }

        public float RectangleHeight
        {
            get
            {
                return ((RectangleHeightSensor)this._sensors[Sensors.RectangleHeightSensor]).GetData();
            }
        }

        public int CircleRadius
        {
            get
            {
                return ((CircleRadiusSensor)this._sensors[Sensors.CircleRadiusSensor]).GetData();
            }
        }

        public Vector2 CircleVelocity
        {
            get
            {

                return ((CircleVelocitySensor)this._sensors[Sensors.CircleVelocitySensor]).GetData();
            }
        }

        public long CircleIdleTime
        {
            get
            {
                return ((IdleCircleTimeSensor)this._sensors[Sensors.IdleCircleTimeSensor]).GetData();
            }
        }

        internal List<TriangleCollectible> Collectibles
        {
            get
            {
                return ((CollectiblesListSensor)this._sensors[Sensors.CollectiblesListSensor]).GetData();
            }
        }

        internal List<RectanglePlatform> RectanglePlatforms
        {
            get
            {
                return ((RectanglePlatformsListSensor)this._sensors[Sensors.RectanglePlatformsListSensor]).GetData();
            }
        }

        internal List<RectanglePlatform> CirclePlatforms
        {
            get
            {
                return ((CirclePlatformsListSensor)this._sensors[Sensors.CirclePlatformsListSensor]).GetData();
            }
        }

        internal List<RectanglePlatform> Obstacles
        {
            get
            {
                return ((ObstaclesListSensor)this._sensors[Sensors.ObstaclesListSensor]).GetData();
            }
        }

        public double TimeSinceAgentStart
        {
            get
            {
                return (DateTime.Now - agentTime).TotalSeconds;
            }
        }

        public void CreateSensors()
        {
            this._sensors.Add(Sensors.RectanglePositionSensor, new RectanglePositionSensor(this._level));
            this._sensors.Add(Sensors.RectangleVelocitySensor, new RectangleVelocitySensor(this._level));
            this._sensors.Add(Sensors.RectangleHeightSensor, new RectangleHeightSensor(this._level));

            this._sensors.Add(Sensors.CirclePositionSensor, new CirclePositionSensor(this._level));
            this._sensors.Add(Sensors.CircleVelocitySensor, new CircleVelocitySensor(this._level));
            this._sensors.Add(Sensors.CircleRadiusSensor, new CircleRadiusSensor(this._level));
            this._sensors.Add(Sensors.IdleCircleTimeSensor, new IdleCircleTimeSensor(this));

            this._sensors.Add(Sensors.RectanglePlatformsListSensor, new RectanglePlatformsListSensor(this._level));
            this._sensors.Add(Sensors.CirclePlatformsListSensor , new CirclePlatformsListSensor(this._level));
            this._sensors.Add(Sensors.ObstaclesListSensor, new ObstaclesListSensor(this._level));

            this._sensors.Add(Sensors.CollectiblesListSensor, new CollectiblesListSensor(this._level));
            
        }

        public void StartSensors()
        {
            agentTime = DateTime.Now;
            foreach (KeyValuePair<Sensors, Sensor> s in this._sensors)
            {
                s.Value.Start();
            }
        }

        public void Sense()
        {
            foreach (KeyValuePair<Sensors, Sensor> s in this._sensors)
            {
                s.Value.UpdateSensor();
            }
        }

        public void Debug()
        {
            foreach (KeyValuePair<Sensors, Sensor> s in this._sensors)
            {
                _level.ScreenManager.Console.print(s.Value.Debug());
            }
        }
    }
}
