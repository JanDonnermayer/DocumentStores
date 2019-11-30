using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace DocumentStores
{
    /// <summary>
    /// Defines handling of files.
    /// </summary>
    internal interface IFileHandling
    {
        /// <summary/> 
        string FileExtension<T>() where T : class;

        /// <summary/> 
        string Subdirectory<T>() where T : class;

        /// <summary/> 
        Task<T> DeserializeAsync<T>(StreamReader reader) where T : class;

        /// <summary/> 
        Task SerializeAsync<T>(StreamWriter writer, T data) where T : class;
    }
}