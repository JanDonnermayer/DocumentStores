using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace DocumentStores
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IFileHandling
    {
        string FileExtension<T>() where T : class;

        string Subdirectory<T>() where T : class;

        Func<StreamReader, Task<T>> Deserialize<T>() where T : class;

        Func<T, StreamWriter, Task> Serialize<T>() where T : class;
    }
}