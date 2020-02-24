using System;
using System.Linq;
using DocumentStores.Internal;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    public class AesEncryptionOptionsTest
    {

        [TestCase("")]
        [TestCase("password")]
        public void Test_New_StringKey_DoesNotThrow(string key)
        {
            Assert.DoesNotThrow(
                () => new AesEncryptionOptions(key: key)
            );
        }

        [Test]
        public void Test_New_LongStringKey_DoesNotThrow()
        {
            var key = new string(Enumerable.Repeat((char)0, 1000).ToArray());
            Assert.DoesNotThrow(
                () => new AesEncryptionOptions(key: key)
            );
        }

        [Test]
        public void Test_New_NullStringKey_ThrowsArgumentNullException()
        {
            string key = null;
            Assert.Throws<ArgumentNullException>(
                () => new AesEncryptionOptions(key: key)
            );
        }

        [TestCase(null)]
        [TestCase(new byte[] { })]
        [TestCase(new byte[] { 1, 2, 3 })]
        public void Test_New_ByteKey_DoesNotThrow(byte[] key)
        {
            Assert.DoesNotThrow(
                () => new AesEncryptionOptions(key: key)
            );
        }

        [Test]
        public void Test_New_LongByteKey_DoesNotThrow()
        {
            var key = Enumerable.Repeat((byte)0, 1000);
            Assert.DoesNotThrow(
                () => new AesEncryptionOptions(key: key)
            );
        }

        [TestCase(null)]
        [TestCase(new byte[] { })]
        [TestCase(new byte[] { 1, 2, 3 })]
        public void Test_New_IV_DoesNotThrow(byte[] iV)
        {
            Assert.DoesNotThrow(
                () => new AesEncryptionOptions(iV: iV)
            );
        }

        [Test]
        public void Test_New_LongIV_ThrowsArgumentException()
        {
            var iV = Enumerable.Repeat((byte)0, 1000);
            Assert.DoesNotThrow(
                () => new AesEncryptionOptions(iV: iV)
            );
        }
    }
}