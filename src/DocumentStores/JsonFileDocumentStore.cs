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

        /// <summary>
        /// Creates a new instance of the <see cref="JsonFileDocumentStore"/>class,
        /// using default <see cref="JsonFileDocumentStoreOptions"/>.
        /// </summary>
        public JsonFileDocumentStore()
            : this(JsonFileDocumentStoreOptions.Default) { }

        /// <summary>
        /// Creates a new instance of the <see cref="JsonFileDocumentStore"/>class,
        /// storing json documents in the specified <paramref name="rootDirectory"/>.
        /// </summary>
        /// <param name="rootDirectory">The directory in which to store the json documents.</param>
        public JsonFileDocumentStore(string rootDirectory)
            : this(new JsonFileDocumentStoreOptions(rootDirectory)) { }

        /// <summary>
        /// Creates a new instance of the <see cref="JsonFileDocumentStore"/>class,
        /// storing json documents in the specified <paramref name="rootDirectory"/>,
        /// applying AES-encryption using the specified <paramref name="password"/>.
        /// </summary>
        /// <param name="rootDirectory">The directory in which to store the json documents.</param>
        /// <param name="password">The password to use for AES encryption.</param>
        public JsonFileDocumentStore(string rootDirectory, string password)
            : this(new JsonFileDocumentStoreOptions(rootDirectory, new AesEncryptionOptions(password))) { }

        /// <summary>
        /// Creates a new instance of the <see cref="JsonFileDocumentStore"/>class,
        /// using the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options to use.</param>
        public JsonFileDocumentStore(JsonFileDocumentStoreOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            const string JSON_FILE_EXTENSION = ".json";

            IDocumentSerializer serializer = options.EncryptionOptions switch
            {
                AesEncryptionOptions opt => new AesEncryptedDocumentSerializer(
                    internalSerializer: new JsonDocumentSerializer(),
                    options: opt
                ),
                NoEncryptionOptions _ => new JsonDocumentSerializer(),
                _ => throw new ArgumentException("Invalid encryption options!")
            };

            var internalStore = new DocumentStoreInternal(
                serializer: serializer,
                dataStore: new FileDataStore(options.RootDirectory, JSON_FILE_EXTENSION)
            );

            this.documentStore = new DocumentStore(
                internalStore
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
