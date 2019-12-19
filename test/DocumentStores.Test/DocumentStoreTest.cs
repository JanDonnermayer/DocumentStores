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
    class DocumentStoreTest
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
        public async Task Put_Then_Delete__ContainsCorrectAddresses()
        {
            var service = GetService();
            const string ADDRESS = "KEY";
            const string VALUE = "VALUE";

            await service.PutAsync(ADDRESS, VALUE);
            var mut_addresses = await service.GetAddressesAsync<string>();
            Assert.IsTrue(mut_addresses.Contains(ADDRESS));

            await service.DeleteAsync<string>(ADDRESS);
            mut_addresses = await service.GetAddressesAsync<string>();
            Assert.IsFalse(mut_addresses.Contains(ADDRESS));
        }



        [Test]
        public void Put_And_Get_SynchronousWaiting__ReturnsOk()
        {
            var service = GetService();

            const string KEY = "KEY";
            const string VALUE = "VALUE";

            var res1 = service.PutAsync(KEY, VALUE).Result;
            var res2 = service.GetAsync<string>(KEY).Result;

            Assert.IsTrue(res1.Try());
            Assert.IsTrue(res2.Try(out string val));
            Assert.AreEqual(VALUE, val);

        }


        [Test]
        public void Get_NonExisting_SynchronousWaiting__ReturnsError()
        {
            var service = GetService();

            const string KEY = "non-existant-key";

            var res = service.GetAsync<string>(KEY).Result;

            Assert.IsFalse(res.Try());
        }


        [Test]
        public void Put_And_GetOrAdd__ReturnsOk()
        {
            var service = GetService();

            const string KEY = "KEY";
            const string VALUE = "VALUE";

            var res1 = service.PutAsync(KEY, VALUE).Result;
            var res2 = service.GetOrAddAsync(KEY, VALUE).Result;

            Assert.IsTrue(res1.Try());
            Assert.IsTrue(res2.Try(out string val));
            Assert.AreEqual(VALUE, val);
        }


        [Test]
        public void GetOrAdd__ReturnsOk()
        {
            var service = GetService();

            const string KEY = "KEY";
            const string VALUE = "VALUE";

            var res2 = service.GetOrAddAsync(KEY, VALUE).Result;

            Assert.IsTrue(res2.Try(out string val));
            Assert.AreEqual(VALUE, val);

        }


        [Test]
        public async Task AddOrUpdate_Parallel__ReturnsOk()
        {
            var service = GetService();

            const string KEY = "KEY";
            var counter = ImmutableCounter.Default;

            const int COUNT = 10;
            const int WORKER_COUNT = 20;

            await Task
                .WhenAll(Enumerable
                    .Range(1, WORKER_COUNT)
                    .Select(i => Task.Run(async () => await Task
                        .WhenAll(Enumerable.Range(1, COUNT)
                        .Select(async i => await service.AddOrUpdateAsync(
                            address: KEY,
                            initialData: ImmutableCounter.Default.Increment(),
                            updateData: _ => _.Increment()))))));


            var finalCounter = (await service.GetAsync<ImmutableCounter>(KEY)).PassOrThrow();

            (await service.DeleteAsync<ImmutableCounter>(KEY)).PassOrThrow();

            Assert.AreEqual(COUNT * WORKER_COUNT, finalCounter.Count);
        }
    }
}