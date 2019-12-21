using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    interface IDataStore
    {
        IEnumerable<DocumentAddress> GetAddresses(
            DocumentRoute route, DocumentSearchOptions options);

        Stream GetReadStream(DocumentAddress address);

        Stream GetWriteStream(DocumentAddress address);

        DateTime GetVersion(DocumentAddress address);

        void SetVersion(DocumentAddress address, DateTime version);

        bool Exists(DocumentAddress address);

        void Delete(DocumentAddress address);

        void Clear();
    }
}