using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using DocumentStores.Internal;

namespace DocumentStores.Test
{
    [TestFixture]
    public class GenericServiceCollectionTests
    {
        private IEnumerator<ServiceDescriptor> enumeratorMock;

        private IServiceCollection serviceCollectionMock;

        private ITestService serviceMock;

        [SetUp]
        public void SetUp()
        {
            enumeratorMock = Mock.Of<IEnumerator<ServiceDescriptor>>();
            serviceMock = Mock.Of<ITestService>();
            serviceCollectionMock = Mock.Of<IServiceCollection>(
                sc => sc.GetEnumerator() == enumeratorMock
            );
        }

        [TestCase(ServiceLifetime.Singleton)]
        [TestCase(ServiceLifetime.Scoped)]
        [TestCase(ServiceLifetime.Transient)]
        public void Test_Constructor_AddsServiceToSpecifiedCollection(ServiceLifetime lifetime)
        {
            // Arrange
            var options = JsonFileDocumentStoreOptions.Default;

            // Act
            _ = new GenericServiceCollection<ITestService>(
               collection: serviceCollectionMock,
               factory: _ => serviceMock,
               lifetime: lifetime
            );

            // Assert
            Mock.Get(serviceCollectionMock)
                .Verify(
                    sc => sc.Add(
                        It.Is<ServiceDescriptor>(
                            sd => sd.ServiceType == typeof(ITestService)
                            && sd.Lifetime == lifetime
                        )
                    ),
                    Times.Once
                );
        }
    }

    public interface ITestService { }
}