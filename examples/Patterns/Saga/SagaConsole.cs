using System;
using Saga.Internal;

namespace Saga
{
    public class SagaConsole : ISagaConsole
    {
        private readonly ConsoleColor _color;

        public SagaConsole(ConsoleColor color)
        {
           _color = color;
        }

        public void PauseBeforeClosing()
        {
            Console.ReadLine();
        }

        public void Write(string text)
        {
            Console.ForegroundColor = _color;
            Console.Write(text);
            Console.ResetColor();
        }

        public void WriteLine(string text)
        {
            Console.ForegroundColor = _color;
            Console.WriteLine(text);
            Console.ResetColor();

        }
    }
}
