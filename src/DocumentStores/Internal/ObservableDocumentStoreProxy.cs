using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class ObservableDocumentStoreProxy<TData>
        : IObservableDocumentStoreProxy<TData> where TData : class
    {
        private readonly IObservableDocumentStore<TData> store;
        private readonly string key;

        public ObservableDocumentStoreProxy(IObservableDocumentStore<TData> store, string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.key = key;
        }

        Task<Result<TData>> IObservableDocumentStoreProxy<TData>.AddOrUpdateDocumentAsync(
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) =>
                store.AddOrUpdateDocumentAsync(key, addDataAsync, updateDataAsync);

        Task<Result<Unit>> IObservableDocumentStoreProxy<TData>.DeleteDocumentAsync() => 
            store.DeleteDocumentAsync(key);

        Task<Result<TData>> IObservableDocumentStoreProxy<TData>.GetDocumentAsync() => 
            store.GetDocumentAsync(key);

        Task<Result<TData>> IObservableDocumentStoreProxy<TData>.GetOrAddDocumentAsync(
            Func<string, Task<TData>> addDataAsync) => 
                store.GetOrAddDocumentAsync(key, addDataAsync);

        Task<Result<Unit>> IObservableDocumentStoreProxy<TData>.PutDocumentAsync(TData data) => 
            store.PutDocumentAsync(key, data);

        IObservable<TData> IObservableDocumentStoreProxy<TData>.GetObservable() => 
            store
                .GetKeysObservable()
                .Where(keys => keys.Contains(key))
                .Select(_ => Observable.FromAsync(
                        () => store.GetDocumentAsync(key)))
                .Merge()
                .Where(o => o.Try())
                .Select(o => o.PassOrThrow());
    }


}