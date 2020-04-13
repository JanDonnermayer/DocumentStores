using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DocumentStores
{
    /// <summary>
    /// An implementation of IServiceCollection, that can provide a singleton instance of
    /// <see cref="JsonFileDocumentStore"/> as <see cref="IDocumentStore"/>
    /// </summary>
    internal class JsonFileDocumentStoreServiceCollection : ServiceCollection, IDocumentStoreServiceCollection
    {
        public JsonFileDocumentStoreServiceCollection(
            IServiceCollection serviceCollection,
            JsonFileDocumentStoreOptions options
        )
        {
            if (serviceCollection is null)
                throw new ArgumentNullException(nameof(serviceCollection));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            serviceCollection.AddSingleton<IDocumentStore>(_ => new JsonFileDocumentStore(options));
            var thisAsIServiceCollection = (IServiceCollection)this;
            foreach (var descriptor in serviceCollection) thisAsIServiceCollection.Add(descriptor);
        }
    }
}