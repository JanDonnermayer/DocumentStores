using System;
using System.IO;
using System.Threading.Tasks;
using DocumentStores.Primitives;
using Newtonsoft.Json;

namespace DocumentStores.Internal
{
    class JsonHandling : IFileHandling, IResultHandling 
    {
        string IFileHandling.FileExtension => ".json";

        Func<T, StreamWriter, Task> IFileHandling.Serialize<T>() =>
             async (data, sw) => await sw.WriteAsync(JsonConvert.SerializeObject(data, Formatting.Indented));

        Func<StreamReader, Task<T>> IFileHandling.Deserialize<T>()=>
           async sr => JsonConvert.DeserializeObject<T>(await sr.ReadToEndAsync());

        Func<Func<Task<T>>, Func<Task<Result<T>>>> IResultHandling.Catch<T>() =>
            producer =>
                producer.Catch(
                    exceptionFilter: ex =>
                        ex is JsonException
                        || ex is DocumentException
                        || ex is IOException);

        Func<Func<Task<Result<T>>>, Func<Task<Result<T>>>> IResultHandling.Retry<T>() =>
            producer =>
                producer.RetryIncrementally(
                    frequencySeed: TimeSpan.FromMilliseconds(50),
                    count: 5,
                    exceptionFilter: ex => ex is IOException);

    }
}