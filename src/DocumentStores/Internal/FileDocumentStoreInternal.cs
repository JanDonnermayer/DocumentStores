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
        private string GetFileExtension<T>() where T : class =>
            ImmutableInterlocked.GetOrAdd(
                ref FileExtensions,
                typeof(T),
                _ => fileHandling.FileExtension<T>());

        private ImmutableDictionary<Type, string> Subdirectories =
            ImmutableDictionary<Type, string>.Empty;
        private string GetSubDirectory<T>() where T : class =>
            ImmutableInterlocked.GetOrAdd(
                ref Subdirectories,
                typeof(T),
                _ => fileHandling.Subdirectory<T>());

        private async Task SerializeAsync<T>(T data, StreamWriter SW) where T : class =>
            await fileHandling.SerializeAsync<T>(SW, data).ConfigureAwait(false);

        private async Task<T> DeserializeAsync<T>(StreamReader SR) where T : class =>
            await fileHandling.DeserializeAsync<T>(SR).ConfigureAwait(false);


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

        private string GetFile<T>(DocumentKey key) where T : class =>
            Path.Combine(this.RootDirectory, this.GetSubDirectory<T>(), key.GetPath() + GetFileExtension<T>());

        private DocumentKey GetKey<T>(string file) where T : class
        {
            var subs1 = file.Substring(
                startIndex: RootDirectory.Length + @"\\".Length + GetSubDirectory<T>().Length);
            var subpath = subs1.Substring(
                startIndex: 0,
                length: subs1.Length - GetFileExtension<T>().Length);

            return DocumentPathBuilder.GetKey(subpath);
        }


        #endregion


        #region Implementation of IDocumentStoreInternal 

        public Task<IEnumerable<DocumentKey>> GetKeysAsync<T>(DocumentTopicName topicName, CancellationToken ct = default) where T : class =>
                  Task.Run(() =>
                  {
                      try
                      {
                          var directory = Path.Combine(RootDirectory, GetSubDirectory<T>(), topicName.GetPath());
                          if (!Directory.Exists(directory)) return Enumerable.Empty<DocumentKey>();

                          return Directory
                            .EnumerateFiles(
                                path: directory,
                                searchPattern: "*" + GetFileExtension<T>(),
                                searchOption: SearchOption.AllDirectories)
                            .Select(GetKey<T>);
                      }
                      catch (Exception)
                      {
                          return Enumerable.Empty<DocumentKey>();
                      }
                  }, ct);

        public async Task<T> GetDocumentAsync<T>(DocumentKey key) where T : class
        {
            var file = GetFile<T>(key);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

            using FileStream FS = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);

            var data = await DeserializeAsync<T>(SR);
            return data;
        }

        public async Task<Unit> PutDocumentAsync<T>(DocumentKey key, T data) where T : class
        {
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
            DocumentKey key,
            Func<string, Task<T>> addDataAsync,
            Func<string, T, Task<T>> updateDataAsync) where T : class
        {
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
            DocumentKey key,
            Func<string, Task<T>> addDataAsync) where T : class
        {
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

        public async Task<Unit> DeleteDocumentAsync<T>(DocumentKey key) where T : class
        {
            var file = GetFile<T>(key);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {key}");

            File.Delete(file);
            return Unit.Default;
        }

        #endregion
    
    }

}
