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
    internal class FileDocumentRouter : IDocumentRouter
    {
        private string GetRootDirectory<TDocument>() =>
            Path.Combine(
                rootDirectory,
                typeof(TDocument).ShortName(true).Replace(">", "}").Replace("<", "{"));


        private readonly string rootDirectory;
        private readonly string fileExtension;

        public FileDocumentRouter(string rootDirectory, string fileExtension)
        {
            this.fileExtension = fileExtension;
            this.rootDirectory = rootDirectory;
        }

        private string GetDirectoryPath<TDocument>(DocumentRoute route)
        {
            var relativePath = string.Join(
                Path.DirectorySeparatorChar.ToString(),
                route.Encode().Append("").ToArray());

            return Path.Combine(
                GetRootDirectory<TDocument>(),
                relativePath);
        }

        private string GetFilePath<TDocument>(DocumentAddress address)
        {
            var path = Path.Combine(
                GetDirectoryPath<TDocument>(address.Route),
                address.Key.Encode());

            return Path.ChangeExtension(path, fileExtension);
        }


        #region  IDocumentRouter

        public Task<IEnumerable<DocumentAddress>> GetAddressesAsync<TDocument>(
            DocumentRoute route,
            DocumentSearchOptions options,
            CancellationToken ct = default) =>
                  Task.Run(() =>
                  {
                      try
                      {
                          var directory = GetDirectoryPath<TDocument>(route);
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


        public Stream GetReadStream<TDocument>(DocumentAddress address)
        {
            var file = GetFilePath<TDocument>(address);
            if (!File.Exists(file)) return Stream.Null;

            return File.OpenRead(file);
        }

        public Stream GetWriteStream<TDocument>(DocumentAddress address)
        {
            var file = GetFilePath<TDocument>(address);
            var directory = new FileInfo(file).Directory;
            if (!directory.Exists) directory.Create();

            return File.OpenWrite(file);
        }

        public bool Exists<TDocument>(DocumentAddress address) =>
            File.Exists(GetFilePath<TDocument>(address));

        #endregion
    }

}
