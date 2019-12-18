using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal interface IDocumentStoreInternal
    {
        Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute route,
            DocumentSearchOptions options,
            CancellationToken ct = default);

        Stream GetReadStream<TData>(DocumentAddress address);

        Stream GetWriteStream<TData>(DocumentAddress address);

        bool Exists<TData>(DocumentAddress address);

        void Delete<TData>(DocumentAddress address);
    }
}