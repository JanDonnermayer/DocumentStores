using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentStores;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class ObservableDocumentStore<TData>
        : IDisposable,
        IObservableDocumentStore<TData> where TData : class


    {
        private readonly IDocumentStore source;
        private readonly IObserver<IEnumerable<string>> observer;
        private readonly IObservable<IEnumerable<string>> observable;
        private readonly IDisposable disposeHandle;

        public ObservableDocumentStore(IDocumentStore source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            var subject = new TaskPoolSubject<IEnumerable<string>>();
            observer = subject;
            observable = subject;
            disposeHandle = subject;
        }

        private async Task NotifyObserversAsync()
        {
            var keys = await source.GetKeysAsync<TData>();
            observer.OnNext(keys);
        }

        private async Task<T> WithNotification<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            finally
            {
                await NotifyObserversAsync();
            }
        }

        IObservable<IEnumerable<string>> IObservableDocumentStore<TData>.GetKeysObservable() =>
            this.observable;

        Task<Result<TData>> IObservableDocumentStore<TData>.AddOrUpdateDocumentAsync(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) =>
            source.AddOrUpdateDocumentAsync(
                key,
                (s) => WithNotification(addDataAsync(s)),
                (s, d) => WithNotification(updateDataAsync(s, d)));

        Task<Result<TData>> IObservableDocumentStore<TData>.GetOrAddDocumentAsync(
            string key,
            Func<string, Task<TData>> addDataAsync) =>
            source.GetOrAddDocumentAsync(
                key,
                (s) => WithNotification(addDataAsync(s)));

        Task<Result> IObservableDocumentStore<TData>.DeleteDocumentAsync(string key) =>
            WithNotification(source.DeleteDocumentAsync<TData>(key));

        Task<Result<TData>> IObservableDocumentStore<TData>.GetDocumentAsync(string key) =>
            source.GetDocumentAsync<TData>(key);

        Task<IEnumerable<string>> IObservableDocumentStore<TData>.GetKeysAsync() =>
            source.GetKeysAsync<TData>();

        Task<Result> IObservableDocumentStore<TData>.PutDocumentAsync(string key, TData data) =>
            WithNotification(source.PutDocumentAsync(key, data));

        public void Dispose() => disposeHandle.Dispose();
    }

}