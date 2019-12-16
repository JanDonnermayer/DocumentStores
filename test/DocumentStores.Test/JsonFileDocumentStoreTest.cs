using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive;
using static DocumentStores.Test.TestEnvironment;
using DocumentStores.Primitives;

namespace DocumentStores.Test
{
    [TestFixture]
    class JsonFileDocumentStoreTest
    {

        [OneTimeSetUp]
        public void CreateTestDirectory()
        {
            var dir = GetRootTestDir();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }


        [OneTimeTearDown]
        public void DeleteTestDirectory()
        {
            Directory.Delete(GetRootTestDir(), recursive: true);
        }

        [Test]
        public async Task Put_Then_Delete__ContainsCorrectAddresses()
        {
            var service = GetService();
            const string ADDRESS = "KEY";
            const string VALUE = "VALUE";
           
            await service.PutDocumentAsync(ADDRESS, VALUE);
            var mut_addresses = await service.GetAddressesAsync<string>();
            Assert.IsTrue(mut_addresses.Contains(ADDRESS));

            await service.DeleteDocumentAsync<string>(ADDRESS);
            mut_addresses = await service.GetAddressesAsync<string>();
            Assert.IsFalse(mut_addresses.Contains(ADDRESS));
        }

        [Test]
        public Task Put_Multiple__NotifiesObserver_Multiple() =>
            Operation_Multiple__NotifiesObserver_Multiple(
                topic => topic.PutDocumentAsync(
                    key: "KEY",
                    data: ImmutableCounter.Default));

        [Test]
        public Task AddOrUpdate_Multiple__NotifiesObserver_Multiple() =>
            Operation_Multiple__NotifiesObserver_Multiple(
                topic => topic.AddOrUpdateDocumentAsync(
                    key: "KEY",
                    initialData: ImmutableCounter.Default,
                    updateData: c => c.Increment()));

        [Test]
        public Task Delete_Multiple__NotifiesObserver_Multiple() =>
            Operation_Multiple__NotifiesObserver_Multiple(
                topic => topic.DeleteDocumentAsync("KEY"));

        private async Task Operation_Multiple__NotifiesObserver_Multiple(
            Func<IDocumentTopic<ImmutableCounter>, Task> operation)
        {
            var service = GetService().CreateTopic<ImmutableCounter>();

            string KEY = Guid.NewGuid().ToString();
            const int OBSERVER_DELAY_MS = 100;
            const int OPERATION_COUNT = 3;
            const int EXPECTED_NOTIFCATION_COUNT = OPERATION_COUNT + 1; // 1 is initial

            int mut_ActualNotificationCount = 0;

            var observable = service.GetKeysObservable();
            using var _ = observable.Subscribe(_ => mut_ActualNotificationCount += 1);

            for (int i = 0; i < OPERATION_COUNT; i++)
                await operation.Invoke(service);

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(EXPECTED_NOTIFCATION_COUNT, mut_ActualNotificationCount);
        }


        [Test]
        public void GetNonExisting_SynchronousWaiting__ReturnsError()
        {
            var service = GetService()
                .CreateTopic<string>();

            const string KEY = "non-existant-key";

            var res = service.GetDocumentAsync(KEY).Result;

            Assert.IsFalse(res.Try());
        }


        [Test]
        public void PutAndGet_SynchronousWaiting__ReturnsOk()
        {
            var service = GetService()
                .CreateTopic<string>();

            const string KEY = "KEY";

            var res1 = service.PutDocumentAsync(KEY, "TestVal").Result;
            var res2 = service.GetDocumentAsync(KEY).Result;

            Assert.IsTrue(res1.Try());
            Assert.IsTrue(res2.Try());
        }


        [Test]
        public async Task Put_Parallel__ReturnsOk()
        {
            var service = GetService().CreateTopic<ImmutableCounter>();

            const string KEY = "KEY";
            var counter = ImmutableCounter.Default;

            const int COUNT = 10;
            const int WORKER_COUNT = 20;

            await Task
                .WhenAll(Enumerable
                    .Range(1, WORKER_COUNT)
                    .Select(i => Task.Run(async () => await Task
                        .WhenAll(Enumerable.Range(1, COUNT)
                        .Select(async i => await service.AddOrUpdateDocumentAsync(
                            KEY,
                            ImmutableCounter.Default.Increment(),
                            _ => _.Increment()))))));


            var finalCounter = (await service.GetDocumentAsync(KEY)).PassOrThrow();

            (await service.DeleteDocumentAsync(KEY)).PassOrThrow();

            Assert.AreEqual(COUNT * WORKER_COUNT, finalCounter.Count);
        }


        // [Test]
        public async Task Put_InvalidFileNameKey__ReturnsOk()
        {
            var service = GetService().CreateTopic<ImmutableCounter>();

            string KEY = JsonConvert.SerializeObject(new { Name = "X", Value = "Buben" });
            var counter = ImmutableCounter.Default;

            var keys = Path
                .GetInvalidFileNameChars()
                .Select(_ => $@"{_}.$LOL.lel\{KEY}/''");

            var results = await Task.WhenAll(keys
                .Select(key => service.PutDocumentAsync(key, counter)));

            foreach (var res in results)
            {
                res.PassOrThrow();
            }

            var actualKeys = (await service.GetKeysAsync()).Select(_ => _.Value);

            Assert.IsTrue(
                condition: Enumerable.SequenceEqual(
                    first: keys,
                    second: actualKeys),
                message: "Keys differ after writing documents!");
        }
    }
}