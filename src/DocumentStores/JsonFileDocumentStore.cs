using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using DocumentStores.Internal;
using System.Collections.Generic;
using System.Threading;

namespace DocumentStores
{
    /// <summary/> 
    public sealed class JsonFileDocumentStore : IDocumentStore
    {
        private readonly IDocumentStore documentStore;

        /// <summary/> 
        public JsonFileDocumentStore(string directory)
        {
            var handling = new JsonFileHandling();
            this.documentStore = new FileDocumentStore(directory, handling);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            string key, Func<string, Task<TData>> addDataAsync, 
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class
        {
            return documentStore.AddOrUpdateDocumentAsync(key, addDataAsync, updateDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> DeleteDocumentAsync<TData>(string key) where TData : class
        {
            return documentStore.DeleteDocumentAsync<TData>(key);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetDocumentAsync<TData>(string key) where TData : class
        {
            return documentStore.GetDocumentAsync<TData>(key);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> GetKeysAsync<TData>(CancellationToken ct = default) where TData : class
        {
            return documentStore.GetKeysAsync<TData>(ct);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetOrAddDocumentAsync<TData>(string key, 
            Func<string, Task<TData>> addDataAsync) where TData : class
        {
            return documentStore.GetOrAddDocumentAsync(key, addDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> PutDocumentAsync<TData>(string key, TData data) where TData : class
        {
            return documentStore.PutDocumentAsync<TData>(key, data);
        }
    }
}
