using System.IO;

namespace DocumentStores.Internal
{
    internal interface IDocumentProxyInternal<TData>
    {
        Stream GetReadStream();

        Stream GetWriteStream();

        bool Exists();

        void Delete();
    }
}