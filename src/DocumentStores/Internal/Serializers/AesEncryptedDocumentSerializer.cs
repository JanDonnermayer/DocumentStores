using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;

namespace DocumentStores.Internal
{
    internal class AesEncryptedDocumentSerializer : IDocumentSerializer
    {
        private readonly IDocumentSerializer serializer;
        private readonly AesEncryptionOptions options;

        public AesEncryptedDocumentSerializer(IDocumentSerializer internalSerializer, AesEncryptionOptions? options = null)
        {
            this.serializer = internalSerializer
                ?? throw new System.ArgumentNullException(nameof(internalSerializer));
            this.options = options ?? (AesEncryptionOptions)AesEncryptionOptions.Default;
        }

        async Task IDocumentSerializer.SerializeAsync<T>(Stream stream, T data) where T : class
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));
            if (data is null)
                throw new System.ArgumentNullException(nameof(data));

            using var rijndael = Rijndael.Create();
            var encryptor = rijndael.CreateEncryptor(options.Key.ToArray(), options.IV.ToArray());
            using var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);

            try
            {
                await serializer.SerializeAsync(cryptoStream, data).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }
            catch (CryptographicException ex)
            {
                throw new SerializationException("Decryption failed. Make sure to use correct key.", ex);
            }
        }

        async Task<T> IDocumentSerializer.DeserializeAsync<T>(Stream stream) where T : class
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            using var rijndael = Rijndael.Create();
            var encryptor = rijndael.CreateDecryptor(options.Key.ToArray(), options.IV.ToArray());
            using var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);

            try
            {
                return await serializer.DeserializeAsync<T>(cryptoStream).ConfigureAwait(false);
            }
            catch (CryptographicException ex)
            {
                throw new SerializationException("Decryption failed. Make sure to use correct key.", ex);
            }
        }
    }
}
