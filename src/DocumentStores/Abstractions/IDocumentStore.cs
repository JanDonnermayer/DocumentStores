using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Abstractions
{
    public interface IDocumentStore
    {
        Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class;
        Task<Result> DeleteDocumentAsync<TData>(string key) where TData : class;
        Task<Result<TData>> GetDocumentAsync<TData>(string key) where TData : class;
        Task<IEnumerable<string>> GetKeysAsync<TData>();
        Task<Result> PutDocumentAsync<TData>(string key, TData data) where TData : class;
    }

    public interface IDocumentStore<TData> where TData : class
    {
        Task<Result<TData>> AddOrUpdateDocumentAsync(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync);
        Task<Result> DeleteDocumentAsync(string key);
        Task<Result<TData>> GetDocumentAsync(string key);
        Task<IEnumerable<string>> GetKeysAsync();
        Task<Result> PutDocumentAsync(string key, TData data);
    }
}