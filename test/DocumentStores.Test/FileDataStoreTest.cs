using System.IO;
using System.Linq;
using DocumentStores.Internal;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    internal class FileDataStoreTest
    {
        private FileDataStore store;

        private readonly string testDirectory =
            Path.Combine(
                Path.GetTempPath(),
                TestContext.CurrentContext.Test.ClassName
            );

        [SetUp]
        public void SetUp()
        {
            if (!Directory.Exists(testDirectory))
                Directory.CreateDirectory(testDirectory);
            store = new FileDataStore(testDirectory, ".bin");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, recursive: true);
        }

        [TestCase("k")]
        [TestCase("k.k")]
        [TestCase("k/k.k")]
        [TestCase("k/k.k/")]
        [TestCase("k/k.k/")]
        [TestCase("<|k|>")]
        public void Test_Write_Delete_CorrectState(string key)
        {
            var route = DocumentRoute.Create(TestContext.CurrentContext.Test.Name);
            var address = DocumentAddress.Create(route, key);
            var data = new byte[] { 1, 2, 3 };

            // act1
            using (var writeStream = store.GetWriteStream(address))
            {
                writeStream.Write(data);
            }

            var keys1 = store.GetAddresses(
                route: route,
                options: DocumentSearchOption.TopLevelOnly
            );
            var exists1 = store.Exists(address);

            // assert1
            Assert.IsTrue(exists1);
            Assert.IsTrue(keys1.Contains(address));

            // act2
            store.Clear();

            var keys2 = store.GetAddresses(
                route: route,
                options: DocumentSearchOption.TopLevelOnly
            );
            var exists2 = store.Exists(address);

            // assert2
            Assert.IsFalse(exists2);
            Assert.IsFalse(keys2.Contains(address));
        }

        [Test]
        public void Test_RoundTripData_Equals()
        {
            var route = DocumentRoute.Create(TestContext.CurrentContext.Test.Name);
            var address = DocumentAddress.Create(route, "k1");
            var data = new byte[] { 1, 2, 3 };

            using (var writeStream = store.GetWriteStream(address))
            {
                writeStream.Write(data);
            }

            var buffer = new byte[data.Length];
            using (var readStream = store.GetReadStream(address))
            {
                readStream.Read(buffer, 0, (int)readStream.Length);
            }

            Assert.AreEqual(buffer, data);
        }

        [Test]
        public void Test_Routing()
        {
            var routeRoot = DocumentRoute.Create(TestContext.CurrentContext.Test.Name);
            const string A = "<|.A.|>";
            var routeA = DocumentRoute.Create(A).Prepend(routeRoot);
            const string B = "<|.B.|>";
            var routeAB = DocumentRoute.Create(A, B).Prepend(routeRoot);

            var addressA1 = DocumentAddress.Create(routeA, "k1");
            var addressAB2 = DocumentAddress.Create(routeAB, "k2");

            var data = new byte[] { 1, 2, 3 };

            using (var writeStream = store.GetWriteStream(addressA1))
            {
                writeStream.Write(data);
            }

            using (var writeStream = store.GetWriteStream(addressAB2))
            {
                writeStream.Write(data);
            }

            // A
            var addressesA = store.GetAddresses(routeA, DocumentSearchOption.TopLevelOnly);
            // A/*
            var addressesAplus = store.GetAddresses(routeA, DocumentSearchOption.AllLevels);            
            // A/B
            var addressesAB = store.GetAddresses(routeAB, DocumentSearchOption.TopLevelOnly);

            Assert.AreEqual(
                new DocumentAddress[] { addressA1 },
                addressesA.ToArray()
            );

            Assert.AreEqual(
                new DocumentAddress[] { addressA1, addressAB2 },
                addressesAplus.ToArray()
            );

            Assert.AreEqual(
                new DocumentAddress[] { addressAB2 },
                addressesAB.ToArray()
            );
        }


    }
}