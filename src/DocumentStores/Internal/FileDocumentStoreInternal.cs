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
    internal class FileDocumentStoreInternal : IDocumentStoreInternal
    {
        private string GetRootDirectory<TData>() =>
            Path.Combine(
                rootDirectory,
                typeof(TData)
                    .ShortName(true)
                    .Replace(">", "}")
                    .Replace("<", "{")
            );


        private readonly string rootDirectory;
        private readonly string fileExtension;

        public FileDocumentStoreInternal(string rootDirectory, string fileExtension)
        {
            this.fileExtension = fileExtension;
            this.rootDirectory = rootDirectory;
        }

        private string GetDirectoryPath<TData>(DocumentRoute route)
        {
            var relativePath = string.Join(
                Path.DirectorySeparatorChar.ToString(),
                route.Encode().Append("").ToArray());

            return Path.Combine(
                GetRootDirectory<TData>(),
                relativePath);
        }

        private string GetFilePath<TData>(DocumentAddress address)
        {
            var path = Path.Combine(
                GetDirectoryPath<TData>(address.Route),
                address.Key.Encode());

            return Path.ChangeExtension(path, fileExtension);
        }


        #region  IDocumentStoreInternal

        public bool Exists<TData>(DocumentAddress address) =>
            File.Exists(GetFilePath<TData>(address));

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TData>(
            DocumentRoute route,
            DocumentSearchOptions options,
            CancellationToken ct = default) =>
                  Task.Run(() =>
                  {
                      try
                      {
                          var directory = GetDirectoryPath<TData>(route);
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


        public Stream GetReadStream<TData>(DocumentAddress address)
        {
            var file = GetFilePath<TData>(address);
            if (!File.Exists(file))
                throw new DocumentException($"No such document: {address}");

            return File.OpenRead(file);
        }

        public Stream GetWriteStream<TData>(DocumentAddress address)
        {
            var file = GetFilePath<TData>(address);
            var directory = new FileInfo(file).Directory;
            if (!directory.Exists) directory.Create();

            return File.OpenWrite(file);
        }

        public void Delete<TData>(DocumentAddress address)
        {
            var file = GetFilePath<TData>(address);
            if (!File.Exists(file)) return;

            File.Delete(file);
        }


        #endregion
    }

}
