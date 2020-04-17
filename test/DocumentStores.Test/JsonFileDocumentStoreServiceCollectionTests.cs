using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class JsonFileDocumentStoreServiceCollectionTests
    {
        private IEnumerator<ServiceDescriptor> enumeratorMock;

        private IServiceCollection serviceCollectionMock;

        [SetUp]
        public void SetUp()
        {
            enumeratorMock = Mock.Of<IEnumerator<ServiceDescriptor>>();
            serviceCollectionMock = Mock.Of<IServiceCollection>(
                sc => sc.GetEnumerator() == enumeratorMock
            );
        }

        [Test]
        public void Test_Constructor_AddsIDocumentStore()
        {
            // Arrange
            var options = JsonFileDocumentStoreOptions.Default;

            // Act
            _ = new JsonFileDocumentStoreServiceCollection(
                serviceCollectionMock,
                options
            );

            // Assert
            Mock.Get(serviceCollectionMock)
                .Verify(
                    sc => sc.Add(
                        It.Is<ServiceDescriptor>(
                            sd => sd.ServiceType == typeof(IDocumentStore)
                            && sd.Lifetime == ServiceLifetime.Singleton
                        )
                    ),
                    Times.Once
                );
        }
    }
}