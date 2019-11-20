using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    interface IDocumentStoreInternal
    {
        Task<TData> AddOrUpdateDocumentAsync<TData>(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class;

        Task<TData> GetOrAddDocumentAsync<TData>(string key,
            Func<string, Task<TData>> addDataAsync) where TData : class;

        Task<Unit> DeleteDocumentAsync<TData>(string key) where TData : class;

        Task<TData> GetDocumentAsync<TData>(string key) where TData : class;

        Task<IEnumerable<string>> GetKeysAsync<TData>(CancellationToken ct = default) where TData : class ;
        
        Task<Unit> PutDocumentAsync<TData>(string key, TData data) where TData : class;
    }


}