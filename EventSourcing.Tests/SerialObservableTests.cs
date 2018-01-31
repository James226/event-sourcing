using System;
using Moq;
using Xunit;

namespace EventSourcing.Tests
{
    public class SerialObservableTests
    {
        public class ExtendedEvent : DomainEvent
        {
            
        }

        public class NonExtendedEvent : DomainEvent
        {
            
        }

        public class WhenOnNextIsCalled
        {
            private readonly Mock<IAsyncObserver<DomainEvent>> _observer;
            private readonly ExtendedEvent _event;
            private readonly Mock<IAsyncObserver<ExtendedEvent>> _extendedObserver;
            private readonly Mock<IAsyncObserver<NonExtendedEvent>> _nonExtendedObserver;

            public WhenOnNextIsCalled()
            {
                var observable = new AsyncSubject<DomainEvent>();
                _observer = new Mock<IAsyncObserver<DomainEvent>>();
                _extendedObserver = new Mock<IAsyncObserver<ExtendedEvent>>();
                _nonExtendedObserver = new Mock<IAsyncObserver<NonExtendedEvent>>();
                observable.Subscribe(_observer.Object);
                observable.OfType<ExtendedEvent>().Subscribe(_extendedObserver.Object);
                observable.OfType<NonExtendedEvent>().Subscribe(_nonExtendedObserver.Object);
                _event = new ExtendedEvent();
                observable.OnNext(_event).Wait();
            }

            [Fact]
            public void ThenTheObserverIsCalled()
            {
                _observer.Verify(o => o.OnNext(_event));
            }

            [Fact]
            public void ThenTheExtendedObserverIsCalled()
            {
                _extendedObserver.Verify(o => o.OnNext(_event));
            }

            [Fact]
            public void ThenTheNonExtendedObserverIsNotCalled()
            {
                _nonExtendedObserver.Verify(o => o.OnNext(It.IsAny<NonExtendedEvent>()), Times.Never);
            }
        }

        public class WhenOnCompleteIsCalled
        {
            private readonly Mock<IAsyncObserver<DomainEvent>> _observer;

            public WhenOnCompleteIsCalled()
            {
                var observable = new AsyncSubject<DomainEvent>();
                _observer = new Mock<IAsyncObserver<DomainEvent>>();
                observable.Subscribe(_observer.Object);
                observable.OnCompleted().Wait();
            }

            [Fact]
            public void ThenTheObserverIsCalled()
            {
                _observer.Verify(o => o.OnCompleted());
            }
        }

        public class WhenOnErrorIsCalled
        {
            private readonly Mock<IAsyncObserver<DomainEvent>> _observer;
            private readonly Exception _exception;

            public WhenOnErrorIsCalled()
            {
                var observable = new AsyncSubject<DomainEvent>();
                _observer = new Mock<IAsyncObserver<DomainEvent>>();
                observable.Subscribe(_observer.Object);
                _exception = new Exception();
                observable.OnError(_exception).Wait();
            }

            [Fact]
            public void ThenTheObserverIsCalled()
            {
                _observer.Verify(o => o.OnError(_exception));
            }
        }
    }
}