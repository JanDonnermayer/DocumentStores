using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    public interface IObservableDocumentStoreBuilder : IServiceCollection
    {
        IObservableDocumentStoreBuilder WithObservableOn<TData>() where TData : class;
    }

}