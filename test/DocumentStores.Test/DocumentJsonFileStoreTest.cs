using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using DocumentStores.Abstractions;
using System;
using System.Linq;

namespace DocumentStores.Test
{
    [TestFixture]
    public class DocumentJsonFileStoreTest
    {
        private static readonly string testDir = Path.Combine(Path.GetTempPath(), "ArrkTaskTracker.Tests");

        private class ImmutableCounter
        {
            public int Count { get; }

            public ImmutableCounter(int count) => this.Count = count;

            public static ImmutableCounter Default => new ImmutableCounter(0);

            public ImmutableCounter Increment() => new ImmutableCounter(Count + 1);
        }


        private static IDocumentStore GetService()
        {
            var logger = new Moq.Mock<ILogger<JsonFileDocumentStore>>().Object;
            var service = new JsonFileDocumentStore(testDir, logger);
            return service;
            //var cache = new DocumentCacheService(service);
            //return cache;
        }

        [Test]
        public async Task ParallelInputTest()
        {

            var service = GetService();

            if (!Directory.Exists(testDir)) Directory.CreateDirectory(testDir);

            var counter = ImmutableCounter.Default;
            var key = Guid.NewGuid().ToString();
            await service.PutDocumentAsync(key, counter);

            const int COUNT = 10;
            const int WORKER_COUNT = 10;

            await Task
                .WhenAll(Enumerable
                    .Range(1, WORKER_COUNT)
                    .Select(i => Task.Run(async () => await Task
                        .WhenAll(Enumerable.Range(1, COUNT)
                        .Select(async i => await service.TransformDocumentAsync<ImmutableCounter>(key, c => c.Increment()))))));

            var finalCounter = await service.GetDocumentAsync<ImmutableCounter>(key);
            Assert.AreEqual(COUNT * WORKER_COUNT, finalCounter.Data.Count);

            Directory.Delete(testDir, true);
        }




    }
}