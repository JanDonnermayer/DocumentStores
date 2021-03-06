﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores;

namespace DocumentStores.Internal
{
    internal class DocumentTopic<TData>
        : IDisposable, IDocumentTopic<TData> where TData : class
    {
        private readonly IDocumentStore store;
        private readonly DocumentRoute route;
        private readonly IObserver<IEnumerable<DocumentKey>> observer;
        private readonly IObservable<IEnumerable<DocumentKey>> observable;
        private readonly IDisposable disposeHandle;

        public DocumentTopic(IDocumentStore source, DocumentRoute route)
        {
            this.store = source ?? throw new ArgumentNullException(nameof(source));
            this.route = route;
            var subject = new TaskPoolBehaviorSubject<IEnumerable<DocumentKey>>(initial: GetKeysInternalAsync().Result);

            observer = subject;
            observable = subject;
            disposeHandle = subject;
        }

        private static DocumentAddress Combine(DocumentRoute route, DocumentKey key) =>
            DocumentAddress.Create(route, key);

        private async Task<IEnumerable<DocumentKey>> GetKeysInternalAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var addresses = await store
                .GetAddressesAsync<TData>(
                    route: route,
                    options: DocumentSearchOption.TopLevelOnly,
                    ct: cts.Token)
                .ConfigureAwait(false);
            return addresses.Select(_ => _.Key);
        }

        private async Task NotifyObserversAsync() =>
            observer.OnNext(await GetKeysInternalAsync().ConfigureAwait(false));

        private async Task<T> WithNotification<T>(Task<T> task)
        {
            var res = await task.ConfigureAwait(false);
            _ = NotifyObserversAsync(); // Non blocking
            return res;
        }

        IObservable<IEnumerable<DocumentKey>> IDocumentTopic<TData>.GetKeysObservable() =>
            this.observable;

        async Task<IResult<TData>> IDocumentTopic<TData>.AddOrUpdateAsync(
              DocumentKey key,
              Func<DocumentKey, Task<TData>> addDataAsync,
              Func<DocumentKey, TData, Task<TData>> updateDataAsync) =>
                await WithNotification(
                    store.AddOrUpdateAsync(
                        address: Combine(route, key),
                        addDataAsync: (s) => addDataAsync(s.Key),
                        updateDataAsync: (s, d) => updateDataAsync(s.Key, d)
                    )
                ).ConfigureAwait(false);

        async Task<IResult<TData>> IDocumentTopic<TData>.GetOrAddAsync(
              DocumentKey key,
              Func<DocumentKey, Task<TData>> addDataAsync) =>
                await WithNotification(
                    store.GetOrAddAsync(
                        address: Combine(route, key),
                        addDataAsync: (s) => addDataAsync(s.Key)
                    )
                ).ConfigureAwait(false);

        async Task<IResult<Unit>> IDocumentTopic<TData>.DeleteAsync(DocumentKey key) =>
            await WithNotification(store.DeleteAsync<TData>(Combine(route, key))).ConfigureAwait(false);

        async Task<IResult<TData>> IDocumentTopic<TData>.GetAsync(DocumentKey key) =>
            await store.GetAsync<TData>(Combine(route, key)).ConfigureAwait(false);

        async Task<IEnumerable<DocumentKey>> IDocumentTopic<TData>.GetKeysAsync() =>
            await GetKeysInternalAsync().ConfigureAwait(false);

        async Task<IResult<Unit>> IDocumentTopic<TData>.PutAsync(DocumentKey key, TData data) =>
            await WithNotification(store.PutAsync(Combine(route, key), data)).ConfigureAwait(false);

        public void Dispose() => disposeHandle.Dispose();
    }

}