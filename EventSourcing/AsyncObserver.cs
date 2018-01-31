using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class AsyncObserver<T> : IAsyncObserver<T>
    {
        private readonly Func<T, Task> _onNext;
        private readonly Func<Exception,Task> _onError;
        private readonly Func<Task> _onCompleted;

        public AsyncObserver(Func<T, Task> onNext, Func<Exception, Task> onError = null, Func<Task> onCompleted = null)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public async Task OnNext(T value)
        {
            var onNext = _onNext;
            if (onNext != null)
            {
                await onNext.Invoke(value);
            }
        }

        public async Task OnError(Exception error)
        {
            var onError = _onError;
            if (onError != null)
            {
                await onError.Invoke(error);
            }
        }

        public async Task OnCompleted()
        {
            var onCompleted = _onCompleted;
            if (onCompleted != null)
            {
                await onCompleted.Invoke();
            }
        }
    }
}