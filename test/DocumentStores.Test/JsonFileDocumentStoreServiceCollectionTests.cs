using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class JsonFileDocumentStoreServiceCollectionTests
    {
        [Test]
        public void Test_Constructor_DoesNotThrow()
        {
            Assert.DoesNotThrow(
                () => new JsonFileDocumentStoreServiceCollection(
                    new ServiceCollection(),
                    JsonFileDocumentStoreOptions.Default
                )
            );
        }

        [Test]
        public void Test_Constructor_CanCreateIDocumentStore()
        {
            // Arrange 
            var serviceCollection = new JsonFileDocumentStoreServiceCollection(
                new ServiceCollection(),
                JsonFileDocumentStoreOptions.Default
            );

            // Act && Assert
            Assert.DoesNotThrow(
                () => serviceCollection
                    .BuildServiceProvider()
                    .GetRequiredService<IDocumentStore>()
            );
        }
    }
}