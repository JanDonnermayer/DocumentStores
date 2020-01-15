using System;
using System.IO;

namespace DocumentStores.Internal
{
    internal interface IDataChannel
    {
        Stream GetReadStream();

        Stream GetWriteStream();

        bool Exists();

        void Delete();
    }
}