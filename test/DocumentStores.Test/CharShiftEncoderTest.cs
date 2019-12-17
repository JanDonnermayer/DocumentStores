using NUnit.Framework;
using DocumentStores.Internal;
using System.Linq;
using DocumentStores.Primitives;
using System.IO;

namespace DocumentStores.Test
{
    [TestFixture]
    class CharShiftEncoderTest
    {
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
