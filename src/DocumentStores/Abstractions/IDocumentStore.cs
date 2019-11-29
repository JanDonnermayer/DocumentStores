using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores;
using DocumentStores.Primitives;

namespace DocumentStores
{    
    public interface IDocumentStore
    {
        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Updates it using the specified <paramref name="updateDataAsync"/> delegate.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="addDataAsync"/> as well as <paramref name="updateDataAsync"/> are excecuted
        /// inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class;

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Returns it.
        /// </summary>
        /// <remarks>
        /// <paramref name="addDataAsync"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> GetOrAddDocumentAsync<TData>(string key,
            Func<string, Task<TData>> addDataAsync) where TData : class;

        /// <summary>
        /// Deletes the document with the specified <paramref name="key"/>.
        /// </summary>
        Task<Result<Unit>> DeleteDocumentAsync<TData>(string key) where TData : class;

        /// <summary>
        /// Returns <typeparamref name="TData"/> contained in the document with the specified <paramref name="key"/>.
        /// </summary>
        Task<Result<TData>> GetDocumentAsync<TData>(string key) where TData : class;

        /// <summary>
        /// Returns all keys, associated to documents of <typeparamref name="TData"/>.
        /// </summary>
        Task<IEnumerable<string>> GetKeysAsync<TData>(CancellationToken ct = default) where TData : class ;
        
        /// <summary>
        /// Saves the specified <paramref name="data"/> to a document with the specified <paramref name="key"/>
        /// </summary>
        Task<Result<Unit>> PutDocumentAsync<TData>(string key, TData data) where TData : class;
    }


}