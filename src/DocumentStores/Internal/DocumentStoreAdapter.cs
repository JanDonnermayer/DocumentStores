using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Immutable;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{

    internal class DocumentStoreAdapter : IDocumentStoreAdapter
    {

        #region Constructor

        public DocumentStoreAdapter(IDocumentSerializer serializer, IDataStore router)
        {
            this.dataStore = router ?? throw new ArgumentNullException(nameof(router));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        #endregion


        #region Private 

        private readonly IDocumentSerializer serializer;

        private readonly IDataStore dataStore;


        /*

        private DocumentRoute GetTypedRoute<T>() =>
            DocumentRoute
                .Create(typeof(T)
                    .ShortName(true)
                    .Replace(">", "}")
                    .Replace("<", "{"));

        private DocumentRoute GetTypedRoute<T>(DocumentRoute route) =>
            route.Prepend(GetTypedRoute<T>());

        private DocumentAddress GetTypedAddress<T>(DocumentAddress address) =>
            address.MapRoute(r => GetTypedRoute<T>(r));

        */

        private IDataProxy GetDataProxy(DocumentAddress address) =>
            dataStore.CreateProxy(address);

        private async Task<T> DeserializeAsync<T>(IDataProxy dataProxy) where T : class
        {
            using var stream = dataProxy.GetReadStream();
            return await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
        }

        private async Task SerializeAsync<T>(IDataProxy dataProxy, T data) where T : class
        {
            dataProxy.Delete();
            using var stream = dataProxy.GetWriteStream();
            await serializer.SerializeAsync<T>(stream, data).ConfigureAwait(false);
        }

        private ImmutableDictionary<DocumentAddress, SemaphoreSlim> locks =
            ImmutableDictionary<DocumentAddress, SemaphoreSlim>.Empty;

        private async Task<IDisposable> GetLockAsync(DocumentAddress key)
        {
            var sem = ImmutableInterlocked.GetOrAdd(ref locks, key, s => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            return new Disposable(() => sem.Release());
        }

        #endregion


        #region Implementation of IDocumentStoreAdapter 

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<T>(
            DocumentRoute route, DocumentSearchOptions options, CancellationToken ct = default) where T : class =>
                  dataStore.GetAddressesAsync(route, options, ct);

        public async Task<T> GetAsync<T>(DocumentAddress address) where T : class
        {
            using var @lock = await GetLockAsync(address);

            var dataProxy = GetDataProxy(address);

            if (!dataProxy.Exists())
                throw new DocumentMissingException(address);

            return await DeserializeAsync<T>(dataProxy);
        }

        public async Task<Unit> PutAsync<T>(DocumentAddress address, T data) where T : class
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var @lock = await GetLockAsync(address);

            var dataProxy = GetDataProxy(address);

            await SerializeAsync(dataProxy, data);

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

            using var @lock = await GetLockAsync(address);

            var dataProxy = GetDataProxy(address);

            async Task<T> GetDataAsync()
            {
                if (!dataProxy.Exists()) return await addDataAsync(address)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

                var data = await DeserializeAsync<T>(dataProxy);

                return await updateDataAsync(address, (T)data)
                    ?? throw new DocumentException($"{nameof(updateDataAsync)} returned null!");
            }

            var data = await GetDataAsync();

            await SerializeAsync(dataProxy, data);

            return data;
        }


        public async Task<T> GetOrAddAsync<T>(
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync) where T : class
        {
            if (addDataAsync is null)
                throw new ArgumentNullException(nameof(addDataAsync));

            using var @lock = await GetLockAsync(address);

            var dataProxy = GetDataProxy(address);

            if (dataProxy.Exists())
            {
                return await DeserializeAsync<T>(dataProxy);
            }
            else
            {
                var data = await addDataAsync(address)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

                await SerializeAsync(dataProxy, data);

                return data;
            }
        }

        public async Task<Unit> DeleteAsync<T>(DocumentAddress address) where T : class
        {
            using var @lock = await GetLockAsync(address);

            var dataProxy = GetDataProxy(address);

            if (!dataProxy.Exists())
                throw new DocumentMissingException(address);

            dataProxy.Delete();

            return Unit.Default;
        }

        #endregion

    }

}
