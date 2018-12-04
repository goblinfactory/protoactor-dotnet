using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Proto;
using Saga.Factories;
using Saga.Internal;
using Saga.Messages;

namespace Saga
{
    public class Runner : IActor
    {
        private RootContext Context = RootContext.Empty;
        private int _intervalBetweenConsoleUpdates;
        private readonly int _numberOfIterations;
        private readonly double _uptime;
        private readonly int _retryAttempts;
        private readonly bool _verbose;
        private readonly HashSet<PID> _transfers = new HashSet<PID>();
        private int _successResults;
        private int _failedAndInconsistentResults;
        private int _failedButConsistentResults;
        private int _unknownResults;
        private InMemoryProvider _inMemoryProvider;
        private readonly ISagaConsole _transactionReporter;
        private readonly ISagaConsole _resultReporter;
        private readonly Probabilities _probabilities;
        public Action<ISagaConsole> OnFinished = _ => { };

        public Runner(ISagaConsole transactionReporter, ISagaConsole resultReporter, Probabilities probabilities, int numberOfIterations, int intervalBetweenConsoleUpdates, int retryAttempts, bool verbose)
        {
            _transactionReporter = transactionReporter;
            _resultReporter = resultReporter;
            _probabilities = probabilities;
            _numberOfIterations = numberOfIterations;
            _intervalBetweenConsoleUpdates = intervalBetweenConsoleUpdates;
            _retryAttempts = retryAttempts;
            _verbose = verbose;
        }

        private PID CreateAccount(string name)
        {
            var accountProps = Props.FromProducer(() => Account.CreateAccountWithProbability(name, _probabilities));
            return Context.SpawnNamed(accountProps, name);
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case SuccessResult msg:
                    _successResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case UnknownResult msg:
                    _unknownResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case FailedAndInconsistent msg:
                    _failedAndInconsistentResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case FailedButConsistentResult msg:
                    _failedButConsistentResults++;
                    CheckForCompletion(msg.Pid);
                    break;
                case Started _:
                    var random = new Random(); // TODO: need to be able to seed this!
                    _inMemoryProvider = new InMemoryProvider();
                    new ForWithProgress(_numberOfIterations, _intervalBetweenConsoleUpdates, true, false).EveryNth( 
                        i =>    _transactionReporter.WriteLine($"Started {i}/{_numberOfIterations} processes"), 
                        (i, nth) => {
                            int j = i;
                            var fromAccount = CreateAccount($"FromAccount{j}");
                            var toAccount = CreateAccount($"ToAccount{j}");
                            var actorName = $"Transfer Process {j}";
                            var persistanceID = $"Transfer Process {j}";
                            var factory = new TransferFactory(context, _inMemoryProvider, random, _uptime, _retryAttempts);
                            var transfer = factory.CreateTransfer(actorName, fromAccount, toAccount, 10, persistanceID);
                            _transfers.Add(transfer);
                            if(i== _numberOfIterations && !nth) _transactionReporter.WriteLine($"Started {j}/{_numberOfIterations} proesses");
                        });
                    break;
            }
            return Actor.Done;
        }
    
        private void CheckForCompletion(PID pid)
        {
            _transfers.Remove(pid);
            
            var remaining = _transfers.Count;
            if (_numberOfIterations >= _intervalBetweenConsoleUpdates)
            {
                Console.Write(".");
                if (remaining % (_numberOfIterations / _intervalBetweenConsoleUpdates) == 0)
                {
                    _transactionReporter.WriteLine("");
                    _transactionReporter.WriteLine($"{remaining} processes remaining");
                }
            }
            else
            {
                _transactionReporter.WriteLine($"{remaining} processes remaining");
            }
            
            if (remaining == 0)
            {
                Thread.Sleep(250);
                _resultReporter.WriteLine(
                    $"RESULTS for {_uptime}% uptime, {_probabilities.RefusalProbability}% chance of refusal, {_probabilities.BusyProbability}% of being busy and {_retryAttempts} retry attempts:");
                _resultReporter.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _successResults)}% ({_successResults}/{_numberOfIterations}) successful transfers");
                _resultReporter.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _failedButConsistentResults)}% ({_failedButConsistentResults}/{_numberOfIterations}) failures leaving a consistent system");
                _resultReporter.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _failedAndInconsistentResults)}% ({_failedAndInconsistentResults}/{_numberOfIterations}) failures leaving an inconsistent system");
                _resultReporter.WriteLine(
                    $"{AsPercentage(_numberOfIterations, _unknownResults)}% ({_unknownResults}/{_numberOfIterations}) unknown results");
                
                if (_verbose)
                {
                    foreach (var stream in _inMemoryProvider.Events)
                    {
                        _resultReporter.WriteLine("");
                        _resultReporter.WriteLine($"Event log for {stream.Key}");
                        foreach (var @event in stream.Value)
                        {
                            _resultReporter.WriteLine(@event.Value.ToString());
                        }
                    }
                }

                OnFinished(_resultReporter);

            }
        }

        private double AsPercentage(double numberOfIterations, double results)
        {
            return (results / numberOfIterations) * 100;
        }
    }
}