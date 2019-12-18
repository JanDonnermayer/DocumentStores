using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal static class IDocumentStoreInternalExtensions
    {
        public static IDocumentProxyInternal<TData> CreateProxy<TData>(
            this IDocumentStoreInternal router, 
            DocumentAddress address) => 
                new DocumentProxyInternal<TData>(router, address);
    }
}