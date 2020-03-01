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
        private static string GetDefaultRootDirectory() =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name,
                typeof(JsonFileDocumentStore).Name
            );

        private static EncryptionOptions GetDefaultEncyrptionOptions() =>
            EncryptionOptions.None;

        /// <summary>
        /// Creates a new instance of <see cref="JsonFileDocumentStoreOptions"/>,
        /// featuring the specified <paramref name="rootDirectory"/>,
        /// using the specified <paramref name="encryptionOptions"/>
        /// </summary>
        public JsonFileDocumentStoreOptions(string? rootDirectory = null, EncryptionOptions? encryptionOptions = null)
        {
            this.RootDirectory = rootDirectory ?? GetDefaultRootDirectory();
            this.EncryptionOptions = encryptionOptions ?? GetDefaultEncyrptionOptions();
        }

        /// <summary>
        /// The root directory.
        /// </summary>
        public string RootDirectory { get; }

        /// <summary>
        /// The encryption options.
        /// </summary>
        public EncryptionOptions EncryptionOptions { get; }

        /// <summary>
        /// Creates a new instance of <see cref="JsonFileDocumentStoreOptions"/>,
        /// using the specified <paramref name="encryptionOptions"/>
        /// </summary>
        public JsonFileDocumentStoreOptions WithEncryptionOptions(EncryptionOptions encryptionOptions) =>
            new JsonFileDocumentStoreOptions(this.RootDirectory, encryptionOptions);

        /// <summary>
        /// Creates a new instance of <see cref="JsonFileDocumentStoreOptions"/>,
        /// using the specified <paramref name="rootDirectory"/>
        /// </summary>
        public JsonFileDocumentStoreOptions WithRootDirectory(string rootDirectory) =>
            new JsonFileDocumentStoreOptions(rootDirectory, this.EncryptionOptions);

        /// <summary>
        /// Creates a new instance of <see cref="JsonFileDocumentStoreOptions"/>,
        /// featuring default values. 
        /// The default for <see cref="RootDirectory"/> is $(ApplicationData)/$(AssemblyName).
        /// The default for <see cref="EncryptionOptions"/> is no encryption.
        /// </summary>
        public static JsonFileDocumentStoreOptions Default =>
            new JsonFileDocumentStoreOptions(
                rootDirectory: GetDefaultRootDirectory(),
                encryptionOptions: GetDefaultEncyrptionOptions()
            );
    }
}