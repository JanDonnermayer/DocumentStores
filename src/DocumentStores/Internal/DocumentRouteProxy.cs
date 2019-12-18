using System;
using System.IO;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class DocumentRouteProxy<TDocument> : IDocumentProxy<TDocument>
    {
        private readonly IDocumentRouter router;
        private readonly DocumentAddress address;

        public DocumentRouteProxy(IDocumentRouter router, DocumentAddress address)
        {
            this.router = router ?? throw new ArgumentNullException(nameof(router));
            this.address = address;
        }

        void IDocumentProxy<TDocument>.Delete() => router.Delete<TDocument>(address);

        bool IDocumentProxy<TDocument>.Exists() => router.Exists<TDocument>(address);

        Stream IDocumentProxy<TDocument>.GetReadStream() => router.GetReadStream<TDocument>(address);

        Stream IDocumentProxy<TDocument>.GetWriteStream() => router.GetWriteStream<TDocument>(address);
    }

}
