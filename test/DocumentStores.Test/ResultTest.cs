using System;
using NUnit.Framework;

namespace DocumentStores.Test
{
    [TestFixture]
    internal class ResultTest
    {
        [Test]
        public void Test_GetData()
        {
            var data = "dat";
            var result = Result<string>.Ok(data);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(data, result.Data);
        }

        [Test]
        public void Test_GetException()
        {
            var ex = new Exception();
            var result = Result<string>.Error(ex);

            Assert.False(result.Success);
            Assert.AreEqual(ex, result.Exception);
        }

        [Test]
        public void Test_Deconstruct_Data()
        {
            var expectedData = "dat";
            var result = Result<string>.Ok(expectedData);

            result.Deconstruct(out var actualData, out var _);

            Assert.AreEqual(expectedData, actualData);
        }

        [Test]
        public void Test_Deconstruct_Exception()
        {
            var expectedEx = new Exception();
            var result = Result<string>.Error(expectedEx);

            result.Deconstruct(out var _, out var actualEx);

            Assert.AreEqual(expectedEx, actualEx);
        }

        [Test]
        public void Test_Try_Data()
        {
            var expectedData = "dat";
            var result = Result<string>.Ok(expectedData);

            var success = result.Try(out var actualData, out var _);

            Assert.IsTrue(success);
            Assert.AreEqual(expectedData, actualData);
        }

        [Test]
        public void Test_Try_Exception()
        {
            var expectedEx = new Exception();
            var result = Result<string>.Error(expectedEx);

            var success = result.Try(out var _, out var actualEx);

            Assert.IsFalse(success);
            Assert.AreEqual(expectedEx, actualEx);
        }
    }
}