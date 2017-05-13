using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IAsyncObserver<in T>
    {
        Task OnCompleted();

        Task OnError(Exception error);

        Task OnNext(T value);
    }
}