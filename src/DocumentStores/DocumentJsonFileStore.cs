using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Newtonsoft.Json;
using System.Collections.Immutable;
using DocumentStores.Primitives;
using DocumentStores.Abstractions;

namespace DocumentStores
{
    public class DocumentJsonFileStore : IDocumentStore
    {

        private ImmutableDictionary<string, SemaphoreSlim> locks =
            ImmutableDictionary<string, SemaphoreSlim>.Empty;

        private ILogger<DocumentJsonFileStore> Logger { get; }
        private string RootDirectory { get; }
        private JsonSerializerOptions SerializerSettings { get; }

        #region Private Members

        private async Task<IDisposable> GetLockAsync(string key)
        {
            var sem = ImmutableInterlocked.GetOrAdd(ref locks, key, s => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            return new Disposable(() => sem.Release());
        }

        private string FilePrefix => @$"{RootDirectory}\";

        private string FileSuffix<T>() =>
            $".{typeof(T).ShortName(true).Replace(">", "]").Replace("<", "[")}.json";

        private string GetFileName<T>(string key) =>
            this.FilePrefix + key + FileSuffix<T>();

        private string GetKey<T>(string file)
        {
            var subs1 = file.Substring(this.FilePrefix.Length);
            var subs2 = subs1.Substring(0, subs1.Length - FileSuffix<T>().Length);
            return subs2;
        }

        private static async Task<OperationResult<T>> Deserialize<T>(StreamReader SR) =>
            JsonConvert.DeserializeObject<T>(await SR.ReadToEndAsync());
        // JsonSerializer.Deserialize<T>(await SR.ReadToEndAsync(), this.SerializerSettings);

        private static async Task Serialize<T>(T data, StreamWriter SW) =>
            await SW.WriteAsync(JsonConvert.SerializeObject(data, Formatting.Indented));
        //await SW.WriteAsync(JsonSerializer.Serialize(data, this.SerializerSettings));

        #endregion

        #region Constructor

        public DocumentJsonFileStore(string directory, ILogger<DocumentJsonFileStore> logger = default)
        {
            this.RootDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
            this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.SerializerSettings = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            if (!new DirectoryInfo(directory).Exists)
            {
                Directory.CreateDirectory(directory);
                Logger.LogInformation($"Created directory '{directory}'");
            }
        }

        #endregion

        #region Implementation of IDocumentStore

        public Task<IEnumerable<string>> GetKeysAsync<T>() =>
           Task.Run(() => Directory.EnumerateFiles(this.RootDirectory, "*" + FileSuffix<T>(), SearchOption.TopDirectoryOnly).Select(GetKey<T>));

        public async Task<OperationResult<T>> GetDocumentAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var file = GetFileName<T>(key);
            if (!File.Exists(file)) return "No such document";

            using (await GetLockAsync(file))
            {
                try
                {
                    using FileStream FS = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using StreamReader SR = new StreamReader(FS);
                    return await Deserialize<T>(SR);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    return ex.Message;
                }
            }
        }


        public async Task<OperationResult> PutDocumentAsync<T>(string key, T data)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var file = GetFileName<T>(key);

            using (await GetLockAsync(file))
            {
                try
                {
                    using FileStream FS = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
                    using StreamWriter SW = new StreamWriter(FS);

                    await Serialize(data, SW);

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    return ex.Message;
                }
            }
        }

        public async Task<OperationResult> TransformDocumentAsync<T>(string key, Func<T, T> transfomer)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var file = GetFileName<T>(key);

            using (await GetLockAsync(file))
            {
                try
                {
                    using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    using StreamReader SR = new StreamReader(FS);
                    using StreamWriter SW = new StreamWriter(FS);

                    var json = await SR.ReadToEndAsync();
                    var originalContent = string.IsNullOrEmpty(json) ? default : JsonConvert.DeserializeObject<T>(json);
                    FS.Position = 0;

                    var transformedContent = transfomer(originalContent) ?? throw new Exception("Transformer returned null!");
                    await SW.WriteAsync(JsonConvert.SerializeObject(transformedContent, Formatting.Indented));
                    SW.Flush();

                    FS.SetLength(FS.Position);

                    return true;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                    return ex.Message;
                }
            }
        }

        public async Task<OperationResult> DeleteDocumentAsync<T>(string key)
        {
            using (await GetLockAsync(key))
            {

                try
                {
                    File.Delete(GetFileName<T>(key));
                    return true;
                }
                catch (Exception ex)
                {

                    return ex.Message;
                }
            }
        }

        #endregion


    }

}
