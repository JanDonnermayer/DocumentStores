using System;
using NUnit.Framework;
using System.Threading.Tasks;
using DocumentStores.Internal;
using DocumentStores.Primitives;

namespace DocumentStores.Test
{
    [TestFixture]
    class AsyncResultBuilderTest
    {
        private class TestException : Exception
        {
            public TestException(int v)
            {
                this.V = v;
            }

            public int V { get; }
        }


        [Test]
        public async Task SimplePipelineExecutesCorrectly()
        {
            int mut_exceptionCount = 0;
            async Task<Unit> ThrowTestExceptionAsync()
            {
                await Task.Yield();
                throw new TestException(mut_exceptionCount ++);
            }

            Func<Task<Unit>> GetSourceFunction() =>
                ThrowTestExceptionAsync;

            static bool ShouldCatch(Exception ex) => ex is TestException;

            static bool ShouldRetry(Exception ex) => ex is TestException;

            static Task<Result<string>> Map(object o) =>
                Task.FromResult(Result<string>.Ok(o.ToString()));

            const int TRY_COUNT_EQ = 5;
            const int TRY_COUNT_INCR = 5;

            var res = await GetSourceFunction()
                .Catch(ShouldCatch)
                .RetryEquitemporal(TimeSpan.FromMilliseconds(50), TRY_COUNT_EQ, ShouldRetry)
                .RetryIncrementally(TimeSpan.FromMilliseconds(50), TRY_COUNT_INCR, ShouldRetry)
                .Map(Map)
                .Do(TestContext.Progress.WriteLine, TestContext.Error.WriteLine)
                .Invoke();

            Assert.That(!res.Try(out Exception ex));
            Assert.That(ex is TestException);
            Assert.AreEqual((TRY_COUNT_EQ + 1) * (TRY_COUNT_INCR + 1) - 1, ((TestException)ex).V);

            Assert.Pass();
        }
    }

}
