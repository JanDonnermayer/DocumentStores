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
            if (SynchronizationContext.Current == null)
            {
                await sw.WriteAsync(JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            else
            {
                sw.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            }
        }

        async Task<T> IFileHandling.DeserializeAsync<T>(StreamReader sr)
        {
            if (SynchronizationContext.Current == null)
            {
                return JsonConvert.DeserializeObject<T>(await sr.ReadToEndAsync());
            }
            else
            {
                return await Task.FromResult(JsonConvert.DeserializeObject<T>(sr.ReadToEnd()));
            }
        }
    }
}
