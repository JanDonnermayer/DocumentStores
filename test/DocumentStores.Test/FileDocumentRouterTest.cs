using NUnit.Framework;
using DocumentStores.Internal;
using System.Linq;
using DocumentStores.Primitives;
using System;
using System.IO;

namespace DocumentStores.Test
{

    [TestFixture]
    class FileDocumentRouterTest
    {

        private FileDocumentRouter GetDocumentRouter() =>
            new FileDocumentRouter();

        [TestCase(@"A\B\", ExpectedResult = 2)]
        [TestCase(@"A\B", ExpectedResult = 1)]
        [TestCase(@"A\", ExpectedResult = 1)]
        [TestCase("A/B/", ExpectedResult = 2)]
        [TestCase("A/B", ExpectedResult = 1)]
        [TestCase("A/", ExpectedResult = 1)]
        [TestCase("A", ExpectedResult = 1)]
        public int ValidPath_GetRoute_ReturnsCorrectLength(string path)
        {
            var route = FileDocumentRouterInternal.GetRoute(path);
            return route.Count();
        }

        [TestCase(@"\A")]
        [TestCase("/A")]
        [TestCase("")]
        [TestCase(null)]
        public void InvalidPath_GetRoute_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentException>(() =>
                FileDocumentRouterInternal.GetRoute(path));
        }


        [TestCase(@"A\B\", ExpectedResult = @"A\B\", IncludePlatform = "Win")]
        [TestCase(@"A\B", ExpectedResult = @"A\", IncludePlatform = "Win")]
        [TestCase(@"A\", ExpectedResult = @"A\", IncludePlatform = "Win")]
        [TestCase("A/B/", ExpectedResult = @"A\B\", IncludePlatform = "Win")]
        [TestCase("A/B", ExpectedResult = @"A\", IncludePlatform = "Win")]
        [TestCase("A/", ExpectedResult = @"A\", IncludePlatform = "Win")]
        [TestCase("A", ExpectedResult = @"A\", IncludePlatform = "Win")]
        [TestCase(@"A\B\", ExpectedResult = "A/B/", IncludePlatform = "Linux")]
        [TestCase(@"A\B", ExpectedResult = "A/", IncludePlatform = "Linux")]
        [TestCase(@"A\", ExpectedResult = "A/", IncludePlatform = "Linux")]
        [TestCase("A/B/", ExpectedResult = "A/B/", IncludePlatform = "Linux")]
        [TestCase("A/B", ExpectedResult = "A/", IncludePlatform = "Linux")]
        [TestCase("A/", ExpectedResult = "A/", IncludePlatform = "Linux")]
        [TestCase("A", ExpectedResult = "A/", IncludePlatform = "Linux")]
        public string ValidPath_GetRoute_ToPath_ReturnsCorrectValue(string path)
        {
            var route = FileDocumentRouterInternal.GetRoute(path);
            return route.ToPath();
        }


        [TestCase(@"\A", ExpectedResult = "A")]
        [TestCase("/A", ExpectedResult = "A")]
        [TestCase(@"A\B", ExpectedResult = "B")]
        [TestCase(@"A/B", ExpectedResult = "B")]
        [TestCase(@"A", ExpectedResult = "A")]
        [TestCase(@"A.A", ExpectedResult = "A.A")]
        public string ValidPath_GetKey_ReturnsCorrectValue(string path)
        {
            var key = FileDocumentRouterInternal.GetKey(path);
            return key;
        }


        [TestCase("")]
        [TestCase(null)]
        public void InvalidPath_GetKey_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentException>(() =>
                FileDocumentRouterInternal.GetKey(path));
        }


        [Test]
        public void InvalidPathCharKeys_Encode_Decode_IsEqual()
        {
            var values = Path.GetInvalidFileNameChars().Select(c => c.ToString());

            var keys = values.Select(DocumentKey.Create).ToArray();
            var roundTrippedKeys= keys.Select(k => k.Encode()).Select(k => k.Decode()).ToArray();

            Assert.AreEqual(keys, roundTrippedKeys);
        }

        
        [Test]
        public void InvalidPathCharRoutes_Encode_Decode_IsEqual()
        {
            var values = Path.GetInvalidFileNameChars().Select(c => c.ToString());
            
            var routes = values.Select(c => DocumentRoute.Create(c)).ToArray();
            var roundTrippedRoutes= routes.Select(k => k.Encode()).Select(k => k.Decode()).ToArray();

            Assert.AreEqual(routes, roundTrippedRoutes);
        }

    }

}
