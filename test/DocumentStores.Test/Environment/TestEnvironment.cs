using System.IO;
using NUnit.Framework;
using System;

namespace DocumentStores.Test
{
    internal static class TestEnvironment
    {
        public static String GetRootTestDir() =>
            Path.Combine(
                Path.GetTempPath(),
                TestContext.CurrentContext.Test.ClassName
            );

        public static string GetTestDir() =>
            Path.Combine(
                GetRootTestDir(),
                TestContext.CurrentContext.Test.Name,
                Guid.NewGuid().ToString()
            );

        public static JsonFileDocumentStore GetService() =>
            new JsonFileDocumentStore(GetTestDir());
    }
}