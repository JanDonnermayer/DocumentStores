using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DocumentStores.Internal;

namespace DocumentStores
{
    /// <inheritdoc/>
    public static class IDocumentTopicExtensions
    {
        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specified <paramref name="initialData"/>.
        /// Else, Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        public static Task<IResult<TData>> AddOrUpdateAsync<TData>(
            this IDocumentTopic<TData> source, DocumentKey key,
            TData initialData, Func<TData, TData> updateData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (initialData is null)
                throw new ArgumentNullException(nameof(initialData));
            if (updateData is null)
                throw new ArgumentNullException(nameof(updateData));

            return source.AddOrUpdateAsync(
                key: key,
                addDataAsync: _ => Task.FromResult(initialData),
                updateDataAsync: (_, data) => Task.FromResult(updateData(data))
            );
        }

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the specified <paramref name="addData"/> delegate.
        /// Else, Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        public static Task<IResult<TData>> AddOrUpdateAsync<TData>(
            this IDocumentTopic<TData> source, DocumentKey key,
            Func<TData> addData, Func<TData, TData> updateData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (addData is null)
                throw new ArgumentNullException(nameof(addData));
            if (updateData is null)
                throw new ArgumentNullException(nameof(updateData));

            return source.AddOrUpdateAsync(
                key: key,
                addDataAsync: _ => Task.FromResult(addData()),
                updateDataAsync: (_, data) => Task.FromResult(updateData(data))
            );
        }

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds the specified <paramref name="initialData"/>.
        /// Else, returns it.
        /// </summary>
        public static Task<IResult<TData>> GetOrAddAsync<TData>(
            this IDocumentTopic<TData> source, DocumentKey key,
            TData initialData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (initialData is null)
                throw new ArgumentNullException(nameof(initialData));

            return source.GetOrAddAsync(
                key: key,
                addDataAsync: _ => Task.FromResult(initialData)
            );
        }

        /// <summary>
        /// If the document with the specified <paramref name="key"/> does not exist,
        /// adds it using the specified <paramref name="addData"/> delegate.
        /// Else, returns it.
        /// </summary>
        public static Task<IResult<TData>> GetOrAddAsync<TData>(
            this IDocumentTopic<TData> source, DocumentKey key,
            Func<TData> addData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (addData is null)
                throw new ArgumentNullException(nameof(addData));

            return source.GetOrAddAsync(
                key: key,
                addDataAsync: _ => Task.FromResult(addData())
            );
        }

        /// <summary>
        /// Returns instances of <typeparamref name="TData"/> contained within
        /// documents associated to this instance of <see cref="IDocumentTopic{TData}"/>
        /// based on the specified <paramref name="predicate"/>.
        /// </summary>
        public static async Task<IReadOnlyDictionary<DocumentKey, TData>> GetAllAsync<TData>(
            this IDocumentTopic<TData> source,
            Func<DocumentKey, bool> predicate) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            var keys = await source
                .GetKeysAsync()
                .ConfigureAwait(false);

            var filteredKeys = keys.Where(predicate);

            async Task<KeyValuePair<DocumentKey, IResult<TData>>> GetAsync(DocumentKey key) =>
                new KeyValuePair<DocumentKey, IResult<TData>>(
                    key: key,
                    value: await source.GetAsync(key).ConfigureAwait(false)
                );

            var results = await Task
                .WhenAll(filteredKeys.Select(GetAsync))
                .ConfigureAwait(false);

            return results
                .Where(r => r.Value.Success)
                .ToDictionary(
                    keySelector: kvp => kvp.Key,
                    elementSelector: kvp => kvp.Value.Validate()
                );
        }

        /// <summary>
        /// Returns instances of <typeparamref name="TData"/> contained within
        /// documents associated to this instance of <see cref="IDocumentTopic{TData}"/>.
        /// </summary>
        public static Task<IReadOnlyDictionary<DocumentKey, TData>> GetAllAsync<TData>(
            this IDocumentTopic<TData> source) where TData : class =>
                source.GetAllAsync(_ => true);

        /// <summary>
        /// Creates a channel for the document with the specified <paramref name="key"/>
        /// </summary>
        public static IDocumentChannel<TData> ToChannel<TData>(
            this IDocumentTopic<TData> source, DocumentKey key) where TData : class =>
                new DocumentChannel<TData>(source, key);
    }
}