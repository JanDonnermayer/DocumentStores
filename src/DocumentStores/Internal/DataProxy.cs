using System;
using System.IO;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class DataProxy : IDataProxy
    {
        private readonly IDataStore store;

        private readonly DocumentAddress address;

        public DataProxy(IDataStore router, DocumentAddress address)
        {
            this.store = router ?? throw new ArgumentNullException(nameof(router));
            this.address = address;
        }

        DateTime IDataProxy.GetVersion() => 
            store.GetVersion(address);

        void IDataProxy.SetVersion(DateTime version) => 
            store.SetVersion(address, version);

        void IDataProxy.Delete() => 
            store.Delete(address);

        bool IDataProxy.Exists() => 
            store.Exists(address);

        Stream IDataProxy.GetReadStream() => 
            store.GetReadStream(address);

        Stream IDataProxy.GetWriteStream() => 
            store.GetWriteStream(address);
    }

}
