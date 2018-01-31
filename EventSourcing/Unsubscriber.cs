using System;

namespace EventSourcing
{
    internal class Unsubscriber<T> : IDisposable
    {
        private readonly Action<IAsyncObserver<T>> _removeCallback;
        private readonly IAsyncObserver<T> _observer;

        public Unsubscriber(Action<IAsyncObserver<T>> removeCallback, IAsyncObserver<T> observer)
        {
            _removeCallback = removeCallback;
            _observer = observer;
        }

        public void Dispose()
        {
            _removeCallback.Invoke(_observer);
        }
    }
}