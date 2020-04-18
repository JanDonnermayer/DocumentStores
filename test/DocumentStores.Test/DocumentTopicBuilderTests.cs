using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using DocumentStores.Internal;

namespace DocumentStores.Test
{
    [TestFixture]
    public class DocumentTopicBuilderTests
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

        public void Test_Constructor_DoesNotThrow()
        {
            // Arrange
            var options = JsonFileDocumentStoreOptions.Default;

            // Act && Assert
            Assert.DoesNotThrow(
                () => new DocumentTopicBuilder(serviceCollectionMock)
            );
        }
    }

    public interface ITestService { }
}