using System.Collections;
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
        public static IDocumentTopic<TData> CreateTopic<TData>(this IDocumentStore source, DocumentRoute path) where TData : class =>
            new DocumentTopic<TData>(source, path);

        /// <summary>
        /// Creates an <see cref="IDocumentTopic{TData}"/> connected to this instance of
        /// <see cref="IDocumentStore"/>
        /// </summary>
        public static IDocumentTopic<TData> CreateTopic<TData>(this IDocumentStore source) where TData : class =>
            new DocumentTopic<TData>(source, DocumentRoute.Default);
    }
}