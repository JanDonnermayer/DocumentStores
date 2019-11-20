using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentStores
{
    interface IFileHandling
    {
        string FileExtension { get; }

        Func<StreamReader, Task<T>> Deserialize<T>() where T : class;

        Func<T, StreamWriter, Task> Serialize<T>() where T : class;
    }
}