using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventSourcing.Tests
{
    public class AggregateLoaderTests
    {
        public class WhenCreateIsInvoked
        {
            private readonly Aggregate<TestState> _aggregate;
            private const string Name = "Fred";
            private readonly Mock<IEventStore> _eventStore;
            private readonly SetNameEvent _event;
            private string _observedName;

            public WhenCreateIsInvoked()
            {
                _eventStore = new Mock<IEventStore>();
                var loader = new AggregateLoader<TestState>(SetHandlers, SetObservers, _eventStore.Object);
                _event = new SetNameEvent {Name = Name};
                _aggregate = loader.Create(Guid.NewGuid(), _event);
            }

            private void SetHandlers(Aggregate<TestState> aggregate)
            {
                aggregate.When<SetNameEvent>((state, evt) => state.Name = evt.Name);
            }

            private void SetObservers(Aggregate<TestState> aggregate)
            {
                aggregate.Events.Subscribe<SetNameEvent>(e => Task.FromResult(_observedName = e.Name));
            }

            [Fact]
            public void ThenTheStateIsCorrect()
            {
                _aggregate.State.Name.Should().Be(Name);
            }

            [Fact]
            public void ThenTheEventIsStored()
            {
                _eventStore.Verify(s => s.Store(_event));
            }

            [Fact]
            public void ThenTheObservedNameIsCorrect()
            {
                _observedName.Should().Be(Name);
            }
        }

        public class WhenLoadIsInvoked
        {
            private const string SetName = "Bob";
            private readonly Aggregate<TestState> _aggregate;
            private readonly List<DomainEvent> _observedEvents;
            private readonly DomainEvent _raisedEvent;

            public WhenLoadIsInvoked()
            {
                _observedEvents = new List<DomainEvent>();
                var id = Guid.NewGuid();
                var eventStore = new Mock<IEventStore>();
                eventStore
                    .Setup(s => s.Load(id))
                    .ReturnsAsync(new[] { new SetNameEvent { Name = SetName } });

                var loader = new AggregateLoader<TestState>(SetHandlers, SetObservers, eventStore.Object);
                _aggregate = loader.Load(id).Result;
                _raisedEvent = new DomainEvent();
                _aggregate.Update(_raisedEvent).Wait();
            }

            private void SetHandlers(Aggregate<TestState> aggregate)
            {
                aggregate.When<DomainEvent>((s, e) => { });
                aggregate.When<SetNameEvent>((s, e) => s.Name = e.Name);
            }

            private void SetObservers(Aggregate<TestState> aggregate)
            {
                aggregate.Events.Subscribe(e =>
                {
                    _observedEvents.Add(e);
                    return Task.FromResult(0);
                });
            }

            [Fact]
            public void ThenTheStateIsCorrect()
            {
                _aggregate.State.Name.Should().Be(SetName);
            }

            [Fact]
            public void ThenTheObservedNameIsCorrect()
            {
                _observedEvents.Should().BeEquivalentTo(_raisedEvent);
            }
        }
    }
}