using DocumentStores.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    public static class IServiceCollectionExtensions
    {
        public static IObservableDocumentStoreBuilder AddJsonFileDocumentStore(this IServiceCollection services, string directory)
        {
            if (services is null) throw new System.ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(directory)) throw new System.ArgumentException("message", nameof(directory));

            services.AddSingleton<IDocumentStore>(_ => new JsonFileDocumentStore(directory));
            return new ObservableDocumentStoreBuilder(services);
        }

    }
}