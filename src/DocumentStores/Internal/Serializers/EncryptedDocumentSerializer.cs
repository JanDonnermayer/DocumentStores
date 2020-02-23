using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DocumentStores.Internal
{
    internal class EncryptedDocumentSerializer : IDocumentSerializer
    {
        private readonly IDocumentSerializer serializer;
        private readonly byte[] key;
        private readonly byte[] IV;

        public EncryptedDocumentSerializer(IDocumentSerializer serializer, byte[] key, byte[] IV)
        {
            this.serializer = serializer
                ?? throw new System.ArgumentNullException(nameof(serializer));

            this.key = key;
            this.IV = IV;
        }

        public async Task<T> DeserializeAsync<T>(Stream stream) where T : class
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            using var rijndael = Rijndael.Create();
            var encryptor = rijndael.CreateDecryptor(key, IV);
            using var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Read);
            return await serializer.DeserializeAsync<T>(cryptoStream).ConfigureAwait(false);
        }

        public async Task SerializeAsync<T>(Stream stream, T data) where T : class
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            if (data is null)
                throw new System.ArgumentNullException(nameof(data));

            using var rijndael = Rijndael.Create();
            var encryptor = rijndael.CreateEncryptor(key, IV);
            using var cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
            await serializer.SerializeAsync(cryptoStream, data).ConfigureAwait(false);
        }
    }
}
