using System;

namespace EventStore
{
    public class AggregateLoader<T> where T : new()
    {
        private readonly Action<Aggregate<T>> _setHandlers;
        private readonly Action<Aggregate<T>> _setObservers;

        public AggregateLoader(Action<Aggregate<T>> setHandlers, Action<Aggregate<T>> setObservers)
        {
            _setHandlers = setHandlers;
            _setObservers = setObservers;
        }

        public Aggregate<T> Create<TEvent>(Guid id, TEvent evt) where TEvent : DomainEvent
        {
            var aggregate = new Aggregate<T>(id);
            _setHandlers?.Invoke(aggregate);
            _setObservers?.Invoke(aggregate);
            aggregate.Update(evt);
            return aggregate;
        }

        public Aggregate<T> Load(Guid id)
        {
            var aggregate = new Aggregate<T>(id);
            _setHandlers?.Invoke(aggregate);
            _setObservers?.Invoke(aggregate);
            //aggregate.LoadFrom();
            return aggregate;
        }
    }
}