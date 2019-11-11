using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentStores.Abstractions;
using DocumentStores.Primitives;
using Microsoft.Extensions.Caching.Memory;

namespace DocumentStores
{
    public class InMemoryDocumentStore : IDocumentStore
    {
        private readonly IMemoryCache cache =
         new MemoryCache(new MemoryCacheOptions());

        Task<OperationResult> IDocumentStore.DeleteDocumentAsync<T>(string key)
        {
            cache.Remove(key);
            return Task.FromResult(new OperationResult());
        }

        Task<OperationResult<T>> IDocumentStore.GetDocumentAsync<T>(string key)
        {
            var item = cache.Get<T>(key);
            return Task.FromResult(new OperationResult<T>(true, data: item));
        }

        Task<IEnumerable<string>> IDocumentStore.GetKeysAsync<T>()
        {
            return Task.FromResult(Enumerable.Empty<String>());
        }

        Task<OperationResult> IDocumentStore.PutDocumentAsync<T>(string key, T data)
        {
            cache.Set<T>(key, data);
            return Task.FromResult(new OperationResult());
        }

        Task<OperationResult> IDocumentStore.TransformDocumentAsync<T>(string key, Func<T, T> transfomer)
        {
            var item = cache.Get<T>(key);
            var newItem = transfomer(item);
            cache.Set<T>(key, newItem);
            return Task.FromResult(new OperationResult());
        }
    }
}