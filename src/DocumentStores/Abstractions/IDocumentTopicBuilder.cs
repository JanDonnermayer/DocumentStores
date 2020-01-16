using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDocumentTopicBuilderServiceCollection : IServiceCollection
    {
        ///<summary>
        /// Adds an <see cref="IDocumentTopic{TData}"/> 
        /// to the <see cref="IServiceCollection"/>
        ///</summary>
        IDocumentTopicBuilderServiceCollection WithObservableOn<TData>() where TData : class;
    }
}