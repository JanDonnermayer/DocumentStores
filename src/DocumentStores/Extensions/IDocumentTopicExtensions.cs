using System;
using System.Collections.Generic;
using System.Linq;
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
                    key: key,
                    addDataAsync: _ => Task.FromResult(initialData),
                    updateDataAsync: (_, data) => Task.FromResult(updateData(data)));


        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Returns it.
        /// </summary>
        public static Task<Result<TData>> GetOrAddDocumentAsync<TData>(
            this IDocumentTopic<TData> source, DocumentKey key,
            TData initialData) where TData : class =>
                source.GetOrAddDocumentAsync(
                    key: key,
                    addDataAsync: _ => Task.FromResult(initialData));

        /// <summary>
        /// Returns instances of <typeparamref name="TData"/> contained within
        /// documents associated to this instance of <see cref="IDocumentTopic{TData}"/>
        /// based on the specified <paramref name="predicate"/>.
        /// </summary>
        public static async Task<IEnumerable<TData>> GetDocumentsAsync<TData>(
            this IDocumentTopic<TData> source,
            Func<DocumentKey, bool> predicate) where TData : class
        {
            var keys = await source
                .GetKeysAsync()
                .ConfigureAwait(false);

            var resuts = await Task
                .WhenAll(keys.Where(predicate)
                .Select(source.GetDocumentAsync))
                .ConfigureAwait(false);

            return resuts
                .Where(r => r.Try())
                .Select(r => r.PassOrThrow());
        }

        /// <summary>
        /// Returns instances of <typeparamref name="TData"/> contained within
        /// documents associated to this instance of <see cref="IDocumentTopic{TData}"/>.
        /// </summary>
        public static Task<IEnumerable<TData>> GetDocumentsAsync<TData>(
            this IDocumentTopic<TData> source) where TData : class =>
                source.GetDocumentsAsync(_ => true);

        /// <summary>
        /// Creates a proxy for the document with the specified <paramref name="key"/>
        /// </summary>
        public static IDocumentProxy<TData> CreateProxy<TData>(
            this IDocumentTopic<TData> source, DocumentKey key) where TData : class =>
                new DocumentProxy<TData>(source, key);

    }
}