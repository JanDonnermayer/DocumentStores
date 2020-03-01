using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Cryptography;

namespace DocumentStores.Internal
{
    internal class AesEncryptionOptions : EncryptionOptions
    {
        public AesEncryptionOptions(IEnumerable<byte>? key = null, IEnumerable<byte>? iV = null)
            : base(
                key: GetValidKey(key ?? GetDefaultKey()),
                iV: GetValidIV(iV ?? GetDefaultIV())
            )
        { }

        private static byte[] GetHash(IEnumerable<byte> source)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(source.ToArray());
        }

        private static IEnumerable<byte> GetDefaultKey() =>
            Enumerable.Range(23, 32).Select(i => (byte)(i)).ToArray();

        private static IEnumerable<byte> GetValidKey(IEnumerable<byte> key) =>
            GetHash(key).Take(32);

        private static IEnumerable<byte> GetDefaultIV() =>
            Enumerable.Range(61, 16).Select(i => (byte)(i)).ToArray();

        private static IEnumerable<byte> GetValidIV(IEnumerable<byte> iV) =>
            GetHash(iV).Take(16);

        public static EncryptionOptions Default =>
            new AesEncryptionOptions();

        public override EncryptionOptions WithKey(IEnumerable<byte> key) =>
            new AesEncryptionOptions(key, this.IV);

        public override EncryptionOptions WithIV(IEnumerable<byte> iv) =>
            new AesEncryptionOptions(this.Key, iv);
    }
}