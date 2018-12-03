using System;
using System.Threading;

namespace Saga
{
    /// <summary>
    /// A nice tame 'preditable' chaos monkey to use for testing.
    /// </summary>
    public class PredictableChaosMonkey : IChaosMonkey
    {
        private Random _random;
        public double ServiceUptime { get; }
        public double RefusalProbability { get; }
        public double BusyProbability { get; }
        public int LongRunningSimulationMs { get; }

        public PredictableChaosMonkey(int seed, double serviceUptime, double refusalProbability, double busyProbability, int longRunningSimulationMs)
        {
            ServiceUptime = serviceUptime;
            RefusalProbability = refusalProbability;
            BusyProbability = busyProbability;
            LongRunningSimulationMs = longRunningSimulationMs;
            _random = new Random(seed);
        }

        public bool Refuse()
        {
            var comparsion = _random.NextDouble() * 100;
            return comparsion <= RefusalProbability;
        }
        public bool Busy()
        {
            var comparsion = _random.NextDouble() * 100;
            return comparsion <= BusyProbability;
        }

        public FailBehavior AmIInTheMoodToday()
        {
            var comparision = _random.NextDouble() * 100;
            if (comparision > ServiceUptime)
            {
                return _random.NextDouble() * 100 > 50 ? FailBehavior.FailBeforeProcessing : FailBehavior.FailAfterProcessing;
            }
            return FailBehavior.ProcessSuccessfully;
        }

        public void LongRunningProcessSimulation()
        {
            Thread.Sleep(LongRunningSimulationMs);
        }
    }
}