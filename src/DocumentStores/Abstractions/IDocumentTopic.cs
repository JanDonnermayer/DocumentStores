using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
;

namespace DocumentStores
{
    /// <summary>
    /// A view on documents at a given route.
    /// </summary>
    public interface IDocumentTopic<TData> where TData : class
    {
        /// <summary>
        /// Gets an observable on all associated document keys.
        /// </summary>
        IObservable<IEnumerable<DocumentKey>> GetKeysObservable();

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Updates it using the specified <paramref name="updateDataAsync"/> delegate.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="addDataAsync"/> as well as <paramref name="updateDataAsync"/> are excecuted
        /// inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> AddOrUpdateAsync(
            DocumentKey key,
            Func<DocumentKey, Task<TData>> addDataAsync,
            Func<DocumentKey, TData, Task<TData>> updateDataAsync);

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Returns it.
        /// </summary>
        /// <remarks>
        /// <paramref name="addDataAsync"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> GetOrAddAsync(
            DocumentKey key,
            Func<DocumentKey, Task<TData>> addDataAsync);

        /// <summary>
        /// Deletes the document with the specified <paramref name="key"/>.
        /// </summary>
        Task<Result<Unit>> DeleteAsync(DocumentKey key);

        /// <summary>
        /// Returns <typeparamref name="TData"/> from the document with the specified <paramref name="key"/>.
        /// </summary>
        Task<Result<TData>> GetAsync(DocumentKey key);

        /// <summary>
        /// Returns all keys, associated to documents of <typeparamref name="TData"/>.
        /// </summary>
        Task<IEnumerable<DocumentKey>> GetKeysAsync();

        /// <summary>
        /// Saves the specified <paramref name="data"/> to the document with the specified <paramref name="key"/>.
        /// If the document does not exist: Creates it.
        /// </summary>
        Task<Result<Unit>> PutAsync(DocumentKey key, TData data);
    }

}