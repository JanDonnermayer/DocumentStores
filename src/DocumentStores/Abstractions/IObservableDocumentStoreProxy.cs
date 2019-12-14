using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <summary>
    /// An <see cref="IObservableDocumentStore{TData}" /> for a single document
    /// with a specific key.
    /// </summary>
    public interface IDocumentChannel<TData> where TData : class        
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
        Task<Result<TData>> AddOrUpdateDocumentAsync(
            Func<string, Task<TData>> addDataAsync,
            Func<string, TData, Task<TData>> updateDataAsync);

        /// <summary>
        /// If the document does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Returns it.
        /// </summary>
        /// <remarks>
        /// <paramref name="addDataAsync"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        Task<Result<TData>> GetOrAddDocumentAsync(
            Func<string, Task<TData>> addDataAsync);

        /// <summary>
        /// Deletes the document.
        /// </summary>
        Task<Result<Unit>> DeleteDocumentAsync();

        /// <summary>
        /// Returns <typeparamref name="TData"/> contained in the document.
        /// </summary>
        Task<Result<TData>> GetDocumentAsync();

        /// <summary>
        /// Saves the specified <paramref name="data"/> to a document.
        /// </summary>
        Task<Result<Unit>> PutDocumentAsync(TData data);

        /// <summary>
        /// Returns an <see cref="IObservable{TData}"/> on the data contained in the document.
        /// </summary>
        IObservable<TData> GetObservable();
    }


}