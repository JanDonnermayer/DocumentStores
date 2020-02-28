using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentStores.Internal;
using Moq;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class DocumentSerializerIntegrationTest
    {
        private IDocumentSerializer GetSerializer(IEnumerable<byte> key, IEnumerable<byte> iv) =>
            new AesEncryptedDocumentSerializer(
                new JsonDocumentSerializer(),
                new AesEncryptionOptions(key, iv)
            );

        [Test]
        public async Task Test_Roundtrip_Equals()
        {
            // Arrange
            var serializer = GetSerializer(
                key: new byte[] { 1, 2, 4 },
                iv: new byte[] { 1, 4, 5 }
            );

            // Arrange
            var testData = "ABC";
            var stream = new ObservableMemoryStream();

            var resultBuffer = Array.Empty<byte>();
            stream.OnDispose().Subscribe(_ => resultBuffer = _);
            

            // Act
            await serializer.SerializeAsync(stream, testData);
            var resultStream = new MemoryStream(resultBuffer);
            var roundtrippedData = await serializer.DeserializeAsync<string>(resultStream);

            // Assert
            Assert.AreEqual(testData, roundtrippedData);
        }

        [Test]
        public void Test_InvalidEncryptedData_ThrowsSerializationException()
        {
            // Arrange
            var serializer = GetSerializer(
                key: new byte[] { 1, 2, 4 },
                iv: new byte[] { 1, 4, 5 }
            );

            var buffer = new byte[] { 1, 2, 4 };
            using var stream = new MemoryStream(buffer);

            // Act & Assert
            Assert.ThrowsAsync<SerializationException>(
                () => serializer.DeserializeAsync<string>(stream)
            );
        }
    }
}