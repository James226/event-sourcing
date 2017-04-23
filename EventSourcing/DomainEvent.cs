using System;

namespace EventStore
{
    public class DomainEvent
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
    }
}