using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DocumentStores.Test
{
    [TestFixture]
    public class JsonFileDocumentStoreTest
    {
        private static string getTestDir() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "DocumentStore.Tests");

        private class ImmutableCounter
        {
            public int Count { get; }

            public ImmutableCounter(int count) => this.Count = count;

            public static ImmutableCounter Default => new ImmutableCounter(0);

            public ImmutableCounter Increment() => new ImmutableCounter(Count + 1);
        }


        private static JsonFileDocumentStore GetService()
        {
            var service = new JsonFileDocumentStore(getTestDir());
            return service;
        }



        [Test]
        public async Task ParallelInputTest()
        {

            var service = GetService().AsObservableDocumentStore<ImmutableCounter>();

            var testDir = getTestDir();
            if (!Directory.Exists(testDir)) Directory.CreateDirectory(testDir);

            var counter = ImmutableCounter.Default;
            var key = Guid.NewGuid().ToString();

            // (await service.GetOrAddDocumentAsync(key, _ => Task.FromResult(counter))).PassOrThrow();
            // (await service.PutDocumentAsync(key, counter)).PassOrThrow();

            const int COUNT = 10;
            const int WORKER_COUNT = 20;

            await Task
                .WhenAll(Enumerable
                    .Range(1, WORKER_COUNT)
                    .Select(i => Task.Run(async () => await Task
                        .WhenAll(Enumerable.Range(1, COUNT)
                        .Select(async i => await service.AddOrUpdateDocumentAsync(
                            key,
                            _ => Task.FromResult(ImmutableCounter.Default.Increment()),
                            (_, c) => Task.FromResult(c.Increment())))))));


            var finalCounter = (await service.GetDocumentAsync(key)).PassOrThrow();

            (await service.DeleteDocumentAsync(key)).PassOrThrow();

            Assert.AreEqual(COUNT * WORKER_COUNT, finalCounter.Count);

            Directory.Delete(testDir, true);
        }


        [Test]
        public async Task AddForbiddenFileNamedDocs()
        {

            var service = GetService().AsObservableDocumentStore<ImmutableCounter>();

            var testDir = getTestDir();
            if (!Directory.Exists(testDir)) Directory.CreateDirectory(testDir);

            var counter = ImmutableCounter.Default;

            var keys = Path.GetInvalidFileNameChars().Select(_ => $"{_}");

            var results = await Task.WhenAll(keys
                .Select(_ => service.PutDocumentAsync(_, counter)));

            foreach (var res in results)
            {
                res.PassOrThrow();
            }

            var actualKeys = await service.GetKeysAsync();

            Assert.IsTrue(
                condition: Enumerable.SequenceEqual(
                    first: keys,
                    second: actualKeys,
                    comparer: StringComparer.OrdinalIgnoreCase),
                message: "Keys differ after writing documents!");

            Directory.Delete(testDir, true);
        }

        [Test]
        public async Task TestObserving()
        {
            var service = GetService().AsObservableDocumentStore<ImmutableCounter>();
            const string key = "Xbuben";

            static async Task TestAdd(IObservableDocumentStore<ImmutableCounter> service, string key)
            {
                var tcs = new TaskCompletionSource<IEnumerable<string>>(TimeSpan.FromSeconds(3));
                await service.PutDocumentAsync(key, ImmutableCounter.Default); // Subscribe after add
                var obs = service.GetKeysObservable().Subscribe(_ => tcs.TrySetResult(_));
                var keys = await tcs.Task;
                Assert.AreEqual(key, keys.First());

                var tcs2 = new TaskCompletionSource<IEnumerable<string>>(TimeSpan.FromSeconds(3));
                var obs2 = service.GetKeysObservable().Subscribe(_ => tcs2.TrySetResult(_));
                var keys2 = await tcs2.Task;
                Assert.AreEqual(key, keys2.First());

            }

            static async Task TestRemove(IObservableDocumentStore<ImmutableCounter> service, string key)
            {
                var tcs = new TaskCompletionSource<IEnumerable<string>>(TimeSpan.FromSeconds(3));
                var obs = service.GetKeysObservable().Subscribe(_ => tcs.TrySetResult(_));
                await service.DeleteDocumentAsync(key);
                var keys = await tcs.Task;
                Assert.IsEmpty(keys);
            }

            await TestAdd(service, key);
            await TestRemove(service, key);
        }


    }
}