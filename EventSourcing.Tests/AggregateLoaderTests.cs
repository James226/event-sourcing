using System;
using System.Reactive.Linq;
using FluentAssertions;
using Xunit;

namespace EventStore.Tests
{
    public class AggregateLoaderTests
    {
        public class WhenCreateIsInvoked
        {
            private readonly Aggregate<TestState> _aggregate;
            private readonly string _name = "Fred";
            private string _observedName;

            public WhenCreateIsInvoked()
            {
                var loader = new AggregateLoader<TestState>(SetHandlers, SetObservers);
                _aggregate = loader.Create(Guid.NewGuid(), new SetNameEvent {Name = _name});
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
            public void ThenTheObservedNameIsCorrect()
            {
                _observedName.Should().Be(_name);
            }
        }

        public class WhenLoadIsInvoked
        {
            private Aggregate<TestState> _aggregate;

            public WhenLoadIsInvoked()
            {
                var loader = new AggregateLoader<TestState>(SetHandlers, SetObservers);
                _aggregate = loader.Load(Guid.NewGuid());
            }

            private void SetHandlers(Aggregate<TestState> aggregate)
            {
                
            }

            private void SetObservers(Aggregate<TestState> aggregate)
            {
                
            }
        }
    }
}