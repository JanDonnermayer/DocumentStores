using System.Collections;
using DocumentStores.Internal;

namespace DocumentStores
{
    public static class IDocumentStoreExtensions
    {
        /// <summary>
        /// Creates an <see cref="IObservableDocumentStore{TData}"/> connected to this instance of
        /// <see cref="IDocumentStore"/>
        /// </summary>
        public static IObservableDocumentStore<TData> AsObservableDocumentStore<TData>(this IDocumentStore source) where TData : class =>
            new ObservableDocumentStore<TData>(source);
    }
}