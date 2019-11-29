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

        Task<T> DeserializeAsync<T>(StreamReader reader) where T : class;

        Task SerializeAsync<T>(StreamWriter writer, T data) where T : class;
    }
}