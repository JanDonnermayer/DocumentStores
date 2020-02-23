using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using System;

namespace DocumentStores.Internal
{
    internal class RijndaelEncryptedDocumentSerializer : IDocumentSerializer
    {
        private readonly IDocumentSerializer serializer;
        private readonly byte[] key;
        private readonly byte[] iV;

        public RijndaelEncryptedDocumentSerializer(IDocumentSerializer internalSerializer, byte[]? key = null, byte[]? iV = null)
        {
            this.serializer = internalSerializer
                ?? throw new System.ArgumentNullException(nameof(internalSerializer));

            var _key = key ?? Enumerable.Range(0, 32).Select(i => (byte)(i)).ToArray();
            var _iV = iV ?? Enumerable.Range(0, 16).Select(i => (byte)(i)).ToArray();

            var acceptedKeySizes = new int[] { 16, 24, 32 };
            if (!acceptedKeySizes.Contains(_key.Length))
                throw new ArgumentException($"Key-size must be in: [{String.Join(", ", acceptedKeySizes)}] bytes.");

            const int acceptedIVSize = 16;
            if (_iV.Length != acceptedIVSize)
                throw new ArgumentException($"IV-size must be exactly {acceptedIVSize} bytes.");

            this.key = _key;
            this.iV = _iV;
        }

        async Task IDocumentSerializer.SerializeAsync<T>(Stream stream, T data) where T : class
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            if (data is null)
                throw new System.ArgumentNullException(nameof(data));

            using var rijndael = Rijndael.Create();
            var encryptor = rijndael.CreateEncryptor(key, iV);
            using var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
            await serializer.SerializeAsync(cryptoStream, data).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        async Task<T> IDocumentSerializer.DeserializeAsync<T>(Stream stream) where T : class
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            using var rijndael = Rijndael.Create();
            var encryptor = rijndael.CreateDecryptor(key, iV);
            using var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
            return await serializer.DeserializeAsync<T>(cryptoStream).ConfigureAwait(false);
        }
    }
}
