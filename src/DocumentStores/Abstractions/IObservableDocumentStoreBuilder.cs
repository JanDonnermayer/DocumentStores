using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IObservableDocumentStoreBuilder : IServiceCollection
    {
        ///<summary>
        /// Adds an <see cref="IObservableDocumentStore{TData}"/> 
        /// to the <see cref="IServiceCollection"/>
        ///</summary>
        IObservableDocumentStoreBuilder WithObservableOn<TData>() where TData : class;
    }
}