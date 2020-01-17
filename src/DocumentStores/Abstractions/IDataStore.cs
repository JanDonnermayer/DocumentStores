using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentStores
{
    internal interface IDataStore
    {
        IEnumerable<DocumentAddress> GetAddresses(
            DocumentRoute route, DocumentSearchOption options);

        Stream GetReadStream(DocumentAddress address);

        Stream GetWriteStream(DocumentAddress address);

        bool ContainsAddress(DocumentAddress address);

        void Delete(DocumentAddress address);

        void Clear();
    }
}