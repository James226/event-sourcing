using System;
using System.Collections.Generic;
using System.Reactive.Linq;
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
            private readonly string _name = "Fred";
            private string _observedName;
            private Mock<IEventStore> _eventStore;
            private SetNameEvent _event;

            public WhenCreateIsInvoked()
            {
                _eventStore = new Mock<IEventStore>();
                var loader = new AggregateLoader<TestState>(SetHandlers, SetObservers, _eventStore.Object);
                _event = new SetNameEvent {Name = _name};
                _aggregate = loader.Create(Guid.NewGuid(), _event);
            }

            private void SetHandlers(Aggregate<TestState> aggregate)
            {
                aggregate.When<SetNameEvent>((state, evt) => state.Name = evt.Name);
            }

            private void SetObservers(Aggregate<TestState> aggregate)
            {
                aggregate.Events.OfType<SetNameEvent>().Subscribe(e => _observedName = e.Name);
            }

            [Fact]
            public void ThenTheStateIsCorrect()
            {
                _aggregate.State.Name.Should().Be(_name);
            }

            [Fact]
            public void ThenTheEventIsStored()
            {
                _eventStore.Verify(s => s.Store(_event));
            }

            [Fact]
            public void ThenTheObservedNameIsCorrect()
            {
                _observedName.Should().Be(_name);
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
                _aggregate.Update(_raisedEvent);
            }

            private void SetHandlers(Aggregate<TestState> aggregate)
            {
                aggregate.When<DomainEvent>((s, e) => { });
                aggregate.When<SetNameEvent>((s, e) => s.Name = e.Name);
            }

            private void SetObservers(Aggregate<TestState> aggregate)
            {
                aggregate.Events.Subscribe(e => _observedEvents.Add(e));
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