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
                var handlerSet = new Mock<IHandlerSet<TestState>>();
                var loader = new AggregateLoader<TestState>(handlerSet.Object, SetObservers, _eventStore.Object);
                _event = new SetNameEvent {Name = Name};
                _aggregate = loader.Create(Guid.NewGuid().ToString(), _event);
            }

            private void SetObservers(IAsyncObservable<DomainEvent> aggregate, TestState state)
            {
                aggregate.OfType<SetNameEvent>().Subscribe(new AsyncObserver<SetNameEvent>(e => Task.FromResult(_observedName = e.Name)));
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
                var id = Guid.NewGuid().ToString();
                var eventStore = new Mock<IEventStore>();
                eventStore
                    .Setup(s => s.Load(id))
                    .ReturnsAsync(new[] { new SetNameEvent { Name = SetName } });

                var handlers = new Mock<IHandlerSet<TestState>>();
                var loader = new AggregateLoader<TestState>(handlers.Object, SetObservers, eventStore.Object);
                _aggregate = loader.Load(id).Result;
                _raisedEvent = new DomainEvent();
                _aggregate.Update(_raisedEvent).Wait();
            }

            private void SetObservers(IAsyncObservable<DomainEvent> aggregate, TestState state)
            {
                aggregate.Subscribe(new AsyncObserver<DomainEvent>(e =>
                {
                    _observedEvents.Add(e);
                    return Task.FromResult(0);
                }));
            }

            [Fact]
            public void ThenTheObservedNameIsCorrect()
            {
                _observedEvents.Should().BeEquivalentTo(_raisedEvent);
            }
        }
    }
}