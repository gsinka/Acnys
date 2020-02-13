using System;
using System.Threading;

namespace Acnys.Core.Testing
{
    public class AwaiterTask<T> : IDisposable
    {
        public T Event { get; set; }
        public Func<T, bool> Filter { get; }
        public ManualResetEvent ResetEvent { get; }

        public AwaiterTask(Func<T, bool> filter, ManualResetEvent resetEvent)
        {
            Filter = filter;
            ResetEvent = resetEvent;
        }

        public void Dispose()
        {
        }
    }
}