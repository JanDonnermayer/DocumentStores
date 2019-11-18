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
using DocumentStores.Internal;
using System.Diagnostics;

namespace DocumentStores
{
    /// <summary>
    /// An <see cref="IDocumentStore"/>-Implementation,
    /// that uses json-files, to store serializable data.
    /// </summary>
    public class JsonFileDocumentStore : IDocumentStore
    {
        #region Private 

        private ImmutableDictionary<string, SemaphoreSlim> locks =
            ImmutableDictionary<string, SemaphoreSlim>.Empty;

        private string RootDirectory { get; }

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

        private static readonly string FileExtension = ".json";

        //Subdirectory name is typename
        private ImmutableDictionary<Type, string> Subdirectories =
            ImmutableDictionary<Type, string>.Empty;

        private string SubDirectory<T>() =>
            ImmutableInterlocked.GetOrAdd(
                ref Subdirectories,
                typeof(T),
                typeof(T).ShortName(true).Replace(">", "}").Replace("<", "{"));

        private string GetFileName<T>(string key) =>
           Path.Combine(this.RootDirectory, this.SubDirectory<T>(), EncodeKey(key) + FileExtension);

        private async Task CreateDirectoryIfMissingAsync(string file)
        {
            var directory = new FileInfo(file).Directory;
            using var @lock = await GetLockAsync(directory.FullName);
            if (!directory.Exists) directory.Create();
        }

        private string GetKey<T>(string file)
        {
            var subs1 = file.Substring(
                startIndex: RootDirectory.Length + @"\\".Length + SubDirectory<T>().Length);
            var name = subs1.Substring(
                startIndex: 0,
                length: subs1.Length - FileExtension.Length);

            return DecodeKey(name);
        }

        private static async Task<T> Deserialize<T>(StreamReader SR) where T : class =>
            JsonConvert.DeserializeObject<T>(await SR.ReadToEndAsync());

        private static async Task Serialize<T>(T data, StreamWriter SW) =>
            await SW.WriteAsync(JsonConvert.SerializeObject(data, Formatting.Indented));

        #endregion


        #region Constructor

        public JsonFileDocumentStore(string directory) =>
            this.RootDirectory = directory ?? throw new ArgumentNullException(nameof(directory));

        #endregion


        #region Internal 

        internal async Task<T> GetDocumentInternalAsync<T>(string key) where T : class
        {
            CheckKey(key);

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

            using FileStream FS = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);

            var data = await Deserialize<T>(SR);
            return data;
        }

        internal async Task<Unit> PutDocumentInternalAsync<T>(string key, T data) where T : class
        {
            CheckKey(key);
            if (data == null) throw new ArgumentNullException(nameof(data));

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

            using FileStream FS = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter SW = new StreamWriter(FS);

            await Serialize(data, SW);
            SW.Flush();
            FS.SetLength(FS.Position);

            return Unit.Default;
        }

        internal async Task<T> AddOrUpdateDocumentInternalAsync<T>(
            string key,
            Func<string, Task<T>> addDataAsync,
            Func<string, T, Task<T>> updateDataAsync) where T : class
        {
            CheckKey(key);
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));
            if (updateDataAsync is null) throw new ArgumentNullException(nameof(updateDataAsync));

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

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

        internal async Task<T> GetOrAddDocumentInternalAsync<T>(
            string key,
            Func<string, Task<T>> addDataAsync) where T : class
        {
            CheckKey(key);
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

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

        internal async Task<Unit> DeleteDocumentInternalAsync<T>(string key) where T : class
        {
            CheckKey(key);

            var file = GetFileName<T>(key);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

            File.Delete(file);
            return Unit.Default;

        }

        #endregion


        #region Implementation of IDocumentStore

        public Task<IEnumerable<string>> GetKeysAsync<T>(CancellationToken ct = default) =>
                  Task.Run(() =>
                  {
                      try
                      {
                          var directory = Path.Combine(RootDirectory, SubDirectory<T>());
                          if (!Directory.Exists(directory)) return Enumerable.Empty<string>();

                          return Directory.EnumerateFiles(
                              directory,
                              "*" + FileExtension,
                              SearchOption.TopDirectoryOnly).Select(GetKey<T>);
                      }
                      catch (Exception _) when (IsCatchable(_))
                      {
                          return Enumerable.Empty<string>();
                      }
                  }, ct);

        public Task<Result<T>> AddOrUpdateDocumentAsync<T>(string key,
            Func<string, Task<T>> addDataAsync, Func<string, T, Task<T>> updateDataAsync) where T : class =>
                Function.ApplyArgs(AddOrUpdateDocumentInternalAsync, key, addDataAsync, updateDataAsync)
                        .WithTryCatch(IsCatchable)
                        .WithIncrementalRetryBehaviour(TimeSpan.FromMilliseconds(50), 5)
                        .Invoke();

        public Task<Result<T>> GetOrAddDocumentAsync<T>(string key,
            Func<string, Task<T>> addDataAsync) where T : class =>
                Function.ApplyArgs(GetOrAddDocumentInternalAsync, key, addDataAsync)
                        .WithTryCatch(IsCatchable)
                        .WithIncrementalRetryBehaviour(TimeSpan.FromMilliseconds(50), 5)
                        .Invoke();

        public Task<Result<T>> GetDocumentAsync<T>(string key) where T : class =>
            Function.ApplyArgs(GetDocumentInternalAsync<T>, key)
                    .WithTryCatch(IsCatchable)
                    .WithIncrementalRetryBehaviour(TimeSpan.FromMilliseconds(50), 5)
                    .Invoke();

        public Task<Result<Unit>> DeleteDocumentAsync<T>(string key) where T : class =>
            Function.ApplyArgs(DeleteDocumentInternalAsync<T>, key)
                    .WithTryCatch(IsCatchable)            
                    .WithIncrementalRetryBehaviour(TimeSpan.FromMilliseconds(50), 5)
                    .Invoke();

        public Task<Result<Unit>> PutDocumentAsync<T>(string key, T data) where T : class =>
            Function.ApplyArgs(PutDocumentInternalAsync<T>, key, data)
                    .WithTryCatch(IsCatchable)
                    .WithIncrementalRetryBehaviour(TimeSpan.FromMilliseconds(50), 5)
                    .Invoke();

        #endregion
    }

}
