﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class DocumentTopic<TData>
        : IDisposable, IDocumentTopic<TData> where TData : class
    {
        private readonly IDocumentStore source;
        private readonly DocumentRoute route;
        private readonly IObserver<IEnumerable<DocumentKey>> observer;
        private readonly IObservable<IEnumerable<DocumentKey>> observable;
        private readonly IDisposable disposeHandle;


        public DocumentTopic(IDocumentStore source, DocumentRoute route)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.route = route;
            var subject = new TaskPoolBehaviourSubject<IEnumerable<DocumentKey>>(initial: GetKeysInternalAsync().Result);

            observer = subject;
            observable = subject;
            disposeHandle = subject;
        }

        private async Task<IEnumerable<DocumentKey>> GetKeysInternalAsync()
        {
            var addresses = await source
                .GetAddressesAsync<TData>(
                    route: route,
                    options: DocumentSearchOptions.TopLevelOnly,
                    ct: new CancellationTokenSource(2000).Token)
                .ConfigureAwait(false);
            return addresses.Select(_ => _.Key);
        }

        private async Task NotifyObserversAsync() => 
            observer.OnNext(await GetKeysInternalAsync());

        private async Task<T> WithNotification<T>(Task<T> task)
        {
            var res = await task.ConfigureAwait(false);
            _ = NotifyObserversAsync(); // Non blocking
            return res;
        }

        IObservable<IEnumerable<DocumentKey>> IDocumentTopic<TData>.GetKeysObservable() =>
            this.observable;

        async Task<Result<TData>> IDocumentTopic<TData>.AddOrUpdateDocumentAsync(
              DocumentKey key,
              Func<string, Task<TData>> addDataAsync,
              Func<string, TData, Task<TData>> updateDataAsync) =>
                await source.AddOrUpdateDocumentAsync(
                    route.ToAddress(key),
                    (s) => WithNotification(addDataAsync(s)),
                    (s, d) => WithNotification(updateDataAsync(s, d)))
                    .ConfigureAwait(false);

        async Task<Result<TData>> IDocumentTopic<TData>.GetOrAddDocumentAsync(
              DocumentKey key,
              Func<string, Task<TData>> addDataAsync) =>
                await source.GetOrAddDocumentAsync(
                    route.ToAddress(key),
                    (s) => WithNotification(addDataAsync(s)))
                    .ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentTopic<TData>.DeleteDocumentAsync(DocumentKey key) =>
            await WithNotification(source.DeleteDocumentAsync<TData>(route.ToAddress(key))).ConfigureAwait(false);

        async Task<Result<TData>> IDocumentTopic<TData>.GetDocumentAsync(DocumentKey key) =>
            await source.GetDocumentAsync<TData>(route.ToAddress(key)).ConfigureAwait(false);

        async Task<IEnumerable<DocumentKey>> IDocumentTopic<TData>.GetKeysAsync() =>
            await GetKeysInternalAsync().ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentTopic<TData>.PutDocumentAsync(DocumentKey key, TData data) =>
            await WithNotification(source.PutDocumentAsync(route.ToAddress(key), data)).ConfigureAwait(false);

        public void Dispose() => disposeHandle.Dispose();
    }

}