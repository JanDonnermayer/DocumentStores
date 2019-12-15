using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores
{
    /// <summary/> 
    public static class IDocumentStoreExtensions
    {
        /// <summary>
        /// Creates an <see cref="IDocumentTopic{TData}"/> connected to this instance of
        /// <see cref="IDocumentStore"/>
        /// </summary>
        public static IDocumentTopic<TData> CreateTopic<TData>(
            this IDocumentStore source, DocumentRoute route) where TData : class =>
                new DocumentTopic<TData>(source, route);

        /// <summary>
        /// Creates an <see cref="IDocumentTopic{TData}"/> connected to this instance of
        /// <see cref="IDocumentStore"/>
        /// </summary>
        public static IDocumentTopic<TData> CreateTopic<TData>(
            this IDocumentStore source, params string[] routeSegments) where TData : class =>
                new DocumentTopic<TData>(source, DocumentRoute.Create(routeSegments));

        /// <summary>
        /// Returns addresses, associated to documents of <typeparamref name="TData"/>.
        /// </summary>
        public static Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            this IDocumentStore store,
            DocumentSearchOptions options = DocumentSearchOptions.AllLevels,
            CancellationToken ct = default) where TData : class =>
                store.GetAddressesAsync<TData>(DocumentRoute.Default, options, ct);
    }
}