using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using DocumentStores.Internal;

namespace DocumentStores
{
    /// <summary>
    /// Options for encryption and decryption.
    /// </summary>
    public abstract class EncryptionOptions
    {
        internal EncryptionOptions(IEnumerable<byte> key, IEnumerable<byte> iV)
        {
            Key = ImmutableList.CreateRange(key ?? throw new ArgumentNullException(nameof(key)));
            IV = ImmutableList.CreateRange(iV ?? throw new ArgumentNullException(nameof(iV)));
        }

        internal IEnumerable<byte> Key { get; }

        internal IEnumerable<byte> IV { get; }

        /// <summary>
        /// Applies the specified <paramref name="key"/>.
        /// </summary>
        public abstract EncryptionOptions WithKey(IEnumerable<byte> key);

        /// <summary>
        /// Applies the specified <paramref name="key"/>.
        /// </summary>
        public EncryptionOptions WithKey(string key) =>
            this.WithKey(System.Text.Encoding.UTF8.GetBytes(key));

        /// <summary>
        /// Applies the specified <paramref name="iv"/>.
        /// </summary>
        public abstract EncryptionOptions WithIV(IEnumerable<byte> iv);

        /// <summary>
        /// Creates default options for AES-encryption,
        /// </summary>
        public static EncryptionOptions Aes =>
            AesEncryptionOptions.Default;

        /// <summary>
        /// Creates options for no encryption.
        /// </summary>
        public static EncryptionOptions None =>
            new NoEncryptionOptions();
    }
}
