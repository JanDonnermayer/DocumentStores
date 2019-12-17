using System.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Reactive.Linq;
using static DocumentStores.Test.TestEnvironment;
using System.IO;

namespace DocumentStores.Test
{
    class DocumentTopicTest
    {
        private static string GetRootTestDir() =>
            Path.Combine(
                Path.GetTempPath(),
                TestContext.CurrentContext.Test.ClassName
            );

        private static string GetTestDir() =>
            Path.Combine(
                GetRootTestDir(),
                TestContext.CurrentContext.Test.Name,
                Guid.NewGuid().ToString()
            );

        private static JsonFileDocumentStore GetService() =>
            new JsonFileDocumentStore(GetTestDir());


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
            var dir = GetRootTestDir();
            if (!Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
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
    }
}