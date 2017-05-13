using System;

namespace EventSourcing
{
    public class DisposableFunction : IDisposable
    {
        private readonly Action _disposeFunc;

        public DisposableFunction(Action disposeFunc)
        {
            _disposeFunc = disposeFunc;
        }

        public void Dispose()
        {
            _disposeFunc?.Invoke();
        }
    }
}