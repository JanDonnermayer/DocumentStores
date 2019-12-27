using System.Collections.Generic;
using DocumentStores.Primitives;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    class DocumentRouteTest
    {

        [Test]
        public void StartsWith_Correct()
        {
            string SEG_1 = "S1";
            string SEG_2 = "S2";

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
    }
}