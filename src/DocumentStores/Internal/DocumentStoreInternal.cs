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

    internal class DocumentStoreInternal : IDocumentStoreInternal
    {

        #region Constructor

        public DocumentStoreInternal(IDocumentSerializer serializer, IDocumentRouter router)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        #endregion


        #region Private 

        private readonly IDocumentSerializer serializer;

        private readonly IDocumentRouter router;

        private IDocumentProxy<TDocument> GetDocumentProxy<TDocument>(DocumentAddress address) =>
            router.CreateProxy<TDocument>(address);

        private ImmutableDictionary<DocumentAddress, SemaphoreSlim> locks =
            ImmutableDictionary<DocumentAddress, SemaphoreSlim>.Empty;

        private async Task<IDisposable> GetLockAsync(DocumentAddress key)
        {
            var sem = ImmutableInterlocked.GetOrAdd(ref locks, key, s => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            return new Disposable(() => sem.Release());
        }

        #endregion


        #region Implementation of IDocumentStoreInternal 

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<T>(
            DocumentRoute route, DocumentSearchOptions options, CancellationToken ct = default) where T : class =>
                  router.GetAddressesAsync<T>(route, options, ct);

        public async Task<T> GetDocumentAsync<T>(DocumentAddress address) where T : class
        {
            using var @lock = await GetLockAsync(address);

            var router = GetDocumentProxy<T>(address);

            if (!router.Exists())
                throw new DocumentException($"No such document: {address}");

            using var stream = router.GetReadStream();

            return await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
        }

        public async Task<Unit> PutDocumentAsync<T>(DocumentAddress address, T data) where T : class
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using var @lock = await GetLockAsync(address);

            var router = GetDocumentProxy<T>(address);

            router.Delete();
            using var stream = router.GetWriteStream();
            await serializer.SerializeAsync(stream, data).ConfigureAwait(false);

            return Unit.Default;
        }

        public async Task<T> AddOrUpdateDocumentAsync<T>(
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync,
            Func<DocumentAddress, T, Task<T>> updateDataAsync) where T : class
        {
            if (addDataAsync is null)
                throw new ArgumentNullException(nameof(addDataAsync));

            if (updateDataAsync is null)
                throw new ArgumentNullException(nameof(updateDataAsync));

            using var @lock = await GetLockAsync(address);

            var document = GetDocumentProxy<T>(address);

            async Task<T> GetDataAsync()
            {
                if (!document.Exists()) return await addDataAsync(address)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

                using var stream = document.GetReadStream();
                var data = await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);

                return await updateDataAsync(address, data)
                    ?? throw new DocumentException($"{nameof(updateDataAsync)} returned null!");
            }

            var data = await GetDataAsync();

            document.Delete();
            using var stream = document.GetWriteStream();
            await serializer.SerializeAsync(stream, data).ConfigureAwait(false);

            return data;
        }


        public async Task<T> GetOrAddDocumentAsync<T>(
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync) where T : class
        {
            if (addDataAsync is null)
                throw new ArgumentNullException(nameof(addDataAsync));

            using var @lock = await GetLockAsync(address);

            var document = GetDocumentProxy<T>(address);

            if (document.Exists())
            {
                using var stream = document.GetReadStream();
                return await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
            }
            else
            {
                var data = await addDataAsync(address)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

                document.Delete();
                using var stream = document.GetWriteStream();
                await serializer.SerializeAsync(stream, data).ConfigureAwait(false);

                return data;
            }
        }

        public async Task<Unit> DeleteDocumentAsync<T>(DocumentAddress address) where T : class
        {
            using var @lock = await GetLockAsync(address);

            var document = GetDocumentProxy<T>(address);

            if (!document.Exists())
                throw new DocumentException($"No such document: {address}");

            document.Delete();

            return Unit.Default;
        }

        #endregion

    }

}
