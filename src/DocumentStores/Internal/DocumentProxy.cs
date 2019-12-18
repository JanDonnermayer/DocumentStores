using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class DocumentProxy<TData>
        : IDocumentProxy<TData> where TData : class
    {
        private readonly IDocumentTopic<TData> topic;
        private readonly DocumentKey key;

        public DocumentProxy(IDocumentTopic<TData> topic, DocumentKey key)
        {
            this.topic = topic ?? throw new ArgumentNullException(nameof(topic));
            this.key = key;
        }

        Task<Result<TData>> IDocumentProxy<TData>.AddOrUpdateDocumentAsync(
            Func<Task<TData>> addDataAsync,
            Func<TData, Task<TData>> updateDataAsync) =>
                topic.AddOrUpdateDocumentAsync(
                    key: key,
                    addDataAsync: _ => addDataAsync(),
                    updateDataAsync: (_, data) => updateDataAsync(data));

        Task<Result<Unit>> IDocumentProxy<TData>.DeleteDocumentAsync() => 
            topic.DeleteDocumentAsync(key);

        Task<Result<TData>> IDocumentProxy<TData>.GetDocumentAsync() => 
            topic.GetDocumentAsync(key);

        Task<Result<TData>> IDocumentProxy<TData>.GetOrAddDocumentAsync(
            Func<Task<TData>> addDataAsync) => 
                topic.GetOrAddDocumentAsync(key, _ => addDataAsync());

        Task<Result<Unit>> IDocumentProxy<TData>.PutDocumentAsync(TData data) => 
            topic.PutDocumentAsync(key, data);

    }
}