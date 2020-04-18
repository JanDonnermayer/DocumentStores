using System;
using DocumentStores;
using DocumentStores.Internal;
using static Microsoft.Extensions.DependencyInjection.ServiceLifetime;

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
        ) => services.AddJsonFileDocumentStore(
                JsonFileDocumentStoreOptions.Default.WithRootDirectory(rootDirectory)
            );

        /// <summary>
        /// Adds an <see cref="JsonFileDocumentStore"/> as <see cref="IDocumentStore"/>
        /// to the <see cref="IServiceCollection"/>, using the specified <paramref name="options"/>
        /// </summary>
        /// <param name="services">The services to which the <see cref="JsonFileDocumentStore"/> is added.</param>
        /// <param name="options">The options to use.</param>
        /// <returns></returns>
        public static IGenericServiceCollection<IDocumentStore> AddJsonFileDocumentStore(
            this IServiceCollection services, JsonFileDocumentStoreOptions options
        ) => services.AddSingletonGeneric<IDocumentStore>(
                _ => new JsonFileDocumentStore(options)
            );
    }
}