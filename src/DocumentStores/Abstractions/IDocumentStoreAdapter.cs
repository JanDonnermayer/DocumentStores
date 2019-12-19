﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal interface IDocumentStoreAdapter
    {
        Task<TData> AddOrUpdateAsync<TData>(
            DocumentAddress address,
            Func<DocumentAddress, Task<TData>> addDataAsync,
            Func<DocumentAddress, TData, Task<TData>> updateDataAsync) where TData : class;

        Task<TData> GetOrAddAsync<TData>(DocumentAddress address,
            Func<DocumentAddress, Task<TData>> addDataAsync) where TData : class;

        Task<Unit> DeleteAsync<TData>(DocumentAddress address) where TData : class;

        Task<TData> GetAsync<TData>(DocumentAddress address) where TData : class;

        Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute route, 
            DocumentSearchOptions options,
            CancellationToken ct = default) where TData : class ;
        
        Task<Unit> PutAsync<TData>(DocumentAddress address, TData data) where TData : class;
    }


}