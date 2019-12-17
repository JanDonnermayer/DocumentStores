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

        private string GetDirectory<T>(DocumentRoute route) where T : class =>
            Path.Combine(
                this.RootDirectory,
                this.GetSubDirectory<T>(),
                route.ToPath());

        private string GetFile<T>(DocumentAddress address) where T : class =>
            Path.Combine(
                this.RootDirectory,
                this.GetSubDirectory<T>(),
                address.ToPath() + GetFileExtension<T>());


        #endregion


        #region Implementation of IDocumentStoreInternal 

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<T>(
            DocumentRoute route, DocumentSearchOptions options, CancellationToken ct = default) where T : class =>
                  Task.Run(() =>
                  {
                      try
                      {
                          var directory = GetDirectory<T>(route);
                          if (!Directory.Exists(directory)) return Enumerable.Empty<DocumentAddress>();

                          SearchOption searchOption = options switch
                          {
                              DocumentSearchOptions.TopLevelOnly => SearchOption.TopDirectoryOnly,
                              DocumentSearchOptions.AllLevels => SearchOption.AllDirectories,
                              _ => throw new ArgumentException($"Invalid {nameof(options)}: {options}")
                          };

                          return Directory
                            .EnumerateFiles(
                                path: directory,
                                searchPattern: "*" + GetFileExtension<T>(),
                                searchOption: searchOption)      
                            .Select(Path.GetFileNameWithoutExtension)
                            .Select(FileDocumentRouter.GetKey)
                            .Select(k => route.ToAddress(k));
                      }
                      catch (Exception)
                      {
                          return Enumerable.Empty<DocumentAddress>();
                      }
                  }, ct);

        public async Task<T> GetDocumentAsync<T>(DocumentAddress address) where T : class
        {
            var file = GetFile<T>(address);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {address}");

            using FileStream FS = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);

            var data = await DeserializeAsync<T>(SR);
            return data;
        }

        public async Task<Unit> PutDocumentAsync<T>(DocumentAddress address, T data) where T : class
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var file = GetFile<T>(address);
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
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync,
            Func<DocumentAddress, T, Task<T>> updateDataAsync) where T : class
        {
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));
            if (updateDataAsync is null) throw new ArgumentNullException(nameof(updateDataAsync));

            var file = GetFile<T>(address);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

            using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);
            using StreamWriter SW = new StreamWriter(FS);

            async Task<T> getDataAsync() => await DeserializeAsync<T>(SR) switch
            {
                null => await addDataAsync(address)
                    ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!"),
                T data => await updateDataAsync(address, data)
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
            DocumentAddress address,
            Func<DocumentAddress, Task<T>> addDataAsync) where T : class
        {
            if (addDataAsync is null) throw new ArgumentNullException(nameof(addDataAsync));

            var file = GetFile<T>(address);
            using var @lock = await GetLockAsync(file);

            await CreateDirectoryIfMissingAsync(file);

            using FileStream FS = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            using StreamReader SR = new StreamReader(FS);
            using StreamWriter SW = new StreamWriter(FS);

            var data = await DeserializeAsync<T>(SR)
                ?? await addDataAsync(address)
                ?? throw new DocumentException($"{nameof(addDataAsync)} returned null!");

            FS.Position = 0;
            await SerializeAsync(data, SW);
            SW.Flush();
            FS.SetLength(FS.Position);

            return data;
        }

        public async Task<Unit> DeleteDocumentAsync<T>(DocumentAddress address) where T : class
        {
            var file = GetFile<T>(address);
            using var @lock = await GetLockAsync(file);

            if (!File.Exists(file)) throw new DocumentException($"No such document: {address}");

            File.Delete(file);
            return Unit.Default;
        }

        #endregion

    }

}
