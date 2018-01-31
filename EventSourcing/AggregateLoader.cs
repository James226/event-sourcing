using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class AggregateLoader<T> where T : new()
    {
        private readonly IHandlerSet<T> _handlers;
        private readonly Action<IAsyncObservable<DomainEvent>, T> _setObservers;
        private readonly IEventStore _eventStore;

        public AggregateLoader(IHandlerSet<T> handlers, Action<IAsyncObservable<DomainEvent>, T> setObservers, IEventStore eventStore)
        {
            _handlers = handlers;
            _setObservers = setObservers;
            _eventStore = eventStore;
        }

        public Aggregate<T> Create<TEvent>(string id, TEvent evt) where TEvent : DomainEvent
        {
            var aggregate = new Aggregate<T>(_handlers, id);
            aggregate.Events.Subscribe(new AsyncObserver<DomainEvent>(_eventStore.Store));
            _setObservers?.Invoke(aggregate.Events, aggregate.State);
            aggregate.Update(evt).Wait();
            return aggregate;
        }

        public async Task<Aggregate<T>> Load(string id)
        {
            var events = await _eventStore.Load(id);

            var aggregate = new Aggregate<T>(_handlers, id);
            aggregate.Events.Subscribe(new AsyncObserver<DomainEvent>(_eventStore.Store));
            _setObservers?.Invoke(aggregate.Events, aggregate.State);
            aggregate.LoadFrom(events);
            return aggregate;
        }
    }
}