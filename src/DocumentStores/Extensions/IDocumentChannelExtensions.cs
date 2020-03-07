using System;
using System.Threading.Tasks;

namespace DocumentStores
{
    /// <inheritdoc /> 
    public static class IDocumentChannelExtensions
    {
        /// <summary>
        /// If the document does not exist,
        /// adds the specified <paramref name="initialData"/>.
        /// Else, Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        public static Task<IResult<TData>> AddOrUpdateAsync<TData>(
            this IDocumentChannel<TData> source,
            TData initialData, Func<TData, TData> updateData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (initialData is null)
                throw new ArgumentNullException(nameof(initialData));
            if (updateData is null)
                throw new ArgumentNullException(nameof(updateData));

            return (source ?? throw new ArgumentNullException(nameof(source))).AddOrUpdateAsync(
                addDataAsync: () => Task.FromResult(initialData),
                updateDataAsync: data => Task.FromResult(updateData(data))
            );
        }

        /// <summary>
        /// If the document does not exist,
        /// adds it using the specified <paramref name="addData"/> delegate.
        /// Else, Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        public static Task<IResult<TData>> AddOrUpdateAsync<TData>(
            this IDocumentChannel<TData> source,
            Func<TData> addData, Func<TData, TData> updateData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (addData is null)
                throw new ArgumentNullException(nameof(addData));
            if (updateData is null)
                throw new ArgumentNullException(nameof(updateData));

            return (source ?? throw new ArgumentNullException(nameof(source))).AddOrUpdateAsync(
                addDataAsync: () => Task.FromResult(addData()),
                updateDataAsync: data => Task.FromResult(updateData(data))
            );
        }

        /// <summary>
        /// If the document does not exist,
        /// adds the specified <paramref name="initialData"/>.
        /// Else, returns it.
        /// </summary>
        public static Task<IResult<TData>> GetOrAddAsync<TData>(
            this IDocumentChannel<TData> source,
            TData initialData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            return source.GetOrAddAsync(
                addDataAsync: () => Task.FromResult(initialData)
            );
        }

        /// <summary>
        /// If the document does not exist,
        /// adds it using the specified <paramref name="addData"/> delegate.
        /// Else, returns it.
        /// </summary>
        public static Task<IResult<TData>> GetOrAddAsync<TData>(
            this IDocumentChannel<TData> source,
            Func<TData> addData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (addData is null)
                throw new ArgumentNullException(nameof(addData));

            return source.GetOrAddAsync(
                addDataAsync: () => Task.FromResult(addData())
            );
        }
    }
}