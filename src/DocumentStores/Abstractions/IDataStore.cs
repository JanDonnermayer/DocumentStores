using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal interface IDataStore
    {
        Task<IEnumerable<DocumentAddress>> GetAddressesAsync(
            DocumentRoute route,
            DocumentSearchOptions options,
            CancellationToken ct = default);

        Stream GetReadStream(DocumentAddress address);

        Stream GetWriteStream(DocumentAddress address);

        bool Exists(DocumentAddress address);

        void Delete(DocumentAddress address);
    }
}