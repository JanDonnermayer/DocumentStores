using System;
using DocumentStores;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <inheritdoc/>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="JsonFileDocumentStore"/> as <see cref="IDocumentStore"/>
        /// to the <see cref="IServiceCollection"/>, using the specified <paramref name="rootDirectory"/>.
        /// </summary>
        /// <param name="services">The services to which the <see cref="JsonFileDocumentStore"/> is added.</param>
        /// <param name="rootDirectory">The directory that is used to store documents</param>
        /// <returns></returns>
        public static IDocumentStoreServiceCollection AddJsonFileDocumentStore(
            this IServiceCollection services, string rootDirectory
        )
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (string.IsNullOrEmpty(rootDirectory))
                throw new ArgumentException("Value cannot be null or empty.", nameof(rootDirectory));

            return new JsonFileDocumentStoreServiceCollection(
                services,
                JsonFileDocumentStoreOptions.Default.WithRootDirectory(rootDirectory)
            );
        }

        /// <summary>
        /// Adds an <see cref="JsonFileDocumentStore"/> as <see cref="IDocumentStore"/>
        /// to the <see cref="IServiceCollection"/>, using the specified <paramref name="options"/>
        /// </summary>
        /// <param name="services">The services to which the <see cref="JsonFileDocumentStore"/> is added.</param>
        /// <param name="options">The options to use.</param>
        /// <returns></returns>
        public static IDocumentStoreServiceCollection AddJsonFileDocumentStore(
            this IServiceCollection services, JsonFileDocumentStoreOptions options
        )
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            return new JsonFileDocumentStoreServiceCollection(services, options);
        }
    }
}