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

        [TestCase(@"A\B\", ExpectedResult = 2)]
        [TestCase(@"A\B", ExpectedResult = 1)]
        [TestCase(@"A\", ExpectedResult = 1)]
        [TestCase("A/B/", ExpectedResult = 2)]
        [TestCase("A/B", ExpectedResult = 1)]
        [TestCase("A/", ExpectedResult = 1)]
        [TestCase("A", ExpectedResult = 1)]
        public int ValidPath_GetRoute_ReturnsCorrectLength(string path)
        {
            var route = FileDocumentRouter.GetRoute(path);
            return route.Count();
        }

        [TestCase(@"\A")] 
        [TestCase("/A")]  
        [TestCase("")]
        [TestCase(null)]
        public void InvalidPath_GetRoute_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentException>(() => 
                FileDocumentRouter.GetRoute(path));
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
            var route = FileDocumentRouter.GetRoute(path);
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
            var key = FileDocumentRouter.GetKey(path);
            return key;
        }


        [TestCase("")]
        [TestCase(null)]
        public void InvalidPath_GetKey_ThrowsArgumentException(string path)
        {
            Assert.Throws<ArgumentException>(() => 
                FileDocumentRouter.GetKey(path));
        }

    }

}
