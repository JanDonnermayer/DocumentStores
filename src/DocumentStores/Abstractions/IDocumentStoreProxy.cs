using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <summary>
    /// Provides methods for working with a document.
    /// </summary>
    public interface IDocumentStoreProxy
    {
        /// <summary>
        /// If the documentdoes not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Updates it using the specified <paramref name="updateDataAsync"/> delegate.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="addDataAsync"/> as well as <paramref name="updateDataAsync"/> are excecuted
        /// inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync) where TData : class;

        /// <summary>
        /// If the document does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Returns it.
        /// </summary>
        /// <remarks>
        /// <paramref name="addDataAsync"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> GetOrAddDocumentAsync<TData>(
            Func<string, Task<TData>> addDataAsync) where TData : class;

        /// <summary>
        /// Deletes the document.
        /// </summary>
        Task<Result<Unit>> DeleteDocumentAsync<TData>() where TData : class;

        /// <summary>
        /// Returns <typeparamref name="TData"/> contained in the document.
        /// </summary>
        Task<Result<TData>> GetDocumentAsync<TData>() where TData : class;

        /// <summary>
        /// Saves the specified <paramref name="data"/> to a document.
        /// </summary>
        Task<Result<Unit>> PutDocumentAsync<TData>(TData data) where TData : class;
    }


}