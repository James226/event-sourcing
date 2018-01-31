using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class Aggregate<T> : IDisposable where T : new()
    {
        public string Id { get; }
        public T State { get; }
        public int Version { get; private set; }
        public IAsyncObservable<DomainEvent> Events => _eventsSubject;

        private readonly AsyncSubject<DomainEvent> _eventsSubject = new AsyncSubject<DomainEvent>();
        private readonly IHandlerSet<T> _handlers;

        public Aggregate(IHandlerSet<T> handlers, string id)
        {
            _handlers = handlers;
            Id = id;
            State = new T();
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
            var handler = _handlers.GetHandler(@event.GetType());
            handler?.Invoke(State, @event);
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