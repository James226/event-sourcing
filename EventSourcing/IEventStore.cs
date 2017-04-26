using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IEventStore
    {
        Task Store(DomainEvent evt);
        Task<IEnumerable<DomainEvent>> Load(Guid id);
    }
}