using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriends.Levels.Shared
{
    internal class LevelResult
    {
        public string TimeStamp { get; set; }
        public bool LevelPassed { get; set; }
        public double ElapsedGameTime { get; set; }
        public double TimeLimit { get; set; }
        public int CollectiblesCaught { get; set; }
        public int CollectiblesAvailable { get; set; }

        public LevelResult(string timeStamp, bool levelPassed, double elapsedGameTime, double timeLimit, int collectiblesCaught, int collectiblesAvailable)
        {
            TimeStamp = timeStamp;
            LevelPassed = levelPassed;
            ElapsedGameTime = elapsedGameTime;
            TimeLimit = timeLimit;
            CollectiblesCaught = collectiblesCaught;
            CollectiblesAvailable = collectiblesAvailable;
        }

        public int GetScore(int levelCompletionBonus, int collectibleUnitValue)
        {
            double score = 0;

            if(LevelPassed){
                score += levelCompletionBonus;
            }

            if(!double.IsPositiveInfinity(TimeLimit)){
                score = score * (TimeLimit - ElapsedGameTime) / TimeLimit;
            }

            score += CollectiblesCaught * collectibleUnitValue;

            return Convert.ToInt32(score);
        }
    }
}
