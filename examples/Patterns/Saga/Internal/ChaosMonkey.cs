using Saga.Internal;
using System;
using System.Threading;

namespace Saga
{
    public class ChaosMonkey : IChaosMonkey
    {
        private Random _random;

        public double RefusalProbability { get; }
        public double BusyProbability { get; }
        public double ServiceUptime { get; }
        public int LongRunningSimulationMs { get; }

        public ChaosMonkey(int? seed, Probabilities probabilities)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            RefusalProbability = probabilities.RefusalProbability;
            BusyProbability = probabilities.BusyProbability;
            ServiceUptime = probabilities.ServiceUptime;
            LongRunningSimulationMs = probabilities.LongRunningSimulationMs;
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


        public bool Refuse()
        {
            var comparsion = _random.NextDouble() * 100;
            return comparsion <= RefusalProbability;
        }

        public void LongRunningProcessSimulation()
        {
            int sleep = _random.Next(0, LongRunningSimulationMs);
            Thread.Sleep(sleep);
        }
    }
    
    
}