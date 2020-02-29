using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentStores
{
    /// <summary>
    /// A view on a single document.
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
        Task<IResult<TData>> AddOrUpdateAsync(
            Func<Task<TData>> addDataAsync,
            Func<TData, Task<TData>> updateDataAsync);

        /// <summary>
        /// If the document does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else: Returns it.
        /// </summary>
        /// <remarks>
        /// <paramref name="addDataAsync"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        Task<IResult<TData>> GetOrAddAsync(
            Func<Task<TData>> addDataAsync);

        /// <summary>
        /// Deletes the document.
        /// </summary>
        Task<IResult<Unit>> DeleteAsync();

        /// <summary>
        /// Returns <typeparamref name="TData"/> contained in the document.
        /// </summary>
        Task<IResult<TData>> GetAsync();

        /// <summary>
        /// Saves the specified <paramref name="data"/> to a document.
        /// </summary>
        Task<IResult<Unit>> PutAsync(TData data);
    }
}