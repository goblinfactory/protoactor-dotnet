using System.Threading;

namespace Saga.Internal
{
    public class ImNoMonkey : IChaosMonkey
    {
        public ImNoMonkey(int longRunningSimulationMs)
        {
            LongRunningSimulationMs = longRunningSimulationMs;
        }

        public double RefusalProbability    => 0;
        public double BusyProbability       => 0;
        public double ServiceUptime         => 100;
        public int LongRunningSimulationMs { get; }

        public FailBehavior AmIInTheMoodToday()
        {
            return FailBehavior.ProcessSuccessfully;
        }

        public bool Busy()
        {
            return false;
        }

        public void LongRunningProcessSimulation()
        {
            Thread.Sleep(LongRunningSimulationMs);
        }

        public bool Refuse()
        {
            return false;
        }
    }
}