using System.IO;

namespace DocumentStores
{
    internal interface IDocumentProxy<TData>
    {
        Stream GetReadStream();

        Stream GetWriteStream();

        bool Exists();

        void Delete();
    }
}