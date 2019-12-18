using System.IO;

namespace DocumentStores.Internal
{
    internal interface IDocumentProxyInternal<TDocument>
    {
        Stream GetReadStream();

        Stream GetWriteStream();

        bool Exists();

        void Delete();
    }
}