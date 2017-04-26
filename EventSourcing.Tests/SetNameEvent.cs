namespace EventSourcing.Tests
{
    public class SetNameEvent : DomainEvent
    {
        public string Name { get; set; }
    }
}