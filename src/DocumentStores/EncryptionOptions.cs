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
            Key = ImmutableArray.CreateRange(key ?? throw new ArgumentNullException(nameof(key)));
            IV = ImmutableArray.CreateRange(iV ?? throw new ArgumentNullException(nameof(iV)));
        }

        internal ImmutableArray<byte> Key { get; }

        internal ImmutableArray<byte> IV { get; }

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
