using System;
using DocumentStores;
using DocumentStores.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <inheritdoc/>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="JsonFileDocumentStore"/> as <see cref="IDocumentStore"/>
        /// to the <see cref="IServiceCollection"/>, using the specified <paramref name="rootDirectory"/>.
        /// </summary>
        /// <param name="services">The services to which the <see cref="JsonFileDocumentStore"/> is added.</param>
        /// <param name="rootDirectory">The directory that is used to store documents</param>
        /// <returns></returns>
        public static IGenericServiceCollection<IDocumentStore> AddJsonFileDocumentStore(
            this IServiceCollection services, string rootDirectory
        )
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            return new GenericServiceCollection<IDocumentStore>(
                serviceCollection: services,
                service: new JsonFileDocumentStore(
                    JsonFileDocumentStoreOptions.Default.WithRootDirectory(rootDirectory)
                )
            );
        }

        /// <summary>
        /// Adds an <see cref="JsonFileDocumentStore"/> as <see cref="IDocumentStore"/>
        /// to the <see cref="IServiceCollection"/>, using the specified <paramref name="options"/>
        /// </summary>
        /// <param name="services">The services to which the <see cref="JsonFileDocumentStore"/> is added.</param>
        /// <param name="options">The options to use.</param>
        /// <returns></returns>
        public static IGenericServiceCollection<IDocumentStore> AddJsonFileDocumentStore(
            this IServiceCollection services, JsonFileDocumentStoreOptions options
        )
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            return new GenericServiceCollection<IDocumentStore>(
                serviceCollection: services,
                service: new JsonFileDocumentStore(options)
            );
        }

        internal static IGenericServiceCollection<TService> AddSingletonGeneric<TService>(
            this IServiceCollection services, Func<IServiceProvider, TService> instanceProvider
        ) where TService : class
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            return new GenericServiceCollection<TService>(
                services,
                instanceProvider
            );
        }
    }
}