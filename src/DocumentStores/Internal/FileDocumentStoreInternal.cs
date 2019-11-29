using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Immutable;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    
    /// <summary>
    /// An <see cref="IDocumentStoreInternal"/>-Implementation,
    /// that uses files, to store serializable data.
    /// </summary>
    /// <remarks>
    /// Each document is stored as a separate file with the following path:
    /// "[RootDirectory]/[DataType]/[key].[extension]".
    /// Keys are cleaned in order to be valid file-names.
    /// </remarks>
    internal class FileDocumentStoreInternal : IDocumentStoreInternal
    {

        #region Constructor

        public FileDocumentStoreInternal(string directory, IFileHandling fileHandling)
        {
            this.RootDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
            this.fileHandling = fileHandling ?? throw new ArgumentNullException(nameof(fileHandling));
        }

        #endregion


        #region Private 

        private readonly IFileHandling fileHandling;

        private string RootDirectory { get; }

        private ImmutableDictionary<Type, string> FileExtensions =
            ImmutableDictionary<Type, string>.Empty;
        private string FileExtension<T>() where T : class =>
            ImmutableInterlocked.GetOrAdd(
                ref FileExtensions,
                typeof(T),
                _ => fileHandling.FileExtension<T>());

        private ImmutableDictionary<Type, string> Subdirectories =
            ImmutableDictionary<Type, string>.Empty;
        private string SubDirectory<T>() where T : class =>
            ImmutableInterlocked.GetOrAdd(
                ref Subdirectories,
                typeof(T),
                _ => fileHandling.Subdirectory<T>());

        private async Task SerializeAsync<T>(T data, StreamWriter SW) where T : class =>
            await fileHandling.SerializeAsync<T>(SW, data).ConfigureAwait(false);

        private async Task<T> DeserializeAsync<T>(StreamReader SR) where T : class =>
            await fileHandling.DeserializeAsync<T>(SR).ConfigureAwait(false);

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

        private ImmutableDictionary<string, SemaphoreSlim> locks =
            ImmutableDictionary<string, SemaphoreSlim>.Empty;

        private async Task<IDisposable> GetLockAsync(string key)
        {
            var sem = ImmutableInterlocked.GetOrAdd(ref locks, key, s => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();
            return new Disposable(() => sem.Release());
        }

        private async Task CreateDirectoryIfMissingAsync(string file)
        {
            var directory = new FileInfo(file).Directory;
            using var @lock = await GetLockAsync(directory.FullName);
            if (!directory.Exists) directory.Create();
        }

        private string GetFile<T>(string key) where T : class =>
           Path.Combine(this.RootDirectory, this.SubDirectory<T>(), EncodeKey(key) + FileExtension<T>());

        private string GetKey<T>(string file) where T : class
        {
            var subs1 = file.Substring(
                startIndex: RootDirectory.Length + @"\\".Length + SubDirectory<T>().Length);
            var name = subs1.Substring(
                startIndex: 0,
                length: subs1.Length - FileExtension<T>().Length);

            return DecodeKey(name);
        }

        #endregion


        #region Implementation of IDocumentStoreInternal 

        public Task<IEnumerable<string>> GetKeysAsync<T>(CancellationToken ct = default) where T : class =>
                  Task.Run(() =>
                  {
                      try
                      {
                          var directory = Path.Combine(RootDirectory, SubDirectory<T>());
                          if (!Directory.Exists(directory)) return Enumerable.Empty<string>();

                          return Directory.EnumerateFiles(
                              directory,
                              "*" + FileExtension<T>(),
                              SearchOption.TopDirectoryOnly).Select(GetKey<T>);
                      }
                      catch (Exception)
                      {
                          return Enumerable.Empty<string>();
                      }
                  }, ct);

        public async Task<T> GetDocumentAsync<T>(string key) where T : class
        {
            CheckKey(key);

            var file = GetFile<T>(key);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

            using FileStream FS = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);

            var data = await DeserializeAsync<T>(SR);
            return data;
        }

        public async Task<Unit> PutDocumentAsync<T>(string key, T data) where T : class
        {
            CheckKey(key);
            if (data == null) throw new ArgumentNullException(nameof(data));

            var file = GetFile<T>(key);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

            using FileStream FS = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter SW = new StreamWriter(FS);

            await SerializeAsync<T>(data, SW);

            SW.Flush();
            FS.SetLength(FS.Position);

            return Unit.Default;
        }

        public async Task<T> AddOrUpdateDocumentAsync<T>(
            string key,
            Func<string, Task<T>> addDataAsync,
            Func<string, T, Task<T>> updateDataAsync) where T : class
        {
            CheckKey(key);
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));
            if (updateDataAsync is null) throw new ArgumentNullException(nameof(updateDataAsync));

            var file = GetFile<T>(key);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

            using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);
            using StreamWriter SW = new StreamWriter(FS);

            async Task<T> getDataAsync() => await DeserializeAsync<T>(SR) switch
            {
                null => await addDataAsync(key)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!"),
                T data => await updateDataAsync(key, data)
                    ?? throw new DocumentException($"{nameof(updateDataAsync)} returned null!"),
            };

            var data = await getDataAsync();

            FS.Position = 0;
            await SerializeAsync(data, SW);
            SW.Flush();
            FS.SetLength(FS.Position);

            return data;
        }

        public async Task<T> GetOrAddDocumentAsync<T>(
            string key,
            Func<string, Task<T>> addDataAsync) where T : class
        {
            CheckKey(key);
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));

            var file = GetFile<T>(key);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

            using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);
            using StreamWriter SW = new StreamWriter(FS);

            var data = await DeserializeAsync<T>(SR)
                ?? await addDataAsync(key)
                ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

            FS.Position = 0;
            await SerializeAsync(data, SW);
            SW.Flush();
            FS.SetLength(FS.Position);

            return data;
        }

        public async Task<Unit> DeleteDocumentAsync<T>(string key) where T : class
        {
            CheckKey(key);

            var file = GetFile<T>(key);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

            File.Delete(file);
            return Unit.Default;
        }

        #endregion
    
    }

}
