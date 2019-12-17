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
            this.documentStore = new DocumentStore(
                new JsonFileDocumentSerializer(),
                new FileDocumentRouter(directory, ".json")
            );
        }

        /// <inheritdoc/>
        public Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            DocumentAddress address, Func<DocumentAddress, Task<TData>> addDataAsync,
            Func<DocumentAddress, TData, Task<TData>> updateDataAsync) where TData : class
        {
            return documentStore.AddOrUpdateDocumentAsync(address, addDataAsync, updateDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> DeleteDocumentAsync<TData>(DocumentAddress address) where TData : class
        {
            return documentStore.DeleteDocumentAsync<TData>(address);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetDocumentAsync<TData>(DocumentAddress address) where TData : class
        {
            return documentStore.GetDocumentAsync<TData>(address);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute route,
            DocumentSearchOptions options = DocumentSearchOptions.AllLevels,
            CancellationToken ct = default) where TData : class
        {
            return documentStore.GetAddressesAsync<TData>(route, options, ct);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetOrAddDocumentAsync<TData>(DocumentAddress address,
            Func<DocumentAddress, Task<TData>> addDataAsync) where TData : class
        {
            return documentStore.GetOrAddDocumentAsync(address, addDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> PutDocumentAsync<TData>(DocumentAddress address, TData data) where TData : class
        {
            return documentStore.PutDocumentAsync<TData>(address, data);
        }
    }
}
