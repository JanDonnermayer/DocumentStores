using NUnit.Framework;
using DocumentStores.Internal;
using System.Linq;
using DocumentStores.Primitives;

namespace DocumentStores.Test
{
    [TestFixture]
    class FileDocumentRouterTest
    {

        [TestCase("A/B/C/", ExpectedResult = 3)]
        [TestCase("A/B/C", ExpectedResult = 2)]
        public int GetRoute_CorrectLength(string path)
        {
            var route = FileDocumentRouter.GetRoute(path);

            return route.Count();
        }

        [TestCase("A/B", ExpectedResult = "B")]
        [TestCase("/B", ExpectedResult = "B")]
        [TestCase("B", ExpectedResult = "B")]
        public string GetKey_CorrectValue(string path)
        {
            var key = FileDocumentRouter.GetKey(path);

            return key;
        }

        [Test]
        public void ValidRoute_RoundTripped_Equals()
        {
            var initialRoute = DocumentRoute.Create("A", "B", "C");
            var roundTrippedRoute = FileDocumentRouter.GetRoute(initialRoute.ToPath());

            Assert.AreEqual(initialRoute, roundTrippedRoute);
        }


    }

}
