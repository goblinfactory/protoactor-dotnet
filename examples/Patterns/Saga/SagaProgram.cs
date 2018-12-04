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
        private readonly int _numberOfTransactions;
        private readonly int _intervalBetweenConsoleUpdates;
        private readonly int _retryAttempts;
        private readonly int? _seed;
        public Action<ISagaConsole> OnFinished = _ => { };

        public SagaProgram(ISagaConsole console, ISagaConsole resultConsole, Probabilities probabilities, int numberOfTransactions, int intervalBetweenConsoleUpdates, int retryAttempts, bool verbose, int? seed = null)
        {
            _console = console;
            _resultConsole = resultConsole;
            _probabilities = probabilities;
            _numberOfTransactions = numberOfTransactions;
            _intervalBetweenConsoleUpdates = intervalBetweenConsoleUpdates;
            _retryAttempts = retryAttempts;
            _seed = seed;
            _verbose = verbose;
        }

        public void Run()
        {
            _console.WriteLine("Starting");
            var retryAttempts = 0;

            var props = Props.FromProducer(() =>
            {
                var run = new Runner(
                    _console,
                    _resultConsole,
                    _probabilities,
                    _numberOfTransactions,
                    _intervalBetweenConsoleUpdates, 
                    _retryAttempts, 
                    _verbose
                );

                run.OnFinished+= con => OnFinished(con);
                return run;

            }).WithChildSupervisorStrategy(new OneForOneStrategy((pid, reason) => SupervisorDirective.Restart, retryAttempts, null));
            
            _console.WriteLine("Spawning runner");
            var runner = RootContext.Empty.SpawnNamed(props, "runner");
            _console.PauseBeforeClosing();
        }
    }
}
