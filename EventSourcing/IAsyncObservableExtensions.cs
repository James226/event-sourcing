namespace EventSourcing
{
    public static class AsyncObservableExtensions
    {
        public static IAsyncObservable<T> OfType<T>(this IAsyncObservable<object> observable)
        {
            var obs = new TypeSubject<T, object>();
            observable.Subscribe(obs);
            return obs;
        }
    }
}