using DocumentStores.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <inheritdoc/>
    public static class IDocumentStoreServiceCollectionExtensions
    {
        ///<summary>
        /// Adds an <see cref="IDocumentTopic{TData}"/> with the specified <paramref name="route"/>
        /// to the <see cref="IServiceCollection"/>.
        ///</summary>
        public static IServiceCollection AddDocumentTopic<T>(
            this IDocumentStoreServiceCollection serviceCollection,
            DocumentRoute route
        ) where T : class
        {
            if (serviceCollection is null)
                throw new System.ArgumentNullException(nameof(serviceCollection));

            return serviceCollection.AddSingleton<IDocumentTopic<T>>(
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
        public static IServiceCollection AddDocumentTopic<T>(
            this IDocumentStoreServiceCollection serviceCollection
        ) where T : class
        {
            return serviceCollection.AddDocumentTopic<T>(DocumentRoute.Default);
        }
    }
}