using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IAsyncObservable<out T>
    {
        IDisposable Subscribe(IAsyncObserver<T> subscriber);
    }
}