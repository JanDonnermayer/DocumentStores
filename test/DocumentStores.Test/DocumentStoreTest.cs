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
using DocumentStores.Internal;

namespace DocumentStores.Test
{
    [TestFixture]
    class DocumentStoreTest
    {
        private static IDocumentStore GetService() =>
            new DocumentStore(
                new JsonDocumentSerializer(),
                new InMemoryDataStore());


        [Test]
        public void Put_Then_Delete__ContainsCorrectAddresses()
        {
            var service = GetService();
            const string ADDRESS = "KEY";
            const string VALUE = "VALUE";

            service.PutAsync(ADDRESS, VALUE).Wait();
            var mut_addresses = service.GetAddressesAsync<string>().Result;
            Assert.IsTrue(mut_addresses.Contains(ADDRESS));

            service.DeleteAsync<string>(ADDRESS).Wait();
            mut_addresses = service.GetAddressesAsync<string>().Result;
            Assert.IsFalse(mut_addresses.Contains(ADDRESS));
        }



        [Test]
        public void Put_And_Get__ReturnsOk()
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
        public void Get_NonExisting__ReturnsError()
        {
            var service = GetService();

            const string KEY = "non-existant-key";

            var res = service.GetAsync<string>(KEY).Result;

            Assert.IsFalse(res.Try());
        }

        [Test]
        public void AddOrUpdate_NonExisting__ReturnsOk()
        {
            var service = GetService();

            const string KEY = "KEY";
            const string VALUE = "VALUE";

            var res = service.AddOrUpdateAsync(KEY, VALUE, _ => VALUE).Result;

            Assert.IsTrue(res.Try());
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