using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DocumentStores.Internal
{
    /// <inheritdoc />
    internal class GenericServiceCollection<TService>
        : ServiceCollection, IGenericServiceCollection<TService>  where TService : class
    {
        public GenericServiceCollection(
            IServiceCollection collection,
            Func<IServiceProvider, TService> factory,
            ServiceLifetime lifetime
        )
        {
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));

            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            collection.Add(new ServiceDescriptor(typeof(TService), factory, lifetime));
            var thisCollection = (IServiceCollection)this;
            foreach (var descriptor in collection) thisCollection.Add(descriptor);
        }
    }
}