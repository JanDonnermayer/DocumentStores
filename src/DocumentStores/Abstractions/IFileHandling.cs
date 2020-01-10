using System.IO;
using System.Threading.Tasks;

namespace DocumentStores
{
    /// <summary>
    /// Defines handling of files.
    /// </summary>
    internal interface IFileHandling
    {
        /// <inheritdoc/> 
        string FileExtension<T>() where T : class;

        /// <inheritdoc/> 
        string Subdirectory<T>() where T : class;

        /// <inheritdoc /> 
        Task<T> DeserializeAsync<T>(StreamReader reader) where T : class;

        /// <inheritdoc/> 
        Task SerializeAsync<T>(StreamWriter writer, T data) where T : class;
    }
}