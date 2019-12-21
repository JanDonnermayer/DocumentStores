using System;
using System.IO;

namespace DocumentStores.Internal
{
    internal interface IDataProxy
    {
        Stream GetReadStream();

        Stream GetWriteStream();

        DateTime GetVersion();

        void SetVersion(DateTime version);

        bool Exists();

        void Delete();
    }
}