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
            DocumentAddress address,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class;

        Task<TData> GetOrAddDocumentAsync<TData>(DocumentAddress address,
            Func<string, Task<TData>> addDataAsync) where TData : class;

        Task<Unit> DeleteDocumentAsync<TData>(DocumentAddress address) where TData : class;

        Task<TData> GetDocumentAsync<TData>(DocumentAddress address) where TData : class;

        Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute name, 
            CancellationToken ct = default) where TData : class ;
        
        Task<Unit> PutDocumentAsync<TData>(DocumentAddress address, TData data) where TData : class;
    }


}