using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class IServiceCollectionExtensionTests
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
        public void Test_AddJsonFileDocumentStore_WithRootDirectory_AddsIDocumentStore()
        {
            // Arrange
            const string rootDirectory = "Some directory";

            // Act
            _ = serviceCollectionMock.AddJsonFileDocumentStore(rootDirectory);

            // Assert
            Mock.Get(serviceCollectionMock)
                .Verify(
                    sc => sc.Add(
                        It.Is<ServiceDescriptor>(
                            sd => sd.ServiceType == typeof(IDocumentStore)
                        )
                    ),
                    Times.Once
                );
        }

        [Test]
        public void Test_AddJsonFileDocumentStore_WithOptions_AddsIDocumentStore()
        {
            // Arrange
            var options = JsonFileDocumentStoreOptions.Default;

            // Act
            _ = serviceCollectionMock.AddJsonFileDocumentStore(options);

            // Assert
            Mock.Get(serviceCollectionMock)
                .Verify(
                    sc => sc.Add(
                        It.Is<ServiceDescriptor>(
                            sd => sd.ServiceType == typeof(IDocumentStore)
                        )
                    ),
                    Times.Once
                );
        }
    }
}