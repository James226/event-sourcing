using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class AggregateLoader<T> where T : new()
    {
        private readonly Action<Aggregate<T>> _setHandlers;
        private readonly Action<Aggregate<T>> _setObservers;
        private readonly IEventStore _eventStore;

        public AggregateLoader(Action<Aggregate<T>> setHandlers, Action<Aggregate<T>> setObservers, IEventStore eventStore)
        {
            _setHandlers = setHandlers;
            _setObservers = setObservers;
            _eventStore = eventStore;
        }

        public Aggregate<T> Create<TEvent>(Guid id, TEvent evt) where TEvent : DomainEvent
        {
            var aggregate = new Aggregate<T>(id);
            _setHandlers?.Invoke(aggregate);
            aggregate.Events.Subscribe(_eventStore.Store);
            _setObservers?.Invoke(aggregate);
            aggregate.Update(evt);
            return aggregate;
        }

        public async Task<Aggregate<T>> Load(Guid id)
        {
            var events = await _eventStore.Load(id);

            var aggregate = new Aggregate<T>(id);
            _setHandlers?.Invoke(aggregate);
            _setObservers?.Invoke(aggregate);
            aggregate.LoadFrom(events);
            return aggregate;
        }
    }
}