using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    /// <summary>
    /// A Subject<T> that enqueues notifictaions on the taskpool.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TaskPoolSubject<T>
        : IObservable<T>, IObserver<T>, IDisposable
    {
        private ImmutableHashSet<IObserver<T>> observers =
            ImmutableHashSet<IObserver<T>>.Empty;

        private void ReleaseObservers() =>
            ImmutableInterlocked.Update(ref observers, _ => _.Clear());

        private void PostForEachObserver(Action<IObserver<T>> action) =>
            Task.Run(() => { foreach (var _ in observers) action(_); });

        void IObserver<T>.OnCompleted()
        {
            PostForEachObserver(_ => _.OnCompleted());
            ReleaseObservers();
        }

        void IObserver<T>.OnError(Exception error)
        {
            if (error is null) throw new ArgumentNullException(nameof(error));
            
            PostForEachObserver(_ => _.OnError(error));
            ReleaseObservers();
        }

        void IObserver<T>.OnNext(T value)
        {
            PostForEachObserver(_ => _.OnNext(value));
        }

        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));

            ImmutableInterlocked.Update(ref observers, _ => _.Add(observer));
            return Disposable.Create(() => ImmutableInterlocked.Update(ref observers, _ => _.Remove(observer)));
        }
        public void Dispose() => ReleaseObservers();

    }

}