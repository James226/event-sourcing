using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class Aggregate<T> : IDisposable where T : new()
    {
        public Guid Id { get; }
        public T State { get; }
        public int Version { get; private set; }
        public IAsyncObservable<DomainEvent> Events => _eventsSubject;

        private readonly Dictionary<Type, Action<DomainEvent>> _handlers = new Dictionary<Type, Action<DomainEvent>>();
        private readonly SerialObservable<DomainEvent> _eventsSubject = new SerialObservable<DomainEvent>();

        public Aggregate(Guid id)
        {
            Id = id;
            State = new T();
        }

        public void When<THandlerEvent>(Action<T, THandlerEvent> handler) where THandlerEvent : DomainEvent
        {
            _handlers[typeof(THandlerEvent)] = e => handler(State, (THandlerEvent) e);
        }

        public void LoadFrom(IEnumerable<DomainEvent> pastEvents)
        {
            foreach (var @event in pastEvents)
            {
                ApplyEvent(@event);
            }
        }

        public async Task Update(DomainEvent evt)
        {
            evt.Id = Id;
            evt.Version = this.Version + 1;

            try
            {
                ApplyEvent(evt);
                await _eventsSubject.OnNext(evt);
                this.Version = evt.Version;
            }
            catch (Exception e)
            {
                await _eventsSubject.OnError(e);
                throw;
            }
        }

        private void ApplyEvent(DomainEvent @event)
        {
            Action<DomainEvent> action;
            if (_handlers.TryGetValue(@event.GetType(), out action))
            {
                action(@event);
            }
        }

        public void Dispose()
        {
            DisposeAsync().Wait();
        }

        public Task DisposeAsync()
        {
            return _eventsSubject.OnCompleted();
        }
    }
}