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

        Task<Result<TData>> IDocumentProxy<TData>.AddOrUpdateAsync(
            Func<Task<TData>> addDataAsync,
            Func<TData, Task<TData>> updateDataAsync) =>
                topic.AddOrUpdateAsync(
                    key: key,
                    addDataAsync: _ => addDataAsync(),
                    updateDataAsync: (_, data) => updateDataAsync(data));

        Task<Result<Unit>> IDocumentProxy<TData>.DeleteAsync() => 
            topic.DeleteAsync(key);

        Task<Result<TData>> IDocumentProxy<TData>.GetAsync() => 
            topic.GetAsync(key);

        Task<Result<TData>> IDocumentProxy<TData>.GetOrAddAsync(
            Func<Task<TData>> addDataAsync) => 
                topic.GetOrAddAsync(key, _ => addDataAsync());

        Task<Result<Unit>> IDocumentProxy<TData>.PutAsync(TData data) => 
            topic.PutAsync(key, data);

    }
}