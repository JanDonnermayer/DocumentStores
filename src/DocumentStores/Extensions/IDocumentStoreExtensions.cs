using System.Collections;
using DocumentStores.Internal;

namespace DocumentStores
{
    public static class IDocumentStoreExtensions
    {

        public static IObservableDocumentStore<TData> AsObservableDocumentStore<TData>(this IDocumentStore source) where TData : class =>
            new ObservableDocumentStore<TData>(source);

    }
}