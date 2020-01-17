using System;
using System.IO;

namespace DocumentStores.Internal
{
    internal class DataChannel : IDataChannel
    {
        private readonly IDataStore store;

        private readonly DocumentAddress address;

        public DataChannel(IDataStore router, DocumentAddress address)
        {
            this.store = router ?? throw new ArgumentNullException(nameof(router));
            this.address = address;
        }

        void IDataChannel.Delete() =>
            store.Delete(address);

        bool IDataChannel.Exists() =>
            store.ContainsAddress(address);

        Stream IDataChannel.GetReadStream() =>
            store.GetReadStream(address);

        Stream IDataChannel.GetWriteStream() =>
            store.GetWriteStream(address);
    }

}
