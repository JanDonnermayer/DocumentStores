using System;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <inheritdoc /> 
    public static class IDocumentProxyExtensions
    {
        /// <summary>
        /// If the document does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        /// <remarks>
        /// <paramref name="updateData"/> is excecuted inside a lock on the specific document.
        /// </remarks>
        public static Task<Result<TData>> AddOrUpdateAsync<TData>(
            this IDocumentProxy<TData> source, 
            TData initialData, Func<TData, TData> updateData) where TData : class =>
                (source ?? throw new ArgumentNullException(nameof(source))).AddOrUpdateAsync(
                    addDataAsync: () => Task.FromResult(initialData),
                    updateDataAsync: data => Task.FromResult(updateData(data))
                );

        /// <summary>
        /// If the document does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Returns it.
        /// </summary>
        public static Task<Result<TData>> GetOrAddAsync<TData>(
            this IDocumentProxy<TData> source,
            TData initialData) where TData : class =>
                (source ?? throw new ArgumentNullException(nameof(source))).GetOrAddAsync(
                    addDataAsync: () => Task.FromResult(initialData)
                );
    }
}