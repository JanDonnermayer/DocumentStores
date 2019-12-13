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

namespace DocumentStores.Test
{
    [TestFixture]
    class JsonFileDocumentStoreTest
    {

        private static String GetRootTestDir() =>
            Path.Combine(
                Path.GetTempPath(),
                "DocumentStore.Tests"
            );

        private static string GetTestDir() =>
            Path.Combine(
                GetRootTestDir(),
                TestContext.CurrentContext.Test.Name,
                Guid.NewGuid().ToString()
            );

        private static JsonFileDocumentStore GetService() =>
            new JsonFileDocumentStore(GetTestDir());


        [TearDown]
        public async Task DeleteTestDirectoryAsync()
        {
            var deleter = new Action(() => Directory.Delete(GetRootTestDir(), recursive: true));
            
            for (int i = 0; i < 5; i++)
            {
                try 
                {
                    deleter.Invoke();
                    return;
                }
                catch(UnauthorizedAccessException)
                {
                    await Task.Delay(100 * i);
                }
            }
        }

        [Test]
        public async Task Put_Parallel__ReturnsOk()
        {
            var service = GetService().AsObservableDocumentStore<ImmutableCounter>();

            const string KEY = "key";
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


        [Test]
        public async Task Put_InvalidFileNameKey__ReturnsOk()
        {
            var service = GetService()
            .AsObservableDocumentStore<ImmutableCounter>();

            string KEY = JsonConvert.SerializeObject(new { Name = "X", Value = "Buben" });
            var counter = ImmutableCounter.Default;

            var keys = Path.GetInvalidFileNameChars().Select(_ => $@"{_}.LOL.lel\{KEY}/''");

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
        }

        [Test]
        public async Task Put_Multiple__NotifiesObserver_Multiple()
        {
            var service = GetService()
                .AsObservableDocumentStore<ImmutableList<ImmutableCounter>>();

            const string KEY = "Xbuben";
            const int OBSERVER_DELAY_MS = 100;
            const int PUT_COUNT = 3;
            const int EXPECTED_NOTIFCATION_COUNT = PUT_COUNT + 1; // 1 is initial

            int mut_ActualNotificationCount = 0;

            var observable = service.GetKeysObservable();
            using var _ = observable.Subscribe(_ => mut_ActualNotificationCount += 1);

            for (int i = 0; i < PUT_COUNT; i++)
                await service.PutDocumentAsync(KEY, ImmutableList<ImmutableCounter>.Empty);

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(EXPECTED_NOTIFCATION_COUNT, mut_ActualNotificationCount);
        }

        [Test]
        public async Task Put_ThenObserver__NotifiesObserver()
        {
            var service = GetService()
                .AsObservableDocumentStore<ImmutableList<ImmutableCounter>>();

            const string KEY = "Xbuben";
            const int OBSERVER_DELAY_MS = 100;
            const int EXPECTED_NOTIFCATION_COUNT = 1; // 1 is initial

            int mut_ActualNotificationCount = 0;

            await service.PutDocumentAsync(KEY, ImmutableList<ImmutableCounter>.Empty);

            await Task.Delay(OBSERVER_DELAY_MS);

            var observable = service.GetKeysObservable();
            using var _ = observable.Subscribe(_ => mut_ActualNotificationCount += 1);

            Assert.AreEqual(EXPECTED_NOTIFCATION_COUNT, mut_ActualNotificationCount);
        }


        [Test]
        public async Task Proxy_AddOrUpdate_Multiple__NotifiesObserver_Multiple()
        {
            var service = GetService()
                .AsObservableDocumentStore<ImmutableCounter>();

            const string KEY = "Xbuben";
            const int OBSERVER_DELAY_MS = 100;
            const int ADD_OR_UPDATE_COUNT = 1;
            const int EXPECTED_NOTIFCATION_COUNT = ADD_OR_UPDATE_COUNT + 1; // 1 is initial

            int mut_ActualNotificationCount = 0;

            var proxy = service.CreateProxy(KEY);

            var observable = service.GetKeysObservable();
            using var _ = observable.Subscribe(_ => mut_ActualNotificationCount += 1);

            for (int i = 0; i < ADD_OR_UPDATE_COUNT; i++)
            {
                await proxy.AddOrUpdateDocumentAsync(
                    initialData: ImmutableCounter.Default,
                    updateData: c => c.Increment()
                );
            }

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(EXPECTED_NOTIFCATION_COUNT, mut_ActualNotificationCount);
        }


        [Test]
        public void GetNonExisting_SynchronousWaiting__ReturnsError()
        {
            var service = GetService()
                .AsObservableDocumentStore<string>();

            const string KEY = "non-existant-key";

            var res = service.GetDocumentAsync(KEY).Result;

            Assert.IsFalse(res.Try());
        }


        [Test]
        public void PutAndGet_SynchronousWaiting__ReturnsOk()
        {
            var service = GetService()
                .AsObservableDocumentStore<string>();

            const string KEY = "key";

            var res1 = service.PutDocumentAsync(KEY, "TestVal").Result;
            var res2 = service.GetDocumentAsync(KEY).Result;

            Assert.IsTrue(res1.Try());
            Assert.IsTrue(res2.Try());
        }

        #region  Private Types

        private class ImmutableCounter
        {
            public int Count { get; }

            public ImmutableCounter(int count) => this.Count = count;

            public static ImmutableCounter Default => new ImmutableCounter(0);

            public ImmutableCounter Increment() => new ImmutableCounter(Count + 1);

            public override bool Equals(object obj) =>
                obj is ImmutableCounter counter &&
                       Count == counter.Count;

            public override int GetHashCode() =>
                HashCode.Combine(Count);
        }

        #endregion
    }



}