using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IEventStorage
    {
        Task Save(DomainEvent evt);
        Task<IEnumerable<DomainEvent>> Load();
    }
}