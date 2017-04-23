using System;
using FluentAssertions;
using Xunit;

namespace EventStore.Tests
{
    public class AggregateTests
    {
        public class WhenAggregateIsCreated
        {
            private readonly Aggregate<TestState> _aggregate;
            private readonly Guid _id;

            public WhenAggregateIsCreated()
            {
                _id = Guid.NewGuid();
                _aggregate = new Aggregate<TestState>(_id);
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
                _aggregate = new Aggregate<TestState>(Guid.NewGuid());
                _aggregate.When<SetNameEvent>((state, evt) => state.Name = evt.Name);
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
                _aggregate = new Aggregate<TestState>(Guid.NewGuid());
                _aggregate.When<SetNameEvent>((state, evt) => state.Name = evt.Name);
                _aggregate.Events.Subscribe(e => _receivedEvent = e);
                _name = "John";
                _eventRaised = new SetNameEvent {Name = _name};
                _aggregate.Update(_eventRaised);
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
                _aggregate = new Aggregate<TestState>(Guid.NewGuid());
                _aggregate.When<SetNameEvent>((state, evt) => state.Name = evt.Name);
                _aggregate.Events.Subscribe(onNext: e => throw new InvalidOperationException(), onError: _ => _errorRaised = true);
                try
                {
                    _aggregate.Update(new SetNameEvent { Name = "John" });
                }
                catch (Exception e)
                {
                    _thrownException = e;
                }
            }

            [Fact]
            public void ThenTheExceptionIsRethrown()
            {
                _thrownException.Should().BeOfType<InvalidOperationException>();
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
                var aggregate = new Aggregate<TestState>(Guid.NewGuid());
                aggregate.Events.Subscribe(onNext: _ => { }, onCompleted: () => { _completeRaised = true; });
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