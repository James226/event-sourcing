using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IAsyncObservable<T>
    {
        IDisposable Subscribe<TU>(IAsyncObserver<TU> observer) where TU : class, T;
        IDisposable Subscribe(Func<T, Task> onNext);
        IDisposable Subscribe<TU>(Func<TU, Task> onNext) where TU : class, T;
    }
}