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
                TestContext.CurrentContext.Test.Name
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
            var route = DocumentRoute.Create("A", "B");
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
            var address = DocumentAddress.Create("k1");
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

    }
}