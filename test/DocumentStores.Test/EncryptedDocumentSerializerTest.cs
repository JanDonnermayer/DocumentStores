using System;
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
        private IDocumentSerializer documentSerializerMock;

        private EncryptedDocumentSerializer encryptedDocumentSerializer;



        [SetUp]
        public void SetUp()
        {
            documentSerializerMock = Mock.Of<IDocumentSerializer>();
            encryptedDocumentSerializer = new EncryptedDocumentSerializer(
                serializer: documentSerializerMock,
                key: Enumerable.Range(0, 16).Select(i => (byte)(i)).ToArray(),
                IV: Enumerable.Range(0, 16).Select(i => (byte)(i)).ToArray()
            );
        }

        [Test]
        public async Task Test_Serialize_WritesToStream()
        {
            // Arrange
            Mock.Get(documentSerializerMock)
                .SetReturnsDefault(Task.CompletedTask);

            var writeStream = new MemoryStream();
            var testData = "ABC";

            // Act
            await encryptedDocumentSerializer.SerializeAsync(
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

            // Act & Assert
            Assert.DoesNotThrowAsync(
                () => encryptedDocumentSerializer.DeserializeAsync<string>(readStream)
            );
        }

        [Test]
        public async Task Test_Roundtrip_DataEquals()
        {
            var stream = new MemoryStream();
        }
    }
}