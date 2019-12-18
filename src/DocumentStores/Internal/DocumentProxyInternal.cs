using System;
using System.IO;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class DocumentProxyInternal<TData> : IDocumentProxyInternal<TData>
    {
        private readonly IDocumentStoreInternal router;

        private readonly DocumentAddress address;

        public DocumentProxyInternal(IDocumentStoreInternal router, DocumentAddress address)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
            this.address = address;
        }

        void IDocumentProxyInternal<TData>.Delete() => 
            router.Delete<TData>(address);

        bool IDocumentProxyInternal<TData>.Exists() => 
            router.Exists<TData>(address);

        Stream IDocumentProxyInternal<TData>.GetReadStream() => 
            router.GetReadStream<TData>(address);

        Stream IDocumentProxyInternal<TData>.GetWriteStream() => 
            router.GetWriteStream<TData>(address);
    }

}
