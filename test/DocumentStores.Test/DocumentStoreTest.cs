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
    internal class DocumentStoreTest
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

            const string KEY = "KEY";

            var res = service.GetAsync<string>(KEY).Result;

            Assert.IsFalse(res.Try());
        }

        [Test]
        public void AddOrUpdate__ReturnsOk()
        {
            var service = GetService();

            const string KEY = "KEY";
            const string VALUE = "VALUE";

            var res = service.AddOrUpdateAsync(KEY, VALUE, _ => VALUE).Result;

            Assert.IsTrue(res.Try());
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

            _ = await Task
                .WhenAll(Enumerable
                    .Range(1, WORKER_COUNT)
                    .Select(_ =>
                        Task.Run(() =>
                            Task.WhenAll(Enumerable.Range(1, COUNT)
                                .Select(_ => service.AddOrUpdateAsync(
                                    address: KEY,
                                    initialData: ImmutableCounter.Default.Increment(),
                                    updateData: _ => _.Increment())
                                )
                            )
                        )
                    )
                ).ConfigureAwait(false);


            var finalCounter = (await service.GetAsync<ImmutableCounter>(KEY).ConfigureAwait(false)).PassOrThrow();

            (await service.DeleteAsync<ImmutableCounter>(KEY).ConfigureAwait(false)).PassOrThrow();

            Assert.AreEqual(COUNT * WORKER_COUNT, finalCounter.Count);
        }


        [Test]
        public void Put_InvalidData_ThrowsArgumentException()
        {
            var service = GetService();

            string VALID_KEY = "KEY";
            object VALID_DATA = new object();
            object INVALID_DATA = null;

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.PutAsync<dynamic>(VALID_KEY, INVALID_DATA));
        }

        [Test]
        public void Put_InvalidKey_ThrowsArgumentException()
        {
            var service = GetService();

            object VALID_DATA = new object();
            const string INVALID_KEY = "";

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.PutAsync<dynamic>(INVALID_KEY, VALID_DATA));
        }

        [Test]
        public void Get_InvalidKey_ThrowsArgumentException()
        {
            var service = GetService();

            const string INVALID_KEY = "";

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetAsync<dynamic>(INVALID_KEY));
        }

        [Test]
        public void Delete_InvalidKey_ThrowsArgumentException()
        {
            var service = GetService();

            const string INVALID_KEY = "";

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.DeleteAsync<dynamic>(INVALID_KEY));
        }

        [Test]
        public void AddOrUpdate_InvalidKey_ThrowsArgumentException()
        {
            var service = GetService();

            object VALID_DATA = new object();
            const string INVALID_KEY = "";

            Task<object> ValidAddDataFactory(DocumentAddress address) =>
                Task.FromResult(VALID_DATA);

            Task<object> ValidUpdateDataFactory(DocumentAddress address, object data) =>
                Task.FromResult(VALID_DATA);

            Assert.ThrowsAsync<ArgumentException>(() =>
                service.AddOrUpdateAsync(
                    address: INVALID_KEY,
                    addDataAsync: ValidAddDataFactory,
                    updateDataAsync: ValidUpdateDataFactory
                )
            );
        }


        [Test]
        public async Task AddOrUpdateExtensions_InvalidInitialData_ReturnsError()
        {
            var service = GetService();

            const string VALID_KEY = "KEY";
            object VALID_DATA = new object();
            object INVALID_DATA = null;

            var res = await IDocumentStoreExtensions
                .AddOrUpdateAsync(
                    source: service,
                    address: VALID_KEY,
                    initialData: INVALID_DATA,
                    updateData: _ => VALID_DATA
                )
                .ConfigureAwait(false);

            Assert.IsFalse(res.Try(out var _, out Exception ex));
            Assert.IsInstanceOf<DocumentException>(ex);
        }


        [Test]
        public async Task AddOrUpdate_InvalidAddDataFactory_ReturnsError()
        {
            var service = GetService();

            string KEY = "KEY";
            object VALID_DATA = new object();
            object INVALID_DATA = null;

            Task<object> ValidUpdateDataFactory(DocumentAddress address, object data) =>
                Task.FromResult(VALID_DATA);

            Task<object> InvalidAddDataFactory(DocumentAddress address) =>
                Task.FromResult(INVALID_DATA);

            var res = await service
                .AddOrUpdateAsync(
                    address: KEY,
                    addDataAsync: InvalidAddDataFactory,
                    updateDataAsync: ValidUpdateDataFactory
                )
                .ConfigureAwait(false);

            Assert.IsFalse(res.Try(out var _, out Exception ex));
            Assert.IsInstanceOf<DocumentException>(ex);
        }


        [Test]
        public async Task AddOrUpdate_InvalidUpdateDataFactory_ReturnsError()
        {
            var service = GetService();

            string KEY = "KEY";
            object VALID_DATA = new object();
            object INVALID_DATA = null;

            Task<object> ValidAddDataFactory(DocumentAddress address) =>
                Task.FromResult(VALID_DATA);

            Task<object> InvalidUpdateDataFactory(DocumentAddress address, object data) =>
                Task.FromResult(INVALID_DATA);

            // Put data, such that updateData is stressed
            await service.PutAsync(KEY, VALID_DATA);

            var res = await
                service.AddOrUpdateAsync(
                    address: KEY,
                    addDataAsync: ValidAddDataFactory,
                    updateDataAsync: InvalidUpdateDataFactory
                );

            Assert.IsFalse(res.Try(out var _, out Exception ex));
            Assert.IsInstanceOf<DocumentException>(ex);
        }


        [Test]
        public async Task Put_And_Get_SameKey_DifferentTypes_NoCollision()
        {
            var service = GetService();

            string KEY = "KEY";
            var DATA_1 = new Box<int>(1);
            var DATA_2 = new Box<string>("D2");

            await service.PutAsync(KEY, DATA_1).ConfigureAwait(false);
            await service.PutAsync(KEY, DATA_2).ConfigureAwait(false);

            var res1 = await service.GetAsync<Box<int>>(KEY).ConfigureAwait(false);
            var res2 = await service.GetAsync<Box<string>>(KEY).ConfigureAwait(false);

            Assert.IsTrue(res1.Try(out Box<int> data1));
            Assert.AreEqual(DATA_1, data1);

            Assert.IsTrue(res2.Try(out Box<string> data2));
            Assert.AreEqual(DATA_2, data2);
        }
    }
}