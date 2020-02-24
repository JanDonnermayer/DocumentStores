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
        /// Creates options for AES-encryption,
        /// optionally using the specified key and initialization-vector.
        /// </summary>
        public static EncryptionOptions Aes(byte[]? key = null, byte[]? iV = null) =>
            new AesEncryptionOptions(key, iV);

        /// <summary>
        /// Creates options for AES-encryption,
        /// using the specified key and optionally the specified initialization-vector.
        /// </summary>
        public static EncryptionOptions Aes(string key, byte[]? iV = null) =>
            new AesEncryptionOptions(key, iV);

        /// <summary>
        /// Creates options for no encryption.
        /// </summary>
        public static EncryptionOptions None =>
            new NoEncryptionOptions();
    }
}
