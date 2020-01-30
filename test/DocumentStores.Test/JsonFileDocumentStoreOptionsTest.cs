using System;
using System.IO;
using System.Linq;
using DocumentStores.Internal;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class JsonFileDocumentStoreOptionsTest
    {
        [Test]
        public void Test_DefaultOptions_PointToAppData()
        {
            var options = JsonFileDocumentStoreOptions.Default;

            Assert.That(
                options.RootDirectory.StartsWith(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                ),
                $"Unexpected root directory: '{options.RootDirectory}'"
            );
        }
    }
}