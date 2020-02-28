using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DocumentStores.Internal;
using Moq;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    class AesEncryptedDocumentSerializerTest
    {
        private IDocumentSerializer internalSerializerMock;

        private IDocumentSerializer serializer;

        private IDocumentSerializer GetSerializer(IEnumerable<byte> key, IEnumerable<byte> iv)
        {
            internalSerializerMock = Mock.Of<IDocumentSerializer>();
            var options = new AesEncryptionOptions(key, iv);

            return new AesEncryptedDocumentSerializer(
                internalSerializer: internalSerializerMock,
                options
            );
        }

        [SetUp]
        public void SetUp()
        {
            serializer = GetSerializer(
                Enumerable.Repeat((byte)0, 3),
                Enumerable.Repeat((byte)0, 3)
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

            serializer = GetSerializer(
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
            Mock.Get(internalSerializerMock)
                .SetReturnsDefault(Task.FromResult(string.Empty));

            // Act & Assert
            Assert.DoesNotThrowAsync(
                () => serializer.DeserializeAsync<dynamic>(new MemoryStream())
            );
        }

        [Test]
        public void Test_when_InternalSerializer_Throws_CryptographicException_then_ThrowsSerializationException()
        {
            // Arrange
            Mock.Get(internalSerializerMock)
                .Setup(s => s.DeserializeAsync<dynamic>(It.IsAny<Stream>()))
                .ThrowsAsync(new CryptographicException());

            // Act & Assert
            Assert.ThrowsAsync<SerializationException>(
                () => serializer.DeserializeAsync<dynamic>(new MemoryStream())
            );
        }
    }
}