using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    public static class IObservableDocumentStoreExtensions
    {
        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Updates it using the specified <paramref name="updateDataAsync"/> delegate.
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
    }
}