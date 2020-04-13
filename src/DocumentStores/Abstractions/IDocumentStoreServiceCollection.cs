using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    /// <summary>
    /// An <see cref="IServiceCollection"/>, that contains <see cref="IDocumentStore"/>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDocumentStoreServiceCollection : IServiceCollection {}
}