using System;
using System.IO;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class DocumentProxyInternal<TDocument> : IDocumentProxyInternal<TDocument>
    {
        private readonly IDocumentRouter router;
        
        private readonly DocumentAddress address;

        public DocumentProxyInternal(IDocumentRouter router, DocumentAddress address)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
            this.address = address;
        }

        void IDocumentProxyInternal<TDocument>.Delete() => 
            router.Delete<TDocument>(address);

        bool IDocumentProxyInternal<TDocument>.Exists() => 
            router.Exists<TDocument>(address);

        Stream IDocumentProxyInternal<TDocument>.GetReadStream() => 
            router.GetReadStream<TDocument>(address);

        Stream IDocumentProxyInternal<TDocument>.GetWriteStream() => 
            router.GetWriteStream<TDocument>(address);
    }

}
