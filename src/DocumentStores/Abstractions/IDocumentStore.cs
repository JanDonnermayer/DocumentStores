using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    public interface IDocumentStore
    {
        Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class;
        Task<Result<TData>> GetOrAddDocumentAsync<TData>(
            string key,
            Func<string, Task<TData>> addDataAsync) where TData : class;
        Task<Result> DeleteDocumentAsync<TData>(string key) where TData : class;
        Task<Result<TData>> GetDocumentAsync<TData>(string key) where TData : class;
        Task<IEnumerable<string>> GetKeysAsync<TData>();
        Task<Result> PutDocumentAsync<TData>(string key, TData data) where TData : class;
    }
}