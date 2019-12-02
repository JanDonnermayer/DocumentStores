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

        async Task IFileHandling.SerializeAsync<T>(StreamWriter sw, T data)
        {
            await Task.Factory.StartNew(() => sw.Write(JsonConvert.SerializeObject(data, Formatting.Indented)));
        }

        async Task<T> IFileHandling.DeserializeAsync<T>(StreamReader sr)
        {
            return await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(sr.ReadToEnd()));
        }
    }
}
