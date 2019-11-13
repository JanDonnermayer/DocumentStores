using System.Collections;


namespace DocumentStores
{
    public static class IDocumentStoreExtensions
    {

        public static IDocumentStore<TData> AsTypedDocumentStore<TData>(this IDocumentStore source) where TData : class =>
            new DocumentStoreTypedWrapper<TData>(source);

    }
}