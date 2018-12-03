using System;
using Proto;
using Saga.Internal;

namespace Saga
{
    public class SagaProgram
    {
        private readonly ISagaConsole _console;
        private readonly ISagaConsole _resultConsole;
        private readonly bool _verbose;
        private Probabilities _probabilities;
        private readonly int? _seed;
        public Action<ISagaConsole> OnFinished = _ => { };

        public SagaProgram(ISagaConsole console, ISagaConsole resultConsole, Probabilities probabilities, bool verbose, int? seed = null)
        {
            _console = console;
            _resultConsole = resultConsole;
            _probabilities = probabilities;
            _seed = seed;
            _verbose = verbose;
        }

        public void Run()
        {
            _console.WriteLine("Starting");
            var numberOfTransfers = 500;
            var intervalBetweenConsoleUpdates = 1;
            var retryAttempts = 0;

            var props = Props.FromProducer(() =>
            {
                var monkey = new ChaosMonkey(_seed, _probabilities);
                var run = new Runner(
                    _console,
                    _resultConsole,
                    _probabilities,
                    numberOfTransfers, 
                    intervalBetweenConsoleUpdates, 
                    retryAttempts, 
                    _verbose
                );
                run.OnFinished = OnFinished;
                return run;
            }).WithChildSupervisorStrategy(new OneForOneStrategy((pid, reason) => SupervisorDirective.Restart, retryAttempts, null));
            
            _console.WriteLine("Spawning runner");
            var runner = RootContext.Empty.SpawnNamed(props, "runner");
            _console.PauseBeforeClosing();
        }
    }
}
