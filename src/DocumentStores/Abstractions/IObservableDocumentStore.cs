using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    public interface IObservableDocumentStore<TData> where TData : class
    {
        IObservable<IEnumerable<string>> GetKeysObservable();

        Task<Result<TData>> AddOrUpdateDocumentAsync(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync);      

        Task<Result<TData>> GetOrAddDocumentAsync(
            string key,
            Func<string, Task<TData>> addDataAsync);

        Task<Result<Unit>> DeleteDocumentAsync(string key);

        Task<Result<TData>> GetDocumentAsync(string key);

        Task<IEnumerable<string>> GetKeysAsync();
        
        Task<Result<Unit>> PutDocumentAsync(string key, TData data);
    }

}