using System;

namespace EventSourcing
{
    public class DomainEvent
    {
        public string Id { get; set; }
        public int Version { get; set; }
    }
}