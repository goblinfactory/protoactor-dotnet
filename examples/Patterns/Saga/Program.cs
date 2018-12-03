using Proto;
using Saga.Internal;

namespace Saga
{

    internal class Program
    {
        private static RootContext Context = RootContext.Empty;

        public static void Main(string[] args)
        {
            var console = new SagaConsole(System.ConsoleColor.White);
            var results = new SagaConsole(System.ConsoleColor.Green);
            var probabilities = new Probabilities(refusalProbability:0.001, busyProbability:0.001, serviceUptime :99.999, longRunningSimulationMs:50);

            var saga = new SagaProgram(console, results, probabilities, 50, 5, 0, false);
            saga.Run();
        }
    }
}
