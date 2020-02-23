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
        private IDocumentSerializer internalSerializerMock;

        private IDocumentSerializer serializer;



        [SetUp]
        public void SetUp()
        {
            internalSerializerMock = Mock.Of<IDocumentSerializer>();
            serializer = new RijndaelEncryptedDocumentSerializer(
                internalSerializer: internalSerializerMock
            );
        }

        [Test]
        public async Task Test_Serialize_WritesToStream()
        {
            // Arrange
            Mock.Get(internalSerializerMock)
                .SetReturnsDefault(Task.CompletedTask);

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

            // Act & Assert
            Assert.DoesNotThrowAsync(
                () => serializer.DeserializeAsync<string>(readStream)
            );
        }

        [Test]
        public void Test_New_InvalidKeySize_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new RijndaelEncryptedDocumentSerializer(
                    internalSerializer: internalSerializerMock,
                    key: Array.Empty<byte>()
                )
            );
        }

        [Test]
        public void Test_New_InvalidIVSize_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new RijndaelEncryptedDocumentSerializer(
                    internalSerializer: internalSerializerMock,
                    iV: Array.Empty<byte>()
                )
            );
        }
    }
}