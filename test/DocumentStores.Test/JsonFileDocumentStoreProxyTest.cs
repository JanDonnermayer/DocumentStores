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
using DocumentStores.Primitives;
using static DocumentStores.Test.TestEnvironment;


namespace DocumentStores.Test
{
    [TestFixture]
    class JsonFileDocumentStoreChannelTest
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
        public Task AddOrUpdate_Multiple__NotifiesObserver_Multiple() =>
            Operation_Multiple__NotifiesObserver_Multiple(channel =>
                channel.AddOrUpdateDocumentAsync(
                    initialData: ImmutableCounter.Default,
                    updateData: c => c.Increment()));

        [Test]
        public Task Put_Multiple__NotifiesObserver_Multiple() =>
            Operation_Multiple__NotifiesObserver_Multiple(channel =>
                channel.PutDocumentAsync(ImmutableCounter.Default));

        [Test]
        public Task Delete_Multiple__NotifiesObserver_Multiple() =>
            Operation_Multiple__NotifiesObserver_Multiple(channel =>
                channel.DeleteDocumentAsync());

        private async Task Operation_Multiple__NotifiesObserver_Multiple(
            Func<IDocumentChannel<ImmutableCounter>, Task> operation)
        {
            var service = GetService()
                .AsObservableDocumentStore<ImmutableCounter>();

            string KEY = Guid.NewGuid().ToString();
            const int OBSERVER_DELAY_MS = 100;
            const int OPERATION_COUNT = 3;
            const int EXPECTED_NOTIFCATION_COUNT = OPERATION_COUNT + 1; // 1 is initial

            int mut_ActualNotificationCount = 0;

            var channel = service.CreateChannel(KEY);

            var observable = service.GetKeysObservable();
            using var _ = observable.Subscribe(_ => mut_ActualNotificationCount += 1);

            for (int i = 0; i < OPERATION_COUNT; i++)
                await operation.Invoke(channel);

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(EXPECTED_NOTIFCATION_COUNT, mut_ActualNotificationCount);
        }


    }




}