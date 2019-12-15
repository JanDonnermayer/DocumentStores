using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <summary/> 
    public static class IDocumentChannelExtensions
    {
        /// <summary>
        /// If the document does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        /// <remarks>
        /// <paramref name="updateData"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        public static Task<Result<TData>> AddOrUpdateDocumentAsync<TData>(
            this IDocumentChannel<TData> source, 
            TData initialData, Func<TData, TData> updateData) where TData : class => 
                source.AddOrUpdateDocumentAsync(
                    () => Task.FromResult(initialData), 
                    data => Task.FromResult(updateData(data)));


        /// <summary>
        /// If the document does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Returns it.
        /// </summary>
        public static Task<Result<TData>> GetOrAddDocumentAsync<TData>(
            this IDocumentChannel<TData> source,
            TData initialData) where TData : class =>
                source.GetOrAddDocumentAsync(
                    () => Task.FromResult(initialData));
    }
}