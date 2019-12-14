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
            DocumentKey key, Func<string, Task<TData>> addDataAsync, 
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class
        {
            return documentStore.AddOrUpdateDocumentAsync(key, addDataAsync, updateDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> DeleteDocumentAsync<TData>(DocumentKey key) where TData : class
        {
            return documentStore.DeleteDocumentAsync<TData>(key);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetDocumentAsync<TData>(DocumentKey key) where TData : class
        {
            return documentStore.GetDocumentAsync<TData>(key);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<DocumentKey>> GetKeysAsync<TData>(
            DocumentTopicName topicName, 
            CancellationToken ct = default) where TData : class
        {
            return documentStore.GetKeysAsync<TData>(topicName, ct);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetOrAddDocumentAsync<TData>(DocumentKey key, 
            Func<string, Task<TData>> addDataAsync) where TData : class
        {
            return documentStore.GetOrAddDocumentAsync(key, addDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> PutDocumentAsync<TData>(DocumentKey key, TData data) where TData : class
        {
            return documentStore.PutDocumentAsync<TData>(key, data);
        }
    }
}
