using Saga;
using Saga.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Examples.Patterns.Saga.Tests
{
    public class SagaProgramTests
    {
        [Fact]
        public void Happy_path_smoke_test() // todo add in retries and tokenise results, make test deterministic, remove the randomness.
        {
            var console = new TestWriter();
            var results = new TestWriter();
            var gate = new ManualResetEvent(false);

            var saga = new SagaProgram(console, results, false);
            saga.OnFinished += _ => { gate.Set(); };

            saga.Run();
            gate.WaitOne(3000);

            var expected = new [] {
                "",
                "RESULTS for 99.99 % uptime, 0.01 % chance of refusal, 0.01 % of being busy and 0 retry attempts:",
                "60 % (3 / 5) successful transfers",
                "0 % (0 / 5) failures leaving a consistent system",
                "0 % (0 / 5) failures leaving an inconsistent system",
                "40 % (2 / 5) unknown results"
            };
            var actual = results.Buffer.Select(l => l.TrimStart().TrimEnd()).ToArray();
            Assert.Equal(expected, actual);
        }
    }

    internal class TestWriter : ISagaConsole
    {
        public List<string> Buffer { get; set; } = new List<string>();

        public void PauseBeforeClosing()
        {
            //do nothing, do not pause.
        }

        public void Write(string text)
        {
            Buffer.Add(text);
        }

        public void WriteLine(string text)
        {
            Buffer.Add(text);
        }
    }

}
