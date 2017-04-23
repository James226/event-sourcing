using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IEventStorage
    {
        Task Save(DomainEvent evt);
        Task<IEnumerable<DomainEvent>> Load();
    }
}