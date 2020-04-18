using System.Collections;
using System.Collections.Generic;
using DocumentStores.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class DependencyInjectionIntegrationTests
    {
        private IServiceCollection serviceCollection;

        private IDocumentStore storeMock;

        [SetUp]
        public void SetUp()
        {
            serviceCollection = new ServiceCollection();
            storeMock = Mock.Of<IDocumentStore>();
        }

        [Test]
        public void Test_Instance_AddTopic_AddChannel_CanCreateIDocumentChannel()
        {
            // Arrange && Act
            var serviceProvider = serviceCollection
                .AddSingletonGeneric(_ => storeMock)
                .AddDocumentTopic<string>()
                .AddDocumentChannel("key1")
                .BuildServiceProvider();

            T GetService<T>() => serviceProvider
                .GetRequiredService<T>();

            // Assert
            Assert.DoesNotThrow(
                () => GetService<IDocumentStore>()
            );

            Assert.DoesNotThrow(
                () => GetService<IDocumentTopic<string>>()
            );

            Assert.DoesNotThrow(
                () => GetService<IDocumentChannel<string>>()
            );
        }
    }
}