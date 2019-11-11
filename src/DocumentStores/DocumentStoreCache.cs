using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Abstractions;
using DocumentStores.Primitives;
using Microsoft.Extensions.Caching.Memory;

namespace DocumentStores
{
    public class DocumentStoreCache : IDocumentStore

    {
        private readonly IDocumentStore internalService;

        private readonly IMemoryCache cache;

        #region Private Members

        private ImmutableDictionary<string, SemaphoreSlim> locks =
            ImmutableDictionary<string, SemaphoreSlim>.Empty;

        private async Task<IDisposable> GetLockAsync(string key)
        {
            var sem = ImmutableInterlocked.GetOrAdd(ref locks, key, s => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            return new Disposable(() => sem.Release());
        }
        #endregion

        #region Constructor

        public DocumentStoreCache(IDocumentStore internalService, IMemoryCache cache)
        {
            this.internalService = internalService ?? throw new ArgumentNullException(nameof(internalService));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public DocumentStoreCache(IDocumentStore internalService) :
            this(internalService, new MemoryCache(new MemoryCacheOptions()))
        {
        }

        #endregion

        #region Implementation of IDocumentStore

        public Task<IEnumerable<string>> GetKeysAsync<T>() =>
            internalService.GetKeysAsync<T>();

        public async Task<OperationResult> DeleteDocumentAsync<T>(string documentId)
        {
            using (await GetLockAsync(documentId))
            {
                cache.Remove(documentId);
                return await internalService.DeleteDocumentAsync<T>(documentId);
            }
        }

        public async Task<OperationResult<T>> GetDocumentAsync<T>(string documentId)
        {
            async Task<T> GetDocumentAsync(ICacheEntry entry)
            {
                var _res = await internalService.GetDocumentAsync<T>(documentId);
                return _res;
            }
            using (await GetLockAsync(documentId))
            {
                var res = await cache.GetOrCreateAsync(documentId, GetDocumentAsync);
                return res;
            }
        }

        public async Task<OperationResult> PutDocumentAsync<T>(string documentId, T data)
        {
            using (await GetLockAsync(documentId))
            {
                cache.Set(documentId, data);
                return await internalService.PutDocumentAsync(documentId, data);
            }
        }

        public async Task<OperationResult> TransformDocumentAsync<T>(string documentId, Func<T, T> transfomer)
        {
            async Task<T> GetDocumentAsync(ICacheEntry entry)
            {
                var _res = await internalService.GetDocumentAsync<T>(documentId);
                return _res;
            }
            using (await GetLockAsync(documentId))
            {
                T originalData = await cache.GetOrCreateAsync(documentId, GetDocumentAsync);
                T transformedData = transfomer(originalData);
                cache.Set(documentId, transformedData);
                return await internalService.PutDocumentAsync(documentId, transformedData);
            }
        }

        #endregion

    }
}
