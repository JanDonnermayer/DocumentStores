using System.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Reactive.Linq;
using System.IO;
using DocumentStores;
using Moq;

using static DocumentStores.Result;
using System.Collections.Generic;

namespace DocumentStores.Test
{
    internal class DocumentTopicTest
    {
        IDocumentStore storeMock;

        [SetUp]
        public void SetUp()
        {
            storeMock = Mock.Of<IDocumentStore>();
        }

        [Test]
        public Task Test_Put_Multiple__NotifiesObserver_Multiple() =>
            Test_Operation_Multiple__NotifiesObserver_Multiple(
                topic => topic.PutAsync(
                    key: "KEY",
                    data: ImmutableCounter.Default));

        [Test]
        public Task Test_AddOrUpdate_Multiple__NotifiesObserver_Multiple() =>
            Test_Operation_Multiple__NotifiesObserver_Multiple(
                topic => topic.AddOrUpdateAsync(
                    key: "KEY",
                    initialData: ImmutableCounter.Default,
                    updateData: c => c.Increment()));

        [Test]
        public Task Test_Delete_Multiple__NotifiesObserver_Multiple() =>
            Test_Operation_Multiple__NotifiesObserver_Multiple(
                topic => topic.DeleteAsync("KEY"));

        private async Task Test_Operation_Multiple__NotifiesObserver_Multiple<TReturnData>(
            Func<IDocumentTopic<ImmutableCounter>, Task<Result<TReturnData>>> operation)
            where TReturnData : class
        {
            Mock.Get(storeMock)
                .SetReturnsDefault(Task.FromResult(Ok()));

            Mock.Get(storeMock)
                .SetReturnsDefault(Task.FromResult(Ok(ImmutableCounter.Default)));

            Mock.Get(storeMock)
                .SetReturnsDefault(Task.FromResult<IEnumerable<DocumentKey>>(new DocumentKey[] { }));

            var topic = storeMock.ToTopic<ImmutableCounter>();

            string KEY = Guid.NewGuid().ToString();
            const int OBSERVER_DELAY_MS = 100;
            const int OPERATION_COUNT = 3;
            const int EXPECTED_NOTIFICATION_COUNT = OPERATION_COUNT + 1; // 1 is initial

            int mut_ActualNotificationCount = 0;

            var observable = topic.GetKeysObservable();
            using var _ = observable.Subscribe(
                _ => mut_ActualNotificationCount += 1,
                e => throw e
            );

            for (int i = 0; i < OPERATION_COUNT; i++)
                (await operation.Invoke(topic)).PassOrThrow();

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(EXPECTED_NOTIFICATION_COUNT, mut_ActualNotificationCount);
        }
    }
}