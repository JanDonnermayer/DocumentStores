using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal static class IDocumentRouterExtensions
    {
        public static IDocumentProxy<TDocument> CreateProxy<TDocument>(this IDocumentRouter router, DocumentAddress address) => 
            new DocumentRouteProxy<TDocument>(router, address);
    }
}