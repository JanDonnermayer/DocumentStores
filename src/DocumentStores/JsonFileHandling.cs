using System;
using System.IO;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using Newtonsoft.Json;

namespace DocumentStores.Internal
{
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonFileHandling : Attribute, IFileHandling
    {        
        string IFileHandling.FileExtension<T>() => 
            ".json";

        string IFileHandling.Subdirectory<T>() =>
           typeof(T).ShortName(true).Replace(">", "}").Replace("<", "{");

        Func<T, StreamWriter, Task> IFileHandling.Serialize<T>() =>
            async (data, sw) => await sw.WriteAsync(JsonConvert.SerializeObject(data, Formatting.Indented));

        Func<StreamReader, Task<T>> IFileHandling.Deserialize<T>()=>
            async sr => JsonConvert.DeserializeObject<T>(await sr.ReadToEndAsync());


    }
}