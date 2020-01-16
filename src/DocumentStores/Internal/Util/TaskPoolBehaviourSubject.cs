using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace DocumentStores.Internal
{
    /// <summary>
    /// A Subject{T} that enqueues notifictaions on the taskpool 
    /// and replays the latest notification.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TaskPoolBehaviourSubject<T>
        : IObservable<T>, IObserver<T>, IDisposable
    {
        private ImmutableHashSet<IObserver<T>> observers =
            ImmutableHashSet<IObserver<T>>.Empty;

        public TaskPoolBehaviourSubject(T initial) =>
            Latest = initial;

        private T Latest { get; set; }

        private void ReleaseObservers() =>
            ImmutableInterlocked.Update(ref observers, _ => _.Clear());

        private void PostForEachObserver(Action<IObserver<T>> action)
        {
            foreach (var _ in observers) Task.Run(() => action(_));
        }


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
            Latest = value;
            PostForEachObserver(_ => _.OnNext(value));
        }

        IDisposable IObservable<T>.Subscribe(IObserver<T> observer)
        {
            if (observer is null) throw new ArgumentNullException(nameof(observer));

            ImmutableInterlocked.Update(ref observers, _ => _.Add(observer));
            observer.OnNext(Latest);
            return Disposable.Create(() => ImmutableInterlocked.Update(ref observers, _ => _.Remove(observer)));
        }
        public void Dispose() => ReleaseObservers();

    }

}