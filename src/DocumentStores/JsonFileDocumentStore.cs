using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using DocumentStores.Internal;
using System.Collections.Generic;
using System.Threading;

namespace DocumentStores
{


    public class JsonFileDocumentStore : IDocumentStore
    {
        private readonly IFileHandling fileHandling;
        private readonly IResultHandling resultHandling;
        private readonly IDocumentStoreInternal store;

        public JsonFileDocumentStore(string directory)
        {
            var handling = new JsonHandling();
            this.fileHandling = handling;
            this.resultHandling = handling;
            this.store = new FileDocumentStoreInternal(directory, fileHandling);  
        }


        #region Implementation of IDocumentStore

        public Task<IEnumerable<string>> GetKeysAsync<TData>(CancellationToken ct = default) =>
            store.GetKeysAsync<TData>(ct);

        public Task<Result<T>> AddOrUpdateDocumentAsync<T>(string key,
            Func<string, Task<T>> addDataAsync, Func<string, T, Task<T>> updateDataAsync) where T : class =>
                Function.ApplyArgs(store.AddOrUpdateDocumentAsync, key, addDataAsync, updateDataAsync)
                        .Init(resultHandling.Catch<T>())
                        .Pipe(resultHandling.Retry<T>())
                        .Invoke();

        public Task<Result<T>> GetOrAddDocumentAsync<T>(string key,
            Func<string, Task<T>> addDataAsync) where T : class =>
                Function.ApplyArgs(store.GetOrAddDocumentAsync, key, addDataAsync)
                        .Init(resultHandling.Catch<T>())
                        .Pipe(resultHandling.Retry<T>())
                        .Invoke();

        public Task<Result<T>> GetDocumentAsync<T>(string key) where T : class =>
            Function.ApplyArgs(store.GetDocumentAsync<T>, key)
                    .Init(resultHandling.Catch<T>())
                    .Pipe(resultHandling.Retry<T>())
                    .Invoke();

        public Task<Result<Unit>> DeleteDocumentAsync<T>(string key) where T : class =>
            Function.ApplyArgs(store.DeleteDocumentAsync<T>, key)
                    .Init(resultHandling.Catch<Unit>())
                    .Pipe(resultHandling.Retry<Unit>())
                    .Invoke();

        public Task<Result<Unit>> PutDocumentAsync<T>(string key, T data) where T : class =>
            Function.ApplyArgs<string, T, Task<Unit>>(store.PutDocumentAsync, key, data)
                    .Init(resultHandling.Catch<Unit>())
                    .Pipe(resultHandling.Retry<Unit>())
                    .Invoke();

        #endregion
    }

}
