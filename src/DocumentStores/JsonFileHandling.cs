using System;
using System.IO;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using Newtonsoft.Json;

namespace DocumentStores.Internal
{
    /// <summary/> 
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonFileHandling : Attribute, IFileHandling
    {        
        string IFileHandling.FileExtension<T>() => 
            ".json";

        string IFileHandling.Subdirectory<T>() =>
           typeof(T).ShortName(true).Replace(">", "}").Replace("<", "{");

        async Task IFileHandling.SerializeAsync<T>(StreamWriter sw, T data) =>
            await sw.WriteAsync(JsonConvert.SerializeObject(data, Formatting.Indented)).ConfigureAwait(false);

        async Task<T> IFileHandling.DeserializeAsync<T>(StreamReader sr)=>
            JsonConvert.DeserializeObject<T>(await sr.ReadToEndAsync().ConfigureAwait(false));


    }
}