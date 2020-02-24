using System.Threading.Tasks;
using NUnit.Framework;
using System.Reactive.Linq;
using Moq;

using static DocumentStores.Result;
using System.Linq;
using System.Collections.Generic;

namespace DocumentStores.Test
{
    [TestFixture]
    public class IDocumentTopicExtensionTests
    {
        private IDocumentTopic<string> topicMock;

        [SetUp]
        public void SetUp()
        {
            topicMock = Mock.Of<IDocumentTopic<string>>();
        }

        [Test]
        public async Task Test_GetAll()
        {
            // Arrange
            var key1 = DocumentKey.FromString("key1");
            var key2 = DocumentKey.FromString("key2");
            var keys = new[] { key1, key2 }.AsEnumerable();

            Mock.Get(topicMock)
                .Setup(t => t.GetKeysAsync())
                .Returns(Task.FromResult(keys));

            void SetupReturns(DocumentKey key, string val) =>
                Mock.Get(topicMock)
                    .Setup(t => t.GetAsync(It.Is<DocumentKey>(k => key.Equals(k))))
                    .Returns(Task.FromResult(Ok(val)));

            var val1 = "val1";
            var val2 = "val2";

            SetupReturns(key1, val1);
            SetupReturns(key2, val2);

            var expectedResult = new Dictionary<DocumentKey, string>() {
                { key1, val1 },
                { key2, val2 }
            };

            // Act 
            var actualResult = await topicMock.GetAllAsync();

            // Assert
            Assert.That(Enumerable.SequenceEqual(actualResult, expectedResult));
        }
    }
}