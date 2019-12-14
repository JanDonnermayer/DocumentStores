using System;
using System.Collections.Generic;
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
        private readonly DocumentTopicName topicName;
        private readonly IObserver<IEnumerable<DocumentKey>> observer;
        private readonly IObservable<IEnumerable<DocumentKey>> observable;
        private readonly IDisposable disposeHandle;


        public DocumentTopic(IDocumentStore source, DocumentTopicName name)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.topicName = name;
            var subject = new TaskPoolBehaviourSubject<IEnumerable<DocumentKey>>(initial:
                source.GetKeysAsync<TData>(name, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).Result);
            observer = subject;
            observable = subject;
            disposeHandle = subject;
        }

        private async Task NotifyObserversAsync()
        {
            var keys = await source.GetKeysAsync<TData>(topicName).ConfigureAwait(false);
            observer.OnNext(keys);
        }

        private async Task<T> WithNotification<T>(Task<T> task)
        {
            var res = await task.ConfigureAwait(false);
            _ = NotifyObserversAsync(); // Non blocking
            return res;
        }

        IObservable<IEnumerable<DocumentKey>> IDocumentTopic<TData>.GetKeysObservable() =>
            this.observable;

        async Task<Result<TData>> IDocumentTopic<TData>.AddOrUpdateDocumentAsync(
              string key,
              Func<string, Task<TData>> addDataAsync,
              Func<string, TData, Task<TData>> updateDataAsync) =>
              await source.AddOrUpdateDocumentAsync(
                  topicName.CreateKey(key),
                  (s) => WithNotification(addDataAsync(s)),
                  (s, d) => WithNotification(updateDataAsync(s, d)))
                  .ConfigureAwait(false);

        async Task<Result<TData>> IDocumentTopic<TData>.GetOrAddDocumentAsync(
              string key,
              Func<string, Task<TData>> addDataAsync) =>
              await source.GetOrAddDocumentAsync(
                  topicName.CreateKey(key),
                  (s) => WithNotification(addDataAsync(s)))
                  .ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentTopic<TData>.DeleteDocumentAsync(string key) =>
            await WithNotification(source.DeleteDocumentAsync<TData>(topicName.CreateKey(key))).ConfigureAwait(false);

        async Task<Result<TData>> IDocumentTopic<TData>.GetDocumentAsync(string key) =>
            await source.GetDocumentAsync<TData>(topicName.CreateKey(key)).ConfigureAwait(false);

        async Task<IEnumerable<DocumentKey>> IDocumentTopic<TData>.GetKeysAsync() =>
            await source.GetKeysAsync<TData>(topicName).ConfigureAwait(false);

        async Task<Result<Unit>> IDocumentTopic<TData>.PutDocumentAsync(string key, TData data) =>
            await WithNotification(source.PutDocumentAsync(topicName.CreateKey(key), data)).ConfigureAwait(false);

        public void Dispose() => disposeHandle.Dispose();
    }

}