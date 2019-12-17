using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores
{
    internal interface IDocumentRouter
    {
        Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TDocument>(
            DocumentRoute route, 
            DocumentSearchOptions options, 
            CancellationToken ct = default);

        Stream GetReadStream<TDocument>(DocumentAddress address);

        Stream GetWriteStream<TDocument>(DocumentAddress address);

        bool Exists<TDocument>(DocumentAddress address);

        void Delete<TDocument>(DocumentAddress address);
    }
}