using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public static Task<Result<TData>> AddOrUpdateAsync<TData>(
            this IDocumentTopic<TData> source, DocumentKey key,
            TData initialData, Func<TData, TData> updateData) where TData : class =>
                source.AddOrUpdateAsync(
                    key: key,
                    addDataAsync: _ => Task.FromResult(initialData),
                    updateDataAsync: (_, data) => Task.FromResult(updateData(data)));


        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specfied <paramref name="initialData"/>.
        /// Else: Returns it.
        /// </summary>
        public static Task<Result<TData>> GetOrAddAsync<TData>(
            this IDocumentTopic<TData> source, DocumentKey key,
            TData initialData) where TData : class =>
                source.GetOrAddAsync(
                    key: key,
                    addDataAsync: _ => Task.FromResult(initialData));

        /// <summary>
        /// Returns instances of <typeparamref name="TData"/> contained within
        /// documents associated to this instance of <see cref="IDocumentTopic{TData}"/>
        /// based on the specified <paramref name="predicate"/>.
        /// </summary>
        public static async Task<IReadOnlyDictionary<DocumentKey, TData>> GetAllAsync<TData>(
            this IDocumentTopic<TData> source,
            Func<DocumentKey, bool> predicate) where TData : class
        {
            var keys = await source
                .GetKeysAsync()
                .ConfigureAwait(false);

            var filteredKeys = keys.Where(predicate);

            async Task<KeyValuePair<DocumentKey, Result<TData>>> GetAsync(DocumentKey key) =>
                new KeyValuePair<DocumentKey, Result<TData>>(key, await source.GetAsync(key));

            var results = await Task
                .WhenAll(filteredKeys.Select(GetAsync))
                .ConfigureAwait(false);

            return results
                .Where(r => r.Value.Try())
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.PassOrThrow());
        }

        /// <summary>
        /// Returns instances of <typeparamref name="TData"/> contained within
        /// documents associated to this instance of <see cref="IDocumentTopic{TData}"/>.
        /// </summary>
        public static Task<IReadOnlyDictionary<DocumentKey, TData>> GetAllAsync<TData>(
            this IDocumentTopic<TData> source) where TData : class =>
                source.GetAllAsync(_ => true);

        /// <InheritDoc/>
        public static async Task<Result<Unit>[]> SynchronizeAsync<T>(
            this IDocumentTopic<T> source, IDocumentTopic<T> target) where T : class
        {
            var sourceDict = await source.GetAllAsync();
            var targetKeys = await target.GetKeysAsync();

            var surplusTargetKeys = targetKeys
                .Except(sourceDict.Keys)
                .ToImmutableHashSet();

            var copyResults = await Task
                .WhenAll(sourceDict.Select(kvp => target.PutAsync(kvp.Key, kvp.Value)))
                .ConfigureAwait(false);

            var deleteResults = await Task
                .WhenAll(surplusTargetKeys.Select(target.DeleteAsync))
                .ConfigureAwait(false);

            return copyResults
                .Concat(deleteResults)
                .ToArray();
        }

        /// <summary>
        /// Creates a proxy for the document with the specified <paramref name="key"/>
        /// </summary>
        public static IDocumentProxy<TData> CreateProxy<TData>(
            this IDocumentTopic<TData> source, DocumentKey key) where TData : class =>
                new DocumentProxy<TData>(source, key);

    }
}