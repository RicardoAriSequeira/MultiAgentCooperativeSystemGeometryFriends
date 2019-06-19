#region Using Statements
using GeometryFriends.XNAStub;
using GeometryFriends.XNAStub.DrawingInstructions;
using System;
#endregion

namespace GeometryFriends.ScreenSystem
{
    class SimulationScreen : GameScreen
    {
        public int SimulationsCount { get; set; }
        public Func<GameScreen> GetSimulationScreen { get; private set; }

        public SimulationScreen(int simulationCount, Func<GameScreen> simulationScreenCommand)
        {
            SimulationsCount = simulationCount;
            GetSimulationScreen = simulationScreenCommand;            
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (!coveredByOtherScreen)
            {
                //previous simulation has finished start a new one or exit
                if (SimulationsCount == 0)
                {
                    ScreenManager.Game.Exit();             
                }
                else
                {
                    SimulationsCount--;
                    GameScreen newLevelScreen = GetSimulationScreen();
                    if (newLevelScreen != null)
                    {
                        ScreenManager.AddScreen(newLevelScreen);
                    }
                }
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime, DrawingInstructionsBatch instructionsBatch){}
    }
}
