using System;
using System.Collections.Generic;
using System.Text;

namespace Saga.Internal
{
    public interface ISagaConsole
    {
        void Write(string text);
        void WriteLine(string text);
        void PauseBeforeClosing();
    }
}
