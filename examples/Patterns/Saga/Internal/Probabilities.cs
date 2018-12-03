namespace Saga.Internal
{
    public class Probabilities
    {
        public Probabilities(double refusalProbability, double busyProbability, double serviceUptime, int longRunningSimulationMs)
        {
            RefusalProbability = refusalProbability;
            BusyProbability = busyProbability;
            ServiceUptime = serviceUptime;
            LongRunningSimulationMs = longRunningSimulationMs;
        }

        public double RefusalProbability { get; }
        public double BusyProbability { get; }
        public double ServiceUptime { get; }
        public int LongRunningSimulationMs { get; }
    }
}
