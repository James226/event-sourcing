using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class TypeSubject<T, TR> : IAsyncObservable<T>, IAsyncObserver<TR> where T : TR
    {
        private readonly List<IAsyncObserver<T>> _subscribers = new List<IAsyncObserver<T>>();

        public IDisposable Subscribe(IAsyncObserver<T> subscriber)
        {
            _subscribers.Add(subscriber);
            return new Unsubscriber<T>(RemoveSubscriber, subscriber);
        }

        public async Task OnNext(TR value)
        {
            if (!(value is T tVal)) return;

            foreach (var subscriber in _subscribers)
            {
                await subscriber.OnNext(tVal);
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