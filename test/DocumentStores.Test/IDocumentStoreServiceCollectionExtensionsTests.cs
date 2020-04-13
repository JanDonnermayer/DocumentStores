using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class IDocumentStoreServiceCollectionExtensionsTests
    {
        private IDocumentStoreServiceCollection serviceCollectionMock;

        [SetUp]
        public void SetUp()
        {
            serviceCollectionMock = Mock.Of<IDocumentStoreServiceCollection>();
        }

        [Test]
        public void Test_AddTopic_AddsIDocumentTopic()
        {
            // Arrange && Act
            serviceCollectionMock.AddDocumentTopic<string>();

            // Assert
            Mock.Get(serviceCollectionMock)
                .Verify(
                    sc => sc.Add(
                        It.Is<ServiceDescriptor>(
                            sd => sd.ServiceType == typeof(IDocumentTopic<string>)
                        )
                    ),
                    Times.Once
                );
        }
    }
}