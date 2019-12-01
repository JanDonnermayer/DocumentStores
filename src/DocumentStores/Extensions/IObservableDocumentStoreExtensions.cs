using System;
using System.Threading.Tasks;
using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <summary/> 
    public static class IObservableDocumentStoreExtensions
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
            this IObservableDocumentStore<TData> source, string key,
            TData initialData, Func<TData, TData> updateData) where TData : class => 
                source.AddOrUpdateDocumentAsync(
                    key, 
                    _ => Task.FromResult(initialData), 
                    (_, data) => Task.FromResult(updateData(data)));


        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Returns it.
        /// </summary>
        public static Task<Result<TData>> GetOrAddDocumentAsync<TData>(
            this IObservableDocumentStore<TData> source, string key,
            TData initialData) where TData : class =>
                source.GetOrAddDocumentAsync(
                    key, 
                    _ => Task.FromResult(initialData));

        /// <summary>
        /// Creates a proxy for the document with the specified <paramref name="key"/>
        /// </summary>
        public static IObservableDocumentStoreProxy<TData> CreateProxy<TData>(
            this IObservableDocumentStore<TData> source, 
            string key)  where TData : class =>
                new ObservableDocumentStoreProxy<TData>(source, key);

    }
}