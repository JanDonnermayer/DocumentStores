using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace DocumentStores
{
    /// <summary>
    /// Defines handling of files.
    /// </summary>
    internal interface IDocumentSerializer
    {
        /// <summary/> 
        Task<T> DeserializeAsync<T>(Stream stream) where T : class;

        /// <summary/> 
        Task SerializeAsync<T>(Stream stream, T data) where T : class;
    }
}