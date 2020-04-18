using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DocumentStores.Internal
{
    /// <summary>
    /// An instance of <see cref="IServiceCollection"/>
    /// containing a singleton instance of <typeparam name="TService"/>
    /// </summary>
    internal class GenericServiceCollection<TService>
        : ServiceCollection, IGenericServiceCollection<TService>  where TService : class
    {
        public GenericServiceCollection(
            IServiceCollection serviceCollection,
            TService service
        ) : this(serviceCollection, _ => service) {}

        public GenericServiceCollection(
            IServiceCollection serviceCollection,
            Func<IServiceProvider, TService> serviceFactory
        )
        {
            if (serviceCollection is null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (serviceFactory is null)
                throw new ArgumentNullException(nameof(serviceFactory));

            serviceCollection.AddSingleton(serviceFactory);
            var thisAsIServiceCollection = (IServiceCollection)this;
            foreach (var descriptor in serviceCollection) thisAsIServiceCollection.Add(descriptor);
        }
    }
}