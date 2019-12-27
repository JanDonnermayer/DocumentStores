using DocumentStores.Primitives;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class DocumentAddressTest
    {

        [Test]
        public void Equals_Correct()
        {
            string SEG= "S1";
            string KEY_1 = "K1";
            string KEY_2 = "K2";

            var address1 = DocumentAddress.Create(SEG, KEY_1);
            var address2 = DocumentAddress.Create(SEG, KEY_2);

            Assert.AreEqual(address1, address1);
            Assert.AreNotEqual(address1, address2);
        }
    }
}