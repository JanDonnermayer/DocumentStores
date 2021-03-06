using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DocumentStores.Internal
{
    internal class TranslatedDataStore : IDataStore
    {
        private readonly IDataStore source;
        private readonly Func<DocumentRoute, DocumentRoute> translateIn;
        private readonly Func<DocumentRoute, DocumentRoute> translateOut;

        private DocumentAddress TranslateIn(DocumentAddress address) =>
            address.MapRoute(translateIn);

        private DocumentAddress TranslateOut(DocumentAddress address) =>
            address.MapRoute(translateOut);

        public TranslatedDataStore(
            IDataStore source,
            Func<DocumentRoute, DocumentRoute> translateIn,
            Func<DocumentRoute, DocumentRoute> translateOut)
        {
            this.source = source;
            this.translateIn = translateIn;
            this.translateOut = translateOut;
        }

        public void Clear() => source.Clear();

        public void Delete(DocumentAddress address) =>
            source.Delete(TranslateIn(address));

        public bool ContainsAddress(DocumentAddress address) =>
            source.ContainsAddress(TranslateIn(address));

        public IEnumerable<DocumentAddress> GetAddresses(DocumentRoute route, DocumentSearchOption options) =>
            source.GetAddresses(translateIn(route), options).Select(TranslateOut);

        public Stream GetReadStream(DocumentAddress address) =>
            source.GetReadStream(TranslateIn(address));

        public Stream GetWriteStream(DocumentAddress address) =>
            source.GetWriteStream(TranslateIn(address));
    }
}