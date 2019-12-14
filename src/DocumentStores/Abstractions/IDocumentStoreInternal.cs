using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal interface IDocumentStoreInternal
    {
        Task<TData> AddOrUpdateDocumentAsync<TData>(
            DocumentKey key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class;

        Task<TData> GetOrAddDocumentAsync<TData>(DocumentKey key,
            Func<string, Task<TData>> addDataAsync) where TData : class;

        Task<Unit> DeleteDocumentAsync<TData>(DocumentKey key) where TData : class;

        Task<TData> GetDocumentAsync<TData>(DocumentKey key) where TData : class;

        Task<IEnumerable<DocumentKey>> GetKeysAsync<TData>(
            DocumentTopicName name, 
            CancellationToken ct = default) where TData : class ;
        
        Task<Unit> PutDocumentAsync<TData>(DocumentKey key, TData data) where TData : class;
    }


}