using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Abstractions
{
    public interface IDocumentStore
    {
        Task<OperationResult> DeleteDocumentAsync<T>(string key);
        Task<OperationResult<T>> GetDocumentAsync<T>(string key);
        Task<IEnumerable<string>> GetKeysAsync<T>();
        Task<OperationResult> PutDocumentAsync<T>(string key, T data);
        Task<OperationResult> TransformDocumentAsync<T>(string key, Func<T, T> transfomer);
    }
}