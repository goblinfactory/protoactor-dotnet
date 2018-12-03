﻿using Proto;
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
            var probabilities = new Probabilities(refusalProbability:0.01, busyProbability:0.01, serviceUptime :99.99, longRunningSimulationMs:150);

            var saga = new SagaProgram(console, results, probabilities, false);
            saga.Run();
        }
    }
}
