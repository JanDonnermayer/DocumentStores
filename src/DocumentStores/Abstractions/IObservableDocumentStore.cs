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

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Updates it using the specified <paramref name="updateDataAsync"/> delegate.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="addDataAsync"/> as well as <paramref name="updateDataAsync"/> are excecuted
        /// inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> AddOrUpdateDocumentAsync(
            string key,
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync);

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Returns it.
        /// </summary>
        /// <remarks>
        /// <paramref name="addDataAsync"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> GetOrAddDocumentAsync(
            string key,
            Func<string, Task<TData>> addDataAsync);

        /// <summary>
        /// Deletes the document with the specified <paramref name="key"/>.
        /// </summary>
        Task<Result<Unit>> DeleteDocumentAsync(string key);

        /// <summary>
        /// Returns <typeparamref name="TData"/> from the document with the specified <paramref name="key"/>.
        /// </summary>
        Task<Result<TData>> GetDocumentAsync(string key);

        /// <summary>
        /// Returns all keys, associated to documents of <typeparamref name="TData"/>.
        /// </summary>
        Task<IEnumerable<string>> GetKeysAsync();

        /// <summary>
        /// Saves the specified <paramref name="data"/> to the document with the specified <paramref name="key"/>.
        /// If the document does not exist: Creates it.
        /// </summary>
        Task<Result<Unit>> PutDocumentAsync(string key, TData data);
    }

}