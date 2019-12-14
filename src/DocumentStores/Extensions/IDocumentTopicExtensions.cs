using System;
using System.Threading.Tasks;
using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <summary/> 
    public static class IDocumentTopicExtensions
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
            this IDocumentTopic<TData> source, DocumentKey key,
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
            this IDocumentTopic<TData> source, DocumentKey key,
            TData initialData) where TData : class =>
                source.GetOrAddDocumentAsync(
                    key,
                    _ => Task.FromResult(initialData));

        /// <summary>
        /// Creates a channel for the document with the specified <paramref name="key"/>
        /// </summary>
        public static IDocumentChannel<TData> CreateChannel<TData>(
            this IDocumentTopic<TData> source, DocumentKey key) where TData : class =>
                new DocumentChannel<TData>(source, key);

    }
}