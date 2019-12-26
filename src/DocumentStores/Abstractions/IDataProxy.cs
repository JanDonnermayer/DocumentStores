using System;
using System.IO;

namespace DocumentStores.Internal
{
    internal interface IDataProxy
    {
        Stream GetReadStream();

        Stream GetWriteStream();

        bool Exists();

        void Delete();
    }
}