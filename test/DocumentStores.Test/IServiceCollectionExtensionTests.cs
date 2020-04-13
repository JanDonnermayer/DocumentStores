using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class IServiceCollectionExtensionTests
    {
        private IServiceCollection serviceCollection;

        [SetUp]
        public void SetUp()
        {
            serviceCollection = new ServiceCollection();
        }

        [Test]
        public void Test_AddJsonFileDocumentStore_WithRootDirectory_ReturnsIDocumentStoreSC()
        {
            // Arrange
            const string ROOT_DIRECTORY = "C:/Temp";

            // Act
            var result = serviceCollection.AddJsonFileDocumentStore(ROOT_DIRECTORY);

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Test_AddJsonFileDocumentStore_WithRootDirectory_CanCreateIDocumentStore()
        {
            // Arrange
            const string ROOT_DIRECTORY = "C:/Temp";

            // Act
            serviceCollection.AddJsonFileDocumentStore(ROOT_DIRECTORY);

            // Assert
            Assert.DoesNotThrow(
                () => serviceCollection
                    .BuildServiceProvider()
                    .GetRequiredService<IDocumentStore>()
            );
        }

        [Test]
        public void Test_AddJsonFileDocumentStore_WithOptions_CanCreateIDocumentStore()
        {
            // Arrange
            var options = JsonFileDocumentStoreOptions.Default;

            // Act
            serviceCollection.AddJsonFileDocumentStore(options);

            // Assert
            Assert.DoesNotThrow(
                () => serviceCollection
                    .BuildServiceProvider()
                    .GetRequiredService<IDocumentStore>()
            );
        }
    }

}