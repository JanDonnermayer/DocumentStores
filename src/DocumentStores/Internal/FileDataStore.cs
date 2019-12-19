using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal class FileDataStore : IDataStore
    {
        private string GetRootDirectory() =>
            rootDirectory;

        private readonly string rootDirectory;
        private readonly string fileExtension;

        public FileDataStore(string rootDirectory, string fileExtension)
        {
            this.fileExtension = fileExtension;
            this.rootDirectory = rootDirectory;
        }

        private string GetDirectoryPath(DocumentRoute route)
        {
            var relativePath = string.Join(
                Path.DirectorySeparatorChar.ToString(),
                route.Encode().Append("").ToArray());

            return Path.Combine(
                GetRootDirectory(),
                relativePath);
        }

        private string GetFilePath(DocumentAddress address)
        {
            var path = Path.Combine(
                GetDirectoryPath(address.Route),
                address.Key.Encode());

            return Path.ChangeExtension(path, fileExtension);
        }


        #region  IDocumentStoreInternal

        public bool Exists(DocumentAddress address) =>
            File.Exists(GetFilePath(address));

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync(
            DocumentRoute route,
            DocumentSearchOptions options,
            CancellationToken ct = default) =>
                  Task.Run(() =>
                  {
                      try
                      {
                          var directory = GetDirectoryPath(route);
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
                                searchPattern: "*" + fileExtension,
                                searchOption: searchOption)
                            .Select(Path.GetFileNameWithoutExtension)
                            .Select(k => DocumentAddress.Create(route, DocumentKey.Create(k)));
                      }
                      catch (Exception)
                      {
                          return Enumerable.Empty<DocumentAddress>();
                      }
                  }, ct);


        public Stream GetReadStream(DocumentAddress address)
        {
            var file = GetFilePath(address);
            if (!File.Exists(file))
                throw new DocumentException($"No such document: {address}");

            return File.OpenRead(file);
        }

        public Stream GetWriteStream(DocumentAddress address)
        {
            var file = GetFilePath(address);
            var directory = new FileInfo(file).Directory;
            if (!directory.Exists) directory.Create();

            return File.OpenWrite(file);
        }

        public void Delete(DocumentAddress address)
        {
            var file = GetFilePath(address);
            if (!File.Exists(file)) return;

            File.Delete(file);
        }

        #endregion
    }

}
