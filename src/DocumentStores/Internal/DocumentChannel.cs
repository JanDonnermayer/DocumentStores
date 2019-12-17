using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class DocumentChannel<TData>
        : IDocumentChannel<TData> where TData : class
    {
        private readonly IDocumentTopic<TData> topic;
        private readonly DocumentKey key;

        public DocumentChannel(IDocumentTopic<TData> topic, DocumentKey key)
        {
            this.topic = topic ?? throw new ArgumentNullException(nameof(topic));
            this.key = key;
        }

        Task<Result<TData>> IDocumentChannel<TData>.AddOrUpdateDocumentAsync(
            Func<Task<TData>> addDataAsync,
            Func<TData, Task<TData>> updateDataAsync) =>
                topic.AddOrUpdateDocumentAsync(
                    key: key,
                    addDataAsync: _ => addDataAsync(),
                    updateDataAsync: (_, data) => updateDataAsync(data));

        Task<Result<Unit>> IDocumentChannel<TData>.DeleteDocumentAsync() => 
            topic.DeleteDocumentAsync(key);

        Task<Result<TData>> IDocumentChannel<TData>.GetDocumentAsync() => 
            topic.GetDocumentAsync(key);

        Task<Result<TData>> IDocumentChannel<TData>.GetOrAddDocumentAsync(
            Func<Task<TData>> addDataAsync) => 
                topic.GetOrAddDocumentAsync(key, _ => addDataAsync());

        Task<Result<Unit>> IDocumentChannel<TData>.PutDocumentAsync(TData data) => 
            topic.PutDocumentAsync(key, data);

    }
}