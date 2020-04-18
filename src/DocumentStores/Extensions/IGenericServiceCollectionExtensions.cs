using System;
using DocumentStores.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <inheritdoc/>
    public static class IGenericServiceCollectionExtensions
    {
        ///<summary>
        /// Adds an <see cref="IDocumentTopic{TData}"/> with the specified <paramref name="route"/>
        /// to the <see cref="IServiceCollection"/>.
        ///</summary>
        public static IGenericServiceCollection<IDocumentTopic<T>> AddDocumentTopic<T>(
            this IGenericServiceCollection<IDocumentStore> serviceCollection,
            DocumentRoute route
        ) where T : class
        {
            if (serviceCollection is null)
                throw new ArgumentNullException(nameof(serviceCollection));

            return serviceCollection.AddSingletonGeneric<IDocumentTopic<T>>(
                sp => new DocumentTopic<T>(
                    source: sp.GetRequiredService<IDocumentStore>(),
                    route: route
                )
            );
        }

        ///<summary>
        /// Adds an <see cref="IDocumentTopic{TData}"/> with default route
        /// to the <see cref="IServiceCollection"/>.
        ///</summary>
        public static IGenericServiceCollection<IDocumentTopic<T>> AddDocumentTopic<T>(
            this IGenericServiceCollection<IDocumentStore> serviceCollection
        ) where T : class
        {
            return serviceCollection.AddDocumentTopic<T>(DocumentRoute.Default);
        }

        ///<summary>
        /// Adds an <see cref="IDocumentChannel{TData}"/> with default route
        /// to the <see cref="IServiceCollection"/>.
        ///</summary>
        public static IGenericServiceCollection<IDocumentChannel<T>> AddDocumentChannel<T>(
            this IGenericServiceCollection<IDocumentTopic<T>> serviceCollection,
            DocumentKey key
        ) where T : class
        {
            if (serviceCollection is null)
                throw new ArgumentNullException(nameof(serviceCollection));

            return serviceCollection.AddSingletonGeneric<IDocumentChannel<T>>(
                sp => new DocumentChannel<T>(
                    topic: sp.GetRequiredService<IDocumentTopic<T>>(),
                    key: key
                )
            );
        }
    }
}