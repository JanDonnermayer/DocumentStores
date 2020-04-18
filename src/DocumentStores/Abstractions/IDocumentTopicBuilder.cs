using Microsoft.Extensions.DependencyInjection;

namespace DocumentStores
{
    public interface IDocumentTopicBuilder
    {
        IServiceCollection Services { get; }
    }
}