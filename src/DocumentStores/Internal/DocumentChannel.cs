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
        private readonly IDocumentTopic<TData> store;
        private readonly DocumentKey key;

        public DocumentChannel(IDocumentTopic<TData> store, DocumentKey key)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.key = key;
        }

        Task<Result<TData>> IDocumentChannel<TData>.AddOrUpdateDocumentAsync(
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) =>
                store.AddOrUpdateDocumentAsync(key, addDataAsync, updateDataAsync);

        Task<Result<Unit>> IDocumentChannel<TData>.DeleteDocumentAsync() => 
            store.DeleteDocumentAsync(key);

        Task<Result<TData>> IDocumentChannel<TData>.GetDocumentAsync() => 
            store.GetDocumentAsync(key);

        Task<Result<TData>> IDocumentChannel<TData>.GetOrAddDocumentAsync(
            Func<string, Task<TData>> addDataAsync) => 
                store.GetOrAddDocumentAsync(key, addDataAsync);

        Task<Result<Unit>> IDocumentChannel<TData>.PutDocumentAsync(TData data) => 
            store.PutDocumentAsync(key, data);

        IObservable<TData> IDocumentChannel<TData>.GetObservable() => 
            store
                .GetAddressesObservable()
                .Where(addr => addr.Select(a => a.Key).Contains(key))
                .Select(_ => Observable.FromAsync(
                        () => store.GetDocumentAsync(key)))
                .Merge()
                .Where(o => o.Try())
                .Select(o => (TData)o);
    }


}