using System;
using System.Collections.Generic;

namespace EventSourcing
{
    public interface IHandlerSet<T>
    {
        void When<THandlerEvent>(Action<T, THandlerEvent> handler) where THandlerEvent : DomainEvent;
        Action<T, DomainEvent> GetHandler(Type type);
    }
    
    public class HandlerSet<T> : IHandlerSet<T>
    {
        private readonly Dictionary<Type, Action<T, DomainEvent>> _handlers = new Dictionary<Type, Action<T, DomainEvent>>();

        public void When<THandlerEvent>(Action<T, THandlerEvent> handler) where THandlerEvent : DomainEvent
        {
            _handlers[typeof(THandlerEvent)] = (state, e) => handler(state, (THandlerEvent)e);
        }

        public Action<T, DomainEvent> GetHandler(Type type)
        {
            return _handlers.TryGetValue(type, out var action) ? action : null;
        }
    }
}