using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Cryptography;

namespace DocumentStores.Internal
{
    internal sealed class AesEncryptionOptions : EncryptionOptions
    {
        private readonly static IEnumerable<int> acceptedIVLengths =
            ImmutableList.Create(16);

        private readonly static IEnumerable<int> acceptedKeyLengths =
            ImmutableList.Create(16, 24, 32);

        public AesEncryptionOptions(IEnumerable<byte>? key = null, IEnumerable<byte>? iV = null)
            : base(
                key: GetValidKey(key ?? GetDefaultKey()),
                iV: GetValidIV(iV ?? GetDefaultIV())
            )
        { }

        public AesEncryptionOptions(string key, IEnumerable<byte>? iV = null)
            : base(
                key: GetValidKey(GetBytes(key ?? throw new ArgumentNullException(nameof(key)))),
                iV: GetValidIV(iV ?? GetDefaultIV())
            )
        { }

        private static byte[] GetBytes(string value) =>
            System.Text.Encoding.UTF8.GetBytes(value);

        private static byte[] GetSHA512Hash(byte[] source)
        {
            using var sha = SHA512.Create();
            return sha.ComputeHash(source);
        }

        private static IEnumerable<byte> GetDefaultKey() =>
            Enumerable.Range(23, 32).Select(i => (byte)(i)).ToArray();

        private static IEnumerable<byte> GetValidKey(IEnumerable<byte> key) =>
            SequenceExtender.TryExtendToNextLargerSequence(
                key, acceptedKeyLengths, (byte)0, out var extended
            ) switch
            {
                true => extended, 
                false => GetSHA512Hash(key.ToArray())
            };

        private static IEnumerable<byte> GetDefaultIV() =>
            Enumerable.Range(61, 16).Select(i => (byte)(i)).ToArray();

        private static IEnumerable<byte> GetValidIV(IEnumerable<byte> iV) =>
            SequenceExtender.TryExtendToNextLargerSequence(
                iV, acceptedIVLengths, (byte)0, out var extended
            ) switch
            {
                true => extended, 
                false => GetSHA512Hash(iV.ToArray())
            };

        public static AesEncryptionOptions Default =>
            new AesEncryptionOptions();
    }
}