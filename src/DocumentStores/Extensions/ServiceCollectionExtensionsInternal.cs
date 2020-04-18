using System;
using DocumentStores;
using DocumentStores.Internal;
using static Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <inheritdoc/>
    internal static class ServiceCollectionExtensionsInternal
    {
        public static IGenericServiceCollection<TService> AddGeneric<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> instanceProvider,
            ServiceLifetime lifetime
        ) where TService : class
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            if (instanceProvider is null)
                throw new ArgumentNullException(nameof(instanceProvider));

            return new GenericServiceCollection<TService>(
                collection: services,
                factory: instanceProvider,
                lifetime: lifetime
            );
        }

        public static IGenericServiceCollection<TService> AddSingletonGeneric<TService>(
            this IServiceCollection services, Func<IServiceProvider, TService> instanceProvider
        ) where TService : class
        {
            return services.AddGeneric(instanceProvider, Singleton);
        }

        public static IGenericServiceCollection<TService> AddScopedGeneric<TService>(
            this IServiceCollection services, Func<IServiceProvider, TService> instanceProvider
        ) where TService : class
        {
            return services.AddGeneric(instanceProvider, Scoped);
        }

        public static IGenericServiceCollection<TService> AddTransientGeneric<TService>(
            this IServiceCollection services, Func<IServiceProvider, TService> instanceProvider
        ) where TService : class
        {
            return services.AddGeneric(instanceProvider, Transient);
        }
    }
}