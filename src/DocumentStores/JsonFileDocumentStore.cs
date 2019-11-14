using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using Newtonsoft.Json;
using System.Collections.Immutable;
using DocumentStores.Primitives;
using System.Collections.Concurrent;

namespace DocumentStores
{
      public class JsonFileDocumentStore : IDocumentStore
    {

        private ImmutableDictionary<string, SemaphoreSlim> locks =
            ImmutableDictionary<string, SemaphoreSlim>.Empty;

        private string RootDirectory { get; }

        //private JsonSerializerOptions SerializerSettings { get; }


        #region Private Members

        private static bool IsCatchable(Exception ex) =>
            ex is Newtonsoft.Json.JsonException
            || ex is DocumentException
            || ex is IOException;

        // Map invalid filename chars to some weird unicode
        private static readonly ImmutableDictionary<char, char> encodingMap =
            Path
                .GetInvalidFileNameChars()
                .Select((_, i) => new KeyValuePair<char, char>(_, (char)(i + 2000)))
                .ToImmutableDictionary();
        private static readonly IImmutableDictionary<char, char> decodingMap =
            encodingMap
                .Select(kvp => new KeyValuePair<char, char>(kvp.Value, kvp.Key))
                .ToImmutableDictionary();
        private static string EncodeKey(string key) =>
            new string(key.Select(_ => encodingMap.TryGetValue(_, out var v) ? v : _).ToArray());
        private static string DecodeKey(string encodedKey) =>
            new string(encodedKey.Select(_ => decodingMap.TryGetValue(_, out var v) ? v : _).ToArray());

        // Check whether key is null,
        // or contains anything from decoding map which would lead to collisions
        private static void CheckKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (key.Any(decodingMap.Keys.Contains))
                throw new ArgumentException("Key contains invalid chars!", nameof(key));
        }

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
            this.FilePrefix + EncodeKey(key) + FileSuffix<T>();

        private string GetKey<T>(string file)
        {
            var subs1 = file.Substring(this.FilePrefix.Length);
            var subs2 = subs1.Substring(0, subs1.Length - FileSuffix<T>().Length);
            return DecodeKey(subs2);
        }

        private static async Task<T> Deserialize<T>(StreamReader SR) where T : class =>
            JsonConvert.DeserializeObject<T>(await SR.ReadToEndAsync());
        // JsonSerializer.Deserialize<T>(await SR.ReadToEndAsync(), this.SerializerSettings);

        private static async Task Serialize<T>(T data, StreamWriter SW) =>
            await SW.WriteAsync(JsonConvert.SerializeObject(data, Formatting.Indented));
        //await SW.WriteAsync(JsonSerializer.Serialize(data, this.SerializerSettings));

        #endregion


        #region Constructor

        public JsonFileDocumentStore(string directory)
        {
            this.RootDirectory = directory ?? throw new ArgumentNullException(nameof(directory));

            //this.SerializerSettings = new JsonSerializerOptions
            //{
            //    IgnoreNullValues = true,
            //    WriteIndented = true,
            //    PropertyNameCaseInsensitive = true
            //};

            if (!new DirectoryInfo(directory).Exists)
            {
                Directory.CreateDirectory(directory);
            }
        }

        #endregion


        #region Implementation of IDocumentStore

        public Task<IEnumerable<string>> GetKeysAsync<T>(CancellationToken ct = default) =>
            Task.Run(() =>
            {
                try
                {
                    return Directory.EnumerateFiles(
                        this.RootDirectory, "*" + FileSuffix<T>(),
                        SearchOption.TopDirectoryOnly).Select(GetKey<T>);
                }
                catch (Exception _) when (IsCatchable(_))
                {
                    return Enumerable.Empty<string>();
                }
            }, ct);

        public async Task<Result<T>> GetDocumentAsync<T>(string key) where T : class
        {
            CheckKey(key);

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            try
            {
                if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

                using FileStream FS = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                using StreamReader SR = new StreamReader(FS);

                var data = await Deserialize<T>(SR);

                return data;
            }
            catch (Exception _) when (IsCatchable(_))
            {
                return _;
            }

        }


        public async Task<Result> PutDocumentAsync<T>(string key, T data) where T : class
        {
            CheckKey(key);
            if (data == null) throw new ArgumentNullException(nameof(data));

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            try
            {
                using FileStream FS = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
                using StreamWriter SW = new StreamWriter(FS);

                await Serialize(data, SW);
                SW.Flush();
                FS.SetLength(FS.Position);

                return Result.Ok();
            }
            catch (Exception _) when (IsCatchable(_))
            {
                return _;
            }

        }

        public async Task<Result<T>> AddOrUpdateDocumentAsync<T>(
            string key,
            Func<string, Task<T>> addDataAsync,
            Func<string, T, Task<T>> updateDataAsync) where T : class
        {
            CheckKey(key);
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));
            if (updateDataAsync is null) throw new ArgumentNullException(nameof(updateDataAsync));

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            try
            {
                using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                using StreamReader SR = new StreamReader(FS);
                using StreamWriter SW = new StreamWriter(FS);

                async Task<T> getDataAsync() => await Deserialize<T>(SR) switch
                {
                    null => await addDataAsync(key)
                        ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!"),
                    T data => await updateDataAsync(key, data)
                       ?? throw new DocumentException($"{nameof(updateDataAsync)} returned null!"),
                };

                var data = await getDataAsync();

                FS.Position = 0;
                await Serialize(data, SW);
                SW.Flush();
                FS.SetLength(FS.Position);

                return data;
            }
            catch (Exception _) when (IsCatchable(_))
            {
                return _;
            }
        }

        public async Task<Result<T>> GetOrAddDocumentAsync<T>(
            string key,
            Func<string, Task<T>> addDataAsync) where T : class
        {
            CheckKey(key);
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            try
            {
                using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                using StreamReader SR = new StreamReader(FS);
                using StreamWriter SW = new StreamWriter(FS);

                var data = await Deserialize<T>(SR)
                    ?? await addDataAsync(key)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

                FS.Position = 0;
                await Serialize(data, SW);
                SW.Flush();
                FS.SetLength(FS.Position);

                return data;
            }
            catch (Exception _) when (IsCatchable(_))
            {
                return _;
            }
        }

        public async Task<Result> DeleteDocumentAsync<T>(string key) where T : class
        {
            CheckKey(key);

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            try
            {
                if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

                File.Delete(file);

                return Result.Ok();
            }
            catch (Exception _) when (IsCatchable(_))
            {
                return _;
            }
        }

        #endregion


    }

}
