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
    class EncryptedDocumentSerializerTest
    {
        private IDocumentSerializer internalSerializerMock;

        private IDocumentSerializer GetSerializer(IEnumerable<byte> key, IEnumerable<byte> iv)
        {
            internalSerializerMock = Mock.Of<IDocumentSerializer>();
            var options = new AesEncryptionOptions(key, iv);

            return new AesEncryptedDocumentSerializer(
                internalSerializer: internalSerializerMock,
                options
            );
        }

        [TestCase(0, 16)]
        [TestCase(16, 0)]
        [TestCase(1000, 16)]
        [TestCase(16, 1000)]
        public async Task Test_Serialize_WritesToStream(int keyLength, int ivLength)
        {
            // Arrange
            Mock.Get(internalSerializerMock)
                .SetReturnsDefault(Task.CompletedTask);

            var serializer = GetSerializer(
                Enumerable.Repeat((byte)0, keyLength),
                Enumerable.Repeat((byte)0, ivLength)
            );

            var writeStream = new MemoryStream();
            var testData = "ABC";

            // Act
            await serializer.SerializeAsync(
                stream: writeStream,
                data: testData
            );

            // Assert
            Assert.IsNotEmpty(writeStream.ToArray());
        }

        [Test]
        public void Test_Deserialize_DoesNotThrow()
        {
            // Arrange
            var readStream = new MemoryStream(new byte[] { 1, 2, 3 });

            var serializer = GetSerializer(
                Enumerable.Repeat((byte)0, 16),
                Enumerable.Repeat((byte)0, 16)
            );

            // Act & Assert
            Assert.DoesNotThrowAsync(
                () => serializer.DeserializeAsync<string>(readStream)
            );
        }
    }
}