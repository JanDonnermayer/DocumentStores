using System;

namespace DocumentStores.Internal
{
    /// <summary>
    /// Used to hide the identy of an IObservable<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ObservableWrapper<T> : IObservable<T>
    {
        private readonly IObservable<T> inner;

        public ObservableWrapper(IObservable<T> inner) =>
            this.inner = inner;

        public IDisposable Subscribe(IObserver<T> observer) =>
            inner.Subscribe(observer);
    }

}