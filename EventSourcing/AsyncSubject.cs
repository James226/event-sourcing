using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class AsyncSubject<T> : IAsyncObservable<T>, IAsyncObserver<T>
    {
        private readonly List<IAsyncObserver<T>> _subscribers = new List<IAsyncObserver<T>>();

        public IDisposable Subscribe(IAsyncObserver<T> subscriber)
        {
            _subscribers.Add(subscriber);
            return new Unsubscriber<T>(RemoveSubscriber, subscriber);
        }

        public async Task OnNext(T value)
        {
            foreach (var subscriber in _subscribers)
            {
                await subscriber.OnNext(value);
            }
        }

        public async Task OnCompleted()
        {
            foreach (var subscriber in _subscribers)
            {
                await subscriber.OnCompleted();
            }
        }

        public async Task OnError(Exception error)
        {
            foreach (var subscriber in _subscribers)
            {
                await subscriber.OnError(error);
            }
        }

        private void RemoveSubscriber(IAsyncObserver<T> subscriber)
        {
            _subscribers.Remove(subscriber);
        }
    }
}