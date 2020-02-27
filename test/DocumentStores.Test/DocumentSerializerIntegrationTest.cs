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
        public async Task Test_Overwrite_Roundtrip_DataEquals()
        {
            // Arrange
            var serializer = GetSerializer(
                key: new byte[] { 1, 2, 4 },
                iv: new byte[] { 1, 4, 5 }
            );

            var buffer = new byte[1000];

            using var stream1 = new MemoryStream(buffer);
            var data1 = "testData";

            // Act
            await serializer.SerializeAsync(
                stream: stream1,
                data: data1
            ).ConfigureAwait(false);

            // Arrange
            using var stream2 = new MemoryStream(buffer);
            var data2 = "test";

            // Act
            await serializer.SerializeAsync(
                stream: stream2,
                data: data2
            ).ConfigureAwait(false);

            // Arrange
            using var stream3 = new MemoryStream(buffer.TakeWhile(b => b != (byte)0).ToArray());

            // Act
            var result = await serializer.DeserializeAsync<string>(
                stream: stream3
            ).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(data2, result);
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