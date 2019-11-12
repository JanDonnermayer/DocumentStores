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
        public async Task AddOrUpdateDocumentAsyncTest()
        {

            var service = GetService();

            if (!Directory.Exists(testDir)) Directory.CreateDirectory(testDir);

            var counter = ImmutableCounter.Default;
            var key = Guid.NewGuid().ToString();

            if (!(await service.PutDocumentAsync(key, counter)).Try(out var ex))
                throw new Exception("Put document failed!", ex);

            const int COUNT = 100;
            const int WORKER_COUNT = 10;

            await Task
                .WhenAll(Enumerable
                    .Range(1, WORKER_COUNT)
                    .Select(i => Task.Run(async () => await Task
                        .WhenAll(Enumerable.Range(1, COUNT)
                        .Select(async i => await service.AddOrUpdateDocumentAsync(
                            key,
                            _ => Task.FromResult(ImmutableCounter.Default),
                            (_, c) => Task.FromResult(c.Increment())))))));


            ImmutableCounter finalCounter = await service.GetDocumentAsync<ImmutableCounter>(key);
            Assert.AreEqual(COUNT * WORKER_COUNT, finalCounter.Count);

            Directory.Delete(testDir, true);
        }


    }
}