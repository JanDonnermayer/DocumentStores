using System;
using System.Threading.Tasks;
using DocumentStores.Internal;
using System.Collections.Generic;
using System.Threading;

namespace DocumentStores
{
    /// <inheritdoc/> 
    public sealed class JsonFileDocumentStore : IDocumentStore
    {
        private readonly IDocumentStore documentStore;

        /// <inheritdoc/> 
        public JsonFileDocumentStore(string directory)
        {
            this.documentStore = new DocumentStore(
                new JsonDocumentSerializer(),
                new FileDataStore(directory, ".json")
            );
        }

        /// <inheritdoc/>
        public Task<Result<TData>> AddOrUpdateAsync<TData>(
            DocumentAddress address, Func<DocumentAddress, Task<TData>> addDataAsync,
            Func<DocumentAddress, TData, Task<TData>> updateDataAsync) where TData : class
        {
            return documentStore.AddOrUpdateAsync(address, addDataAsync, updateDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> DeleteAsync<TData>(DocumentAddress address) where TData : class
        {
            return documentStore.DeleteAsync<TData>(address);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetAsync<TData>(DocumentAddress address) where TData : class
        {
            return documentStore.GetAsync<TData>(address);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute route,
            DocumentSearchOption options = DocumentSearchOption.AllLevels,
            CancellationToken ct = default) where TData : class
        {
            return documentStore.GetAddressesAsync<TData>(route, options, ct);
        }

        /// <inheritdoc/>
        public Task<Result<TData>> GetOrAddAsync<TData>(DocumentAddress address,
            Func<DocumentAddress, Task<TData>> addDataAsync) where TData : class
        {
            return documentStore.GetOrAddAsync(address, addDataAsync);
        }

        /// <inheritdoc/>
        public Task<Result<Unit>> PutAsync<TData>(DocumentAddress address, TData data) where TData : class
        {
            return documentStore.PutAsync<TData>(address, data);
        }
    }
}
