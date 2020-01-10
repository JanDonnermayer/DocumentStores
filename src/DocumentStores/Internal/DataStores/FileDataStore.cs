using System;
using System.Collections.Generic;
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
            this.fileExtension = fileExtension
                ?? throw new ArgumentNullException(nameof(fileExtension));
            this.rootDirectory = rootDirectory
                ?? throw new ArgumentNullException(nameof(rootDirectory));
        }

        private string GetDirectoryPath(DocumentRoute route)
        {
            var relativePath = string.Join(
                Path.DirectorySeparatorChar.ToString(System.Globalization.CultureInfo.InvariantCulture),
                route.Encode().Append("").ToArray()
            );

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

        bool IDataStore.Exists(DocumentAddress address) =>
            File.Exists(GetFilePath(address));

        IEnumerable<DocumentAddress> IDataStore.GetAddresses(
            DocumentRoute route,
            DocumentSearchOptions options)
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
              .Select(k => DocumentAddress.Create(route, DocumentKey.FromString(k)));
        }

        Stream IDataStore.GetReadStream(DocumentAddress address)
        {
            var file = GetFilePath(address);
            if (!File.Exists(file)) throw new DocumentMissingException(address);

            return File.OpenRead(file);
        }

        Stream IDataStore.GetWriteStream(DocumentAddress address)
        {
            var file = GetFilePath(address);
            var directory = new FileInfo(file).Directory;
            if (!directory.Exists) directory.Create();

            return File.OpenWrite(file);
        }

        void IDataStore.Delete(DocumentAddress address)
        {
            var file = GetFilePath(address);
            if (!File.Exists(file)) return;

            File.Delete(file);
        }

        void IDataStore.Clear()
        {
            if (!Directory.Exists(rootDirectory)) return;
            Directory.Delete(rootDirectory, recursive: true);
        }

        #endregion
    }

}
