using System.Collections.Generic;
;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    internal class DocumentRouteTest
    {
        [Test]
        public void StartsWith_Correct()
        {
            const string SEG_1 = "S1";
            const string SEG_2 = "S2";

            var route0 = DocumentRoute.Default;
            var route1 = DocumentRoute.Create(SEG_1);
            var route2 = DocumentRoute.Create(SEG_1, SEG_2);

            var expectedStartsWithTrueRelations =
                new List<(DocumentRoute, DocumentRoute)>(){
                (route0, route0),
                (route1, route0),
                (route1, route1),
                (route2, route0),
                (route2, route1)
            };

            var expectedStartsWithFalseRelations =
                new List<(DocumentRoute, DocumentRoute)>(){
                (route0, route1),
                (route0, route2),
                (route1, route2)
            };

            foreach (var rel in expectedStartsWithTrueRelations)
                Assert.IsTrue(rel.Item1.StartsWith(rel.Item2));

            foreach (var rel in expectedStartsWithFalseRelations)
                Assert.IsFalse(rel.Item1.StartsWith(rel.Item2));
        }

        [Test]
        public void Equals_Correct()
        {
            const string SEG_1 = "S1";
            const string SEG_2 = "S2";

            var route1 = DocumentRoute.Create(SEG_1);
            var route2 = DocumentRoute.Create(SEG_1);
            var route3 = DocumentRoute.Create(SEG_2);

            Assert.AreEqual(route1, route2);
            Assert.AreNotEqual(route1, route3);
        }
    }
}