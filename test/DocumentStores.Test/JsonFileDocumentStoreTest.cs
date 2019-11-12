using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Linq;

namespace DocumentStores.Test
{
    [TestFixture]
    public class JsonFileDocumentStoreTest
    {
        private static readonly string testDir = Path.Combine(Path.GetTempPath(), "ArrkTaskTracker.Tests");

        private class ImmutableCounter
        {
            public int Count { get; }

            public ImmutableCounter(int count) => this.Count = count;

            public static ImmutableCounter Default => new ImmutableCounter(0);

            public ImmutableCounter Increment() => new ImmutableCounter(Count + 1);
        }


        private static JsonFileDocumentStore GetService()
        {
            var service = new JsonFileDocumentStore(testDir);
            return service;
        }




        [Test]
        public async Task ParallelInputTest()
        {

            var service = GetService().AsTypedDocumentStore<ImmutableCounter>();

            if (!Directory.Exists(testDir)) Directory.CreateDirectory(testDir);

            var counter = ImmutableCounter.Default;
            var key = Guid.NewGuid().ToString();

            (await service.GetOrAddDocumentAsync(key, _ => Task.FromResult(counter))).PassOrThrow();
            //(await service.PutDocumentAsync(key, counter)).PassOrThrow();

            const int COUNT = 10;
            const int WORKER_COUNT = 20;

            await Task
                .WhenAll(Enumerable
                    .Range(1, WORKER_COUNT)
                    .Select(i => Task.Run(async () => await Task
                        .WhenAll(Enumerable.Range(1, COUNT)
                        .Select(async i => await service.AddOrUpdateDocumentAsync(
                            key,
                            _ => Task.FromResult(ImmutableCounter.Default),
                            (_, c) => Task.FromResult(c.Increment())))))));


            var finalCounter = (await service.GetDocumentAsync(key)).PassOrThrow();

            (await service.DeleteDocumentAsync(key)).PassOrThrow();

            Assert.AreEqual(COUNT * WORKER_COUNT, finalCounter.Count);

            Directory.Delete(testDir, true);
        }


    }
}