using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores;

namespace DocumentStores
{
    /// <summary>
    /// Provides methods for working with documents.
    /// </summary>
    public interface IDocumentStore
    {
        /// <summary>
        /// If the document with the specified <paramref name="address"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else, Updates it using the specified <paramref name="updateDataAsync"/> delegate.
        /// </summary>
        /// <remarks>
        /// Both <paramref name="addDataAsync"/> as well as <paramref name="updateDataAsync"/> are excecuted
        /// inside a lock on the specific document.
        /// </remarks>
        Task<IResult<TData>> AddOrUpdateAsync<TData>(
            DocumentAddress address,
            Func<DocumentAddress, Task<TData>> addDataAsync,
            Func<DocumentAddress, TData, Task<TData>> updateDataAsync) where TData : class;

        /// <summary>
        /// If the document with the specified <paramref name="address"/> does not exist,
        /// adds it using the <paramref name="addDataAsync"/> delegate.
        /// Else, returns it.
        /// </summary>
        /// <remarks>
        /// <paramref name="addDataAsync"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        Task<IResult<TData>> GetOrAddAsync<TData>(
            DocumentAddress address,
            Func<DocumentAddress, Task<TData>> addDataAsync) where TData : class;

        /// <summary>
        /// Deletes the document with the specified <paramref name="address"/>.
        /// </summary>
        Task<IResult<Unit>> DeleteAsync<TData>(DocumentAddress address) where TData : class;

        /// <summary>
        /// Returns <typeparamref name="TData"/> contained in the document with the specified <paramref name="address"/>.
        /// </summary>
        Task<IResult<TData>> GetAsync<TData>(DocumentAddress address) where TData : class;

        /// <summary>
        /// Returns addresses, associated to documents of <typeparamref name="TData"/>.
        /// </summary>
        Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute route,
            DocumentSearchOption options = DocumentSearchOption.AllLevels,
            CancellationToken ct = default) where TData : class;

        /// <summary>
        /// Saves the specified <paramref name="data"/> to a document with the specified <paramref name="address"/>
        /// </summary>
        Task<IResult<Unit>> PutAsync<TData>(DocumentAddress address, TData data) where TData : class;
    }
}