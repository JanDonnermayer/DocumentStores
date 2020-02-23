using System.IO;
using System.Threading.Tasks;
using DocumentStores.Internal;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    class JsonDocumentserializerTest
    {
        private IDocumentSerializer serializer;


        [SetUp]
        public void SetUp()
        {
            serializer = new JsonDocumentSerializer();
        }

        [Test]
        public async Task Test_Serialize_WritesToStream()
        {
            // Arrange
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
        public async Task Test_Roundtrip_DataEquals()
        {
            // Arrange
            var testData = "ABC";
            var stream = new MemoryStream();

            // Act
            await serializer.SerializeAsync(stream, testData);
            stream.Position = 0;
            var roundtrippedData = await serializer.DeserializeAsync<string>(stream);

            // Assert
            Assert.AreEqual(testData, roundtrippedData);
        }
    }
}