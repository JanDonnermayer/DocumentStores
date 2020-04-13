using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using static System.String;

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
            this.fileExtension = fileExtension
                ?? throw new ArgumentNullException(nameof(fileExtension));
            this.rootDirectory = rootDirectory
                ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        private string GetDirectoryPath(DocumentRoute route)
        {
            var relativePath = Join(
                Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture),
                route.Encode().Segments.Append(Empty).ToArray()
            );

            return Path.Combine(
                GetRootDirectory(),
                relativePath
            );
        }

        private string GetFilePath(DocumentAddress address)
        {
            var path = Path.Combine(
                GetDirectoryPath(address.Route),
                address.Key.Encode()
            );

            return Path.ChangeExtension(path, fileExtension);
        }

        private static DocumentAddress GetAddress(string path)
        {
            var fileName = Path
                .GetFileName(path);

            var segments = path
                .Split(Path.DirectorySeparatorChar)
                .Except(new[] { Empty, fileName })
                .ToArray();

            var route = DocumentRoute
                .Create(segments)
                .Decode();

            var fileNameTrimmed = Path
                .GetFileNameWithoutExtension(fileName);

            var key = DocumentKey
                .Create(fileNameTrimmed)
                .Decode();

            return DocumentAddress
                .Create(route, key);
        }

        #region  IDocumentStoreInternal

        public bool ContainsAddress(DocumentAddress address) =>
            File.Exists(GetFilePath(address));

        public IEnumerable<DocumentAddress> GetAddresses(
            DocumentRoute route,
            DocumentSearchOption options)
        {
            var directory = GetDirectoryPath(route);
            if (!Directory.Exists(directory)) return Enumerable.Empty<DocumentAddress>();

            SearchOption searchOption = options switch
            {
                DocumentSearchOption.TopLevelOnly => SearchOption.TopDirectoryOnly,
                DocumentSearchOption.AllLevels => SearchOption.AllDirectories,
                _ => throw new ArgumentException($"Invalid {nameof(options)}: {options}")
            };

            return Directory
                .EnumerateFiles(
                    path: directory,
                    searchPattern: "*" + fileExtension,
                    searchOption: searchOption
                )
                .Select( // abs -> rel
                    path => path.Replace(directory, String.Empty)
                )
                .Select(GetAddress)
                .Select( // rel -> abs
                    relAddress => relAddress.MapRoute(
                        relRoute => relRoute.Prepend(route)
                    )
                );
        }

        public Stream GetReadStream(DocumentAddress address)
        {
            var file = GetFilePath(address);
            if (!File.Exists(file)) throw new DocumentMissingException(address);

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

        public void Clear()
        {
            if (!Directory.Exists(rootDirectory)) return;
            Directory.Delete(rootDirectory, recursive: true);
        }

        #endregion
    }
}
