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
        /// <summary>
        /// Creates a new instance of <see cref="JsonFileDocumentStoreOptions"/>,
        /// featuring the specified <paramref name="rootDirectory"/>,
        /// optionally using the specified <paramref name="encryptionOptions"/>
        /// /// </summary>
        public JsonFileDocumentStoreOptions(string rootDirectory, EncryptionOptions? encryptionOptions = null)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
                throw new ArgumentException("Please provide a valid directory!", nameof(rootDirectory));

            this.RootDirectory = rootDirectory;
            this.EncryptionOptions = encryptionOptions ?? EncryptionOptions.None;
        }

        /// <summary>
        /// The root directory.
        /// </summary>
        public string RootDirectory { get; }

        /// <summary>
        /// The encryption options.
        /// </summary>
        /// <value></value>
        public EncryptionOptions EncryptionOptions { get; }

        /// <summary>
        /// Creates a new instance of <see cref="JsonFileDocumentStoreOptions"/>,
        /// featuring default values. 
        /// The default value for <see cref="RootDirectory"/> is $(ApplicationData)/$(AssemblyName).
        /// </summary>
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