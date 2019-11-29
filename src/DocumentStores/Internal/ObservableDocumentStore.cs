using System;
using System.Collections.Generic;
using System.Threading;
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
            var subject = new TaskPoolBehaviourSubject<IEnumerable<string>>(initial:
                source.GetKeysAsync<TData>(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).Result);
            observer = subject;
            observable = subject;
            disposeHandle = subject;
        }

        private async Task NotifyObserversAsync()
        {
            var keys = await source.GetKeysAsync<TData>();
            observer.OnNext(keys);
        }

        private Task<T> WithNotification<T>(Task<T> task)
        {
            _ = NotifyObserversAsync();
            return task;
        }

        IObservable<IEnumerable<string>> IObservableDocumentStore<TData>.GetKeysObservable() =>
            this.observable;

        async Task<Result<TData>> IObservableDocumentStore<TData>.AddOrUpdateDocumentAsync(
              string key,
              Func<string, Task<TData>> addDataAsync,
              Func<string, TData, Task<TData>> updateDataAsync) =>
              await source.AddOrUpdateDocumentAsync(
                  key,
                  (s) => WithNotification(addDataAsync(s)),
                  (s, d) => WithNotification(updateDataAsync(s, d)))
                  .ConfigureAwait(false);

        async Task<Result<TData>> IObservableDocumentStore<TData>.GetOrAddDocumentAsync(
              string key,
              Func<string, Task<TData>> addDataAsync) =>
              await source.GetOrAddDocumentAsync(
                  key,
                  (s) => WithNotification(addDataAsync(s)))
                  .ConfigureAwait(false);

        async Task<Result<Unit>> IObservableDocumentStore<TData>.DeleteDocumentAsync(string key) =>
            await WithNotification(source.DeleteDocumentAsync<TData>(key)).ConfigureAwait(false);

        async Task<Result<TData>> IObservableDocumentStore<TData>.GetDocumentAsync(string key) =>
            await source.GetDocumentAsync<TData>(key).ConfigureAwait(false);

        async Task<IEnumerable<string>> IObservableDocumentStore<TData>.GetKeysAsync() =>
            await source.GetKeysAsync<TData>().ConfigureAwait(false);

        async Task<Result<Unit>> IObservableDocumentStore<TData>.PutDocumentAsync(string key, TData data) =>
            await WithNotification(source.PutDocumentAsync(key, data)).ConfigureAwait(false);

        public void Dispose() => disposeHandle.Dispose();
    }

}