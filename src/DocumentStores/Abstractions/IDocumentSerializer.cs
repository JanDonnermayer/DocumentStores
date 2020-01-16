using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace DocumentStores
{
    /// <summary>
    /// Provides serialization.
    /// </summary>
    internal interface IDocumentSerializer
    {
        Task<T> DeserializeAsync<T>(Stream stream) where T : class;

        Task SerializeAsync<T>(Stream stream, T data) where T : class;
    }
}