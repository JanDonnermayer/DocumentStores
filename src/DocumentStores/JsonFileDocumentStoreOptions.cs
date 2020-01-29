using System;
using System.IO;
using System.Reflection;

namespace DocumentStores
{
    /// <summary>
    /// Options for <see cref="JsonFileDocumentStore"/>.
    /// </summary>
    public sealed class JsonFileDocumentStoreOptions
    {
        private JsonFileDocumentStoreOptions(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
                throw new ArgumentException("Please provide a valid directory!", nameof(rootDirectory));

            this.RootDirectory = rootDirectory;
        }

        /// <summary>
        /// The root directory.
        /// </summary>
        public string RootDirectory { get; }

        /// <summary>
        /// Creates a new instance of <see cref="JsonFileDocumentStoreOptions"/>,
        /// featuring the specified <paramref name="rootDirectory"/>.
        /// </summary>
        /// <param name="rootDirectory">The root directory for the stored documents.</param>
        public static JsonFileDocumentStoreOptions ForRootDirectory(string rootDirectory) =>
            new JsonFileDocumentStoreOptions(rootDirectory);

        /// <summary>
        /// Creates the default options, with RootDirectory pointing to AppData.
        /// </summary>
        /// <returns></returns>
        public static JsonFileDocumentStoreOptions Default =>
            new JsonFileDocumentStoreOptions(
                rootDirectory: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    Assembly.GetEntryAssembly().GetName().Name,
                    typeof(JsonFileDocumentStore).Name
                )
            );
    }
}