using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Proto;
using Saga.Internal;
using Saga.Messages;

namespace Saga
{
    partial class Account : IActor
    {
        private readonly string _name;
        private readonly Dictionary<PID, object> _processedMessages = new Dictionary<PID, object>();
        private decimal _balance = 10;

        public IChaosMonkey Monkey { get; set;  } = new ImNoMonkey(150);

        public Account(string name)
        {
            _name = name;
        }

        private static int seed;

        public static Account CreateAccountWithProbability(string name, Probabilities probabilities)
        {
            var monkey = new ChaosMonkey(Interlocked.Increment(ref seed), probabilities);
            var account = new Account(name);
            account.Monkey = monkey;
            return account;
        }

        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case Credit msg when AlreadyProcessed(msg.ReplyTo):
                    return Reply(msg.ReplyTo);
                case Credit msg:
                    return AdjustBalance(msg.ReplyTo, msg.Amount);
                case Debit msg when AlreadyProcessed(msg.ReplyTo):
                    return Reply(msg.ReplyTo);
                case Debit msg when msg.Amount + _balance >= 0:
                    return AdjustBalance(msg.ReplyTo, msg.Amount);
                case Debit msg:
                    msg.ReplyTo.Tell(new InsufficientFunds());
                    break;
                case GetBalance _:
                    context.Respond(_balance);
                    break;
            }
            return Actor.Done;
        }
        
        private Task Reply(PID replyTo)
        {
            replyTo.Tell(_processedMessages[replyTo]);
            return Actor.Done;
        }

        /// <summary>
        ///  we want to simulate the following: 
        ///  * permanent refusals to process the message
        ///  * temporary refusals to process the message 
        ///  * failures before updating the balance
        ///  * failures after updating the balance
        ///  * slow processing
        ///  * successful processing
        /// </summary>
        private Task AdjustBalance(PID replyTo, decimal amount)
        {
            if (Monkey.Refuse())
            {
                _processedMessages.Add(replyTo, new Refused());
                replyTo.Tell(new Refused());
            }
                
            if (Monkey.Busy())
                replyTo.Tell(new ServiceUnavailable());

            // generate the behavior to be used whilst processing this message
            var behaviour = Monkey.AmIInTheMoodToday();

            if (behaviour == FailBehavior.FailBeforeProcessing)
                return Failure(replyTo);

            // simulate potential long-running process
            Monkey.LongRunningProcessSimulation();
            
            _balance += amount;
            _processedMessages.Add(replyTo, new OK());
            
            // simulate chance of failure after applying the change. This will
            // force a retry of the operation which will test the operation
            // is idempotent
            if (behaviour == FailBehavior.FailAfterProcessing)
                return Failure(replyTo);

            replyTo.Tell(new OK());
            return Actor.Done;
        }

        private Task Failure(PID replyTo)
        {
            replyTo.Tell(new InternalServerError());
            return Actor.Done;
        }

        private bool AlreadyProcessed(PID replyTo)
        {
            return _processedMessages.ContainsKey(replyTo);
        }
    }    
}