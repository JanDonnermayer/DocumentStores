using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Internal;

namespace DocumentStores
{
    /// <inheritdoc/>
    public static class IDocumentStoreExtensions
    {
        /// <summary>
        /// If the document with the specified <paramref name="address"/> does not exist,
        /// adds the specified <paramref name="initialData"/>.
        /// Else, Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        public static Task<IResult<TData>> AddOrUpdateAsync<TData>(
            this IDocumentStore source, DocumentAddress address,
            TData initialData, Func<TData, TData> updateData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (initialData is null)
                throw new ArgumentNullException(nameof(initialData));
            if (updateData is null)
                throw new ArgumentNullException(nameof(updateData));

            return source.AddOrUpdateAsync(
                address: address,
                addDataAsync: _ => Task.FromResult(initialData),
                updateDataAsync: (_, data) => Task.FromResult(updateData(data))
            );
        }

        /// <summary>
        /// If the document with the specified <paramref name="address"/> does not exist,
        /// adds it using the specified <paramref name="addData"/> delegate.
        /// Else, Updates it using the specified <paramref name="updateData"/> delegate.
        /// </summary>
        public static Task<IResult<TData>> AddOrUpdateAsync<TData>(
            this IDocumentStore source, DocumentAddress address,
            Func<TData> addData, Func<TData, TData> updateData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (addData is null)
                throw new ArgumentNullException(nameof(addData));
            if (updateData is null)
                throw new ArgumentNullException(nameof(updateData));

            return source.AddOrUpdateAsync(
                address: address,
                addDataAsync: _ => Task.FromResult(addData()),
                updateDataAsync: (_, data) => Task.FromResult(updateData(data))
            );
        }

        /// <summary>
        /// If the document with the specified <paramref name="address"/> does not exist,
        /// adds the specified <paramref name="initialData"/>.
        /// Else, returns it.
        /// </summary>
        public static Task<IResult<TData>> GetOrAddAsync<TData>(
            this IDocumentStore source, DocumentAddress address,
            TData initialData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (initialData is null)
                throw new ArgumentNullException(nameof(initialData));

            return source.GetOrAddAsync(
                address: address,
                addDataAsync: _ => Task.FromResult(initialData)
            );
        }

        /// <summary>
        /// If the document with the specified <paramref name="address"/> does not exist,
        /// adds it using the specified <paramref name="addData"/> delegate.
        /// Else, returns it.
        /// </summary>
        public static Task<IResult<TData>> GetOrAddAsync<TData>(
            this IDocumentStore source, DocumentAddress address,
            Func<TData> addData) where TData : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (addData is null)
                throw new ArgumentNullException(nameof(addData));

            return source.GetOrAddAsync(
                address: address,
                addDataAsync: _ => Task.FromResult(addData())
            );
        }

        /// <summary>
        /// Returns addresses, associated to documents of <typeparamref name="TData"/>.
        /// </summary>
        public static Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            this IDocumentStore store,
            DocumentSearchOption options = DocumentSearchOption.AllLevels,
            CancellationToken ct = default) where TData : class
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            return store.GetAddressesAsync<TData>(DocumentRoute.Default, options, ct);
        }

        /// <summary>
        /// Creates an <see cref="IDocumentTopic{TData}"/> connected to this instance of
        /// <see cref="IDocumentStore"/>
        /// </summary>
        public static IDocumentTopic<TData> ToTopic<TData>(
            this IDocumentStore source, DocumentRoute route) where TData : class =>
                new DocumentTopic<TData>(source, route);

        /// <summary>
        /// Creates an <see cref="IDocumentTopic{TData}"/> connected to this instance of
        /// <see cref="IDocumentStore"/>
        /// </summary>
        public static IDocumentTopic<TData> ToTopic<TData>(
            this IDocumentStore source, params string[] routeSegments) where TData : class =>
                new DocumentTopic<TData>(source, DocumentRoute.Create(routeSegments));
    }
}