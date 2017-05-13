using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class ObserverWrapper<T> : IAsyncObserver<T>
    {
        private readonly Func<T, Task> _onNext;
        private readonly Func<Exception, Task> _onError;
        private readonly Func<Task> _onCompleted;

        public ObserverWrapper(Func<T, Task> onNext, Func<Exception, Task> onError, Func<Task> onCompleted)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }

        public static IAsyncObserver<T> Create<TU>(IAsyncObserver<TU> observer) where TU : class, T
        {
            return Create<TU>(observer.OnNext, observer.OnError, observer.OnCompleted);
        }

        public static IAsyncObserver<T> Create<TU>(Func<TU, Task> onNext, Func<Exception, Task> onError, Func<Task> onCompleted) where TU : class, T
        {
            async Task Next(T v)
            {
                var val = v as TU;
                if (val != null)
                {
                    await onNext(val);
                }
            }
            return new ObserverWrapper<T>(Next, onError, onCompleted);
        }

        public Task OnCompleted()
        {
            return _onCompleted?.Invoke();
        }

        public Task OnError(Exception error)
        {
            return _onError?.Invoke(error);
        }

        public Task OnNext(T value)
        {
            return _onNext?.Invoke(value);
        }
    }
}