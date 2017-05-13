using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class SerialObservable<T> : IAsyncObserver<T>, IAsyncObservable<T>
    {
        private readonly List<IAsyncObserver<T>> _observers = new List<IAsyncObserver<T>>();

        public IDisposable Subscribe<TU>(IAsyncObserver<TU> observer) where TU : class, T
        {
            var observerWrapper = ObserverWrapper<T>.Create(observer);
            _observers.Add(observerWrapper);
            return new DisposableFunction(() => _observers.Remove(observerWrapper));
        }

        public IDisposable Subscribe<TU>(Func<TU, Task> onNext) where TU : class, T
        {
            var observerWrapper = ObserverWrapper<T>.Create(onNext, _ => Task.FromResult(0), () => Task.FromResult(0));
            _observers.Add(observerWrapper);
            return new DisposableFunction(() => _observers.Remove(observerWrapper));
        }

        public IDisposable Subscribe(Func<T, Task> onNext)
        {
            var observerWrapper = new ObserverWrapper<T>(onNext, _ => Task.FromResult(0), () => Task.FromResult(0));
            _observers.Add(observerWrapper);
            return new DisposableFunction(() => _observers.Remove(observerWrapper));
        }

        public async Task OnNext(T value)
        {
            foreach (var observer in _observers)
            {
                await observer.OnNext(value);
            }
        }

        public async Task OnCompleted()
        {
            foreach (var observer in _observers)
            {
                await observer.OnCompleted();
            }
        }

        public async Task OnError(Exception error)
        {
            foreach (var observer in _observers)
            {
                await observer.OnError(error);
            }
        }
    }
}