using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventSourcing.Tests
{
    public class AggregateTests
    {
        public class WhenAggregateIsCreated
        {
            private readonly Aggregate<TestState> _aggregate;
            private readonly string _id;

            public WhenAggregateIsCreated()
            {
                _id = Guid.NewGuid().ToString();
                _aggregate = new Aggregate<TestState>(Mock.Of<IHandlerSet<TestState>>(), _id);
            }

            [Fact]
            public void ThenTheAggregateStateIsCreated()
            {
                _aggregate.State.Should().NotBeNull();
            }

            [Fact]
            public void ThenTheIdIsSet()
            {
                _aggregate.Id.Should().Be(_id);
            }
        }

        public class WhenLoadFromIsInvoked
        {
            private readonly string _name;
            private readonly Aggregate<TestState> _aggregate;

            public WhenLoadFromIsInvoked()
            {
                _name = "Bob";
                var handlerSet = new HandlerSet<TestState>();
                handlerSet.When<SetNameEvent>((state, evt) => state.Name = evt.Name);
                
                _aggregate = new Aggregate<TestState>(handlerSet, Guid.NewGuid().ToString());
                _aggregate.LoadFrom(new[] { new SetNameEvent { Name = _name } });
            }

            [Fact]
            public void ThenTheStateIsRehydrated()
            {
                _aggregate.State.Name.Should().Be(_name);
            }
        }

        public class WhenAnEventIsAdded
        {
            private readonly Aggregate<TestState> _aggregate;
            private readonly string _name;
            private readonly SetNameEvent _eventRaised;
            private DomainEvent _receivedEvent;

            public WhenAnEventIsAdded()
            {
                var handlers = new HandlerSet<TestState>();
                handlers.When<SetNameEvent>((state, evt) => state.Name = evt.Name);
                
                _aggregate = new Aggregate<TestState>(handlers, Guid.NewGuid().ToString());
                _aggregate.Events.OfType<SetNameEvent>().Subscribe(new AsyncObserver<SetNameEvent>(e =>
                {
                    _receivedEvent = e;
                    return Task.FromResult(0);
                }));

                _name = "John";
                _eventRaised = new SetNameEvent {Name = _name};
                _aggregate.Update(_eventRaised).Wait();
            }

            [Fact]
            public void ThenTheStateIsHydrated()
            {
                _aggregate.State.Name.Should().Be(_name);
            }

            [Fact]
            public void ThenTheEventsObservableShouldBeFired()
            {
                _receivedEvent.Should().BeSameAs(_eventRaised);
            }

            [Fact]
            public void ThenTheIdShouldBeSetOnTheEvent()
            {
                _receivedEvent.Id.Should().Be(_aggregate.Id);
            }

            [Fact]
            public void ThenTheVersionShouldBeSet()
            {
                _receivedEvent.Version.Should().Be(1);
            }

            [Fact]
            public void ThenTheAggregateVersionShouldBeSet()
            {
                _aggregate.Version.Should().Be(1);
            }
        }

        public class WhenAnObserverThrows
        {
            private readonly Aggregate<TestState> _aggregate;
            private readonly Exception _thrownException;
            private bool _errorRaised;

            public WhenAnObserverThrows()
            {
                var handlers = new HandlerSet<TestState>();
                handlers.When<SetNameEvent>((state, evt) => state.Name = evt.Name);

                _aggregate = new Aggregate<TestState>(handlers, Guid.NewGuid().ToString());
                _aggregate.Events.Subscribe(new AsyncObserver<DomainEvent>(
                    e => throw new InvalidOperationException(), 
                    _ => Task.FromResult(_errorRaised = true),
                    () => Task.FromResult(0)));
                try
                {
                    _aggregate.Update(new SetNameEvent { Name = "John" }).Wait();
                }
                catch (Exception e)
                {
                    _thrownException = e;
                }
            }

            [Fact]
            public void ThenTheExceptionIsRethrown()
            {
                _thrownException.InnerException.Should().BeOfType<InvalidOperationException>();
            }

            [Fact]
            public void ThenOnErrorShouldBeRaised()
            {
                _errorRaised.Should().BeTrue();
            }

            [Fact]
            public void ThenTheVersionIsNotIncremented()
            {
                _aggregate.Version.Should().Be(0);
            }
        }

        public class WhenDisposeIsInvoked
        {
            private bool _completeRaised;

            public WhenDisposeIsInvoked()
            {
                var aggregate = new Aggregate<TestState>(Mock.Of<IHandlerSet<TestState>>(), Guid.NewGuid().ToString());
                aggregate.Events.Subscribe(new AsyncObserver<DomainEvent>(_ => Task.FromResult(0), _ => Task.FromResult(0), () => Task.FromResult(_completeRaised = true)));
                aggregate.Dispose();
            }

            [Fact]
            public void ThenTheEventsObserverIsComplete()
            {
                _completeRaised.Should().BeTrue();
            }
        }
    }
}