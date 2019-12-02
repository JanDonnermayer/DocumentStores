using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using Newtonsoft.Json;

namespace DocumentStores.Internal
{
    internal class JsonFileHandling : IFileHandling
    {
        string IFileHandling.FileExtension<T>() =>
            ".json";

        string IFileHandling.Subdirectory<T>() =>
           typeof(T).ShortName(true).Replace(">", "}").Replace("<", "{");

        Task IFileHandling.SerializeAsync<T>(StreamWriter sw, T data)
        {
            sw.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            return Task.CompletedTask;
        }

        Task<T> IFileHandling.DeserializeAsync<T>(StreamReader sr)
        {
            return Task.FromResult(JsonConvert.DeserializeObject<T>(sr.ReadToEnd()));
        }
    }
}
