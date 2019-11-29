using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <summary> </summary>
    public interface IObservableDocumentStoreBuilder : IServiceCollection
    {
        ///<summary>
        /// Adds an <see cref="IObservableDocumentStore{TData}"/> 
        /// to the <see cref="IServiceCollection"/>
        ///</summary>
        IObservableDocumentStoreBuilder WithObservableOn<TData>() where TData : class;
    }

}