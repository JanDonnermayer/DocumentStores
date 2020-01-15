using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Immutable;
;

namespace DocumentStores.Internal
{
    internal class DocumentStoreAdapter : IDocumentStoreAdapter
    {
        #region Constructor

        public DocumentStoreAdapter(IDocumentSerializer serializer, IDataStore dataStore)
        {
            this.rootDataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        #endregion

        #region Private 

        private readonly IDocumentSerializer serializer;

        private readonly IDataStore rootDataStore;

        private ImmutableDictionary<Type, IDataStore> typeSpecificDataStores =
            ImmutableDictionary<Type, IDataStore>.Empty;

        private IDataStore GetDataStore<T>()
        {
            static DocumentRoute GetTypedRoute() =>
              DocumentRoute
                  .Create(typeof(T)
                      .ShortName(true)
                      .Replace(">", "}")
                      .Replace("<", "{"));

            IDataStore CreateTypeSpecificDataStore() =>
                new TranslatedDataStore(
                    source: rootDataStore,
                    translateIn: r => r.Prepend(GetTypedRoute()),
                    translateOut: r => r.TrimLeft(GetTypedRoute())
                );

            return ImmutableInterlocked.GetOrAdd(
                location: ref typeSpecificDataStores,
                key: typeof(T),
                valueFactory: _ => CreateTypeSpecificDataStore()
            );
        }

        private IDataChannel GetDataChannel<T>(DocumentAddress address) =>
            GetDataStore<T>().ToChannel(address);

        private async Task<T> DeserializeAsync<T>(IDataChannel dataChannel) where T : class
        {
            using var stream = dataChannel.GetReadStream();
            return await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
        }

        private async Task SerializeAsync<T>(IDataChannel dataChannel, T data) where T : class
        {
            dataChannel.Delete();
            using var stream = dataChannel.GetWriteStream();
            await serializer.SerializeAsync<T>(stream, data).ConfigureAwait(false);
        }

        private ImmutableDictionary<DocumentAddress, SemaphoreSlim> locks =
            ImmutableDictionary<DocumentAddress, SemaphoreSlim>.Empty;

        private async Task<IDisposable> GetLockAsync(DocumentAddress key)
        {
            var sem = ImmutableInterlocked.GetOrAdd(ref locks, key, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync().ConfigureAwait(false);
            return new Disposable(() => sem.Release());
        }

        #endregion


        #region Implementation of IDocumentStoreAdapter 

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<T>(
            DocumentRoute route, DocumentSearchOption options, CancellationToken ct = default) where T : class =>
                Task.Run(() => GetDataStore<T>().GetAddresses(route, options), ct);

        public async Task<T> GetAsync<T>(DocumentAddress address) where T : class
        {
            using var _ = await GetLockAsync(address).ConfigureAwait(false);

            var dataChannel = GetDataChannel<T>(address);

            if (!dataChannel.Exists())
                throw new DocumentMissingException(address);

            return await DeserializeAsync<T>(dataChannel).ConfigureAwait(false);
        }

        public async Task<Unit> PutAsync<T>(DocumentAddress address, T data) where T : class
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var _ = await GetLockAsync(address).ConfigureAwait(false);

            var dataChannel = GetDataChannel<T>(address);

            await SerializeAsync(dataChannel, data).ConfigureAwait(false);

            return Unit.Default;
        }

        public async Task<T> AddOrUpdateAsync<T>(
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync,
            Func<DocumentAddress, T, Task<T>> updateDataAsync) where T : class
        {
            if (addDataAsync is null)
                throw new ArgumentNullException(nameof(addDataAsync));

            if (updateDataAsync is null)
                throw new ArgumentNullException(nameof(updateDataAsync));

            using var _ = await GetLockAsync(address).ConfigureAwait(false);

            var dataChannel = GetDataChannel<T>(address);

            async Task<T> GetDataAsync()
            {
                if (!dataChannel.Exists())
                {
                    return await addDataAsync(address).ConfigureAwait(false)
                        ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");
                }

                var data = await DeserializeAsync<T>(dataChannel).ConfigureAwait(false);

                return await updateDataAsync(address, (T)data).ConfigureAwait(false)
                    ?? throw new DocumentException($"{nameof(updateDataAsync)} returned null!");
            }

            var data = await GetDataAsync().ConfigureAwait(false);

            await SerializeAsync(dataChannel, data).ConfigureAwait(false);

            return data;
        }

        public async Task<T> GetOrAddAsync<T>(
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync) where T : class
        {
            if (addDataAsync is null)
                throw new ArgumentNullException(nameof(addDataAsync));

            using var _ = await GetLockAsync(address).ConfigureAwait(false);

            var dataChannel = GetDataChannel<T>(address);

            if (dataChannel.Exists())
            {
                return await DeserializeAsync<T>(dataChannel).ConfigureAwait(false);
            }
            else
            {
                var data = await addDataAsync(address).ConfigureAwait(false)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

                await SerializeAsync(dataChannel, data).ConfigureAwait(false);

                return data;
            }
        }

        public async Task<Unit> DeleteAsync<T>(DocumentAddress address) where T : class
        {
            using var _ = await GetLockAsync(address).ConfigureAwait(false);

            var dataChannel = GetDataChannel<T>(address);

            if (!dataChannel.Exists())
                throw new DocumentMissingException(address);

            dataChannel.Delete();

            return Unit.Default;
        }

        #endregion

    }

}
