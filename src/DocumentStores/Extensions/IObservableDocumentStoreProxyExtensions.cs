using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <summary/> 
    public static class IObservableDocumentStoreProxyExtensions
    {
        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        /// <remarks>
        /// <paramref name="updateData"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        public static Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            this IObservableDocumentStoreProxy<TData> source, string key,
            TData initialData, Func<TData, TData> updateData) where TData : class => 
                source.AddOrUpdateDocumentAsync(
                    _ => Task.FromResult(initialData), 
                    (_, data) => Task.FromResult(updateData(data)));


        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Returns it.
        /// </summary>
        public static Task<Result<TData>> GetOrAddDocumentAsync<TData>(
            this IObservableDocumentStoreProxy<TData> source, string key,
            TData initialData) where TData : class =>
                source.GetOrAddDocumentAsync(
                    _ => Task.FromResult(initialData));
    }
}