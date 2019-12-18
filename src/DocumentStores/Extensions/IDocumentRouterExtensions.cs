using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal static class IDocumentRouterExtensions
    {
        public static IDocumentProxyInternal<TDocument> CreateProxy<TDocument>(this IDocumentRouter router, DocumentAddress address) => 
            new DocumentProxyInternal<TDocument>(router, address);
    }
}