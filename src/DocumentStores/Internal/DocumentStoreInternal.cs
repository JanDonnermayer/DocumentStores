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

    /// <summary>
    /// An <see cref="IDocumentStoreInternal"/>-Implementation,
    /// that uses files, to store serializable data.
    /// </summary>
    /// <remarks>
    /// Each document is stored as a separate file with the following path:
    /// "[RootDirectory]/[DataType]/[key].[extension]".
    /// Keys are cleaned in order to be valid file-names.
    /// </remarks>
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
            if (!router.Exists<T>(address)) throw new DocumentException($"No such document: {address}");
            using var stream = router.GetReadStream<T>(address);

            return await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
        }

        public async Task<Unit> PutDocumentAsync<T>(DocumentAddress address, T data) where T : class
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            using var @lock = await GetLockAsync(address);
            using var stream = router.GetWriteStream<T>(address);

            await serializer.SerializeAsync(stream, data).ConfigureAwait(false);

            return Unit.Default;
        }

        public async Task<T> AddOrUpdateDocumentAsync<T>(
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync,
            Func<DocumentAddress, T, Task<T>> updateDataAsync) where T : class
        {
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));
            if (updateDataAsync is null) throw new ArgumentNullException(nameof(updateDataAsync));

            using var @lock = await GetLockAsync(address);

            async Task<T> GetDocumentAsync()
            {
                if (!router.Exists<T>(address)) return await addDataAsync(address)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

                using var stream = router.GetReadStream<T>(address);
                var data = await serializer.DeserializeAsync<T>(stream).ConfigureAwait(false);
                return await updateDataAsync(address, data) ?? throw new DocumentException($"{nameof(updateDataAsync)} returned null!");
            }

            var data = await GetDocumentAsync();
            using var stream = router.GetWriteStream<T>(address);
            await serializer.SerializeAsync(stream, data);

            return data;
        }


        public async Task<T> GetOrAddDocumentAsync<T>(
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync) where T : class
        {
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));

            var file = GetFile<T>(address);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

            using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);
            using StreamWriter SW = new StreamWriter(FS);

            var data = await DeserializeAsync<T>(SR)
                ?? await addDataAsync(address)
                ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

            FS.Position = 0;
            await SerializeAsync(data, SW);
            SW.Flush();
            FS.SetLength(FS.Position);

            return data;
        }

        public async Task<Unit> DeleteDocumentAsync<T>(DocumentAddress address) where T : class
        {
            var file = GetFile<T>(address);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {address}");

            File.Delete(file);
            return Unit.Default;
        }

        #endregion

    }

}
