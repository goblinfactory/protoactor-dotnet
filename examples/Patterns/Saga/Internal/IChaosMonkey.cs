namespace Saga
{
    public interface IChaosMonkey
    {
        bool Refuse();
        bool Busy();
        FailBehavior AmIInTheMoodToday();
        void LongRunningProcessSimulation();
        double RefusalProbability { get; }
        double BusyProbability { get; }
        double ServiceUptime { get; }
        int LongRunningSimulationMs { get; }
    }
}