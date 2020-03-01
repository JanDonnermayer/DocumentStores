using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

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

        Task<IResult<TData>> IDocumentChannel<TData>.AddOrUpdateAsync(
            Func<Task<TData>> addDataAsync,
            Func<TData, Task<TData>> updateDataAsync) =>
                topic.AddOrUpdateAsync(
                    key: key,
                    addDataAsync: _ => addDataAsync(),
                    updateDataAsync: (_, data) => updateDataAsync(data));

        Task<IResult<Unit>> IDocumentChannel<TData>.DeleteAsync() =>
            topic.DeleteAsync(key);

        Task<IResult<TData>> IDocumentChannel<TData>.GetAsync() =>
            topic.GetAsync(key);

        Task<IResult<TData>> IDocumentChannel<TData>.GetOrAddAsync(
            Func<Task<TData>> addDataAsync) =>
                topic.GetOrAddAsync(key, _ => addDataAsync());

        Task<IResult<Unit>> IDocumentChannel<TData>.PutAsync(TData data) =>
            topic.PutAsync(key, data);
    }
}