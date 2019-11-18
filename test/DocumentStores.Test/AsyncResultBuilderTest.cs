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
        private class TestException : Exception { }


        [Test]
        public async Task SimplePipelineExecutesCorrectly()
        {
            static async Task<Unit> ThrowTestExceptionAsync()
            {
                await Task.Yield();
                throw new TestException();
            }

            static Func<Task<Unit>> GetSourceFunction() =>
                ThrowTestExceptionAsync;

            static bool ShouldCatch(Exception ex) => ex is TestException;

            static bool ShouldRetry(Exception ex) => ex is TestException;

            static Task<Result<string>> Map(object o) =>
                Task.FromResult(Result<string>.Ok(o.ToString()));

            await GetSourceFunction()
                .Catch(ShouldCatch)
                .RetryEquitemporal(TimeSpan.FromMilliseconds(1000), 5, ShouldRetry)
                .RetryIncrementally(TimeSpan.FromMilliseconds(50), 5, ShouldRetry)
                .Map(Map)
                .Do(TestContext.Progress.WriteLine, TestContext.Error.WriteLine)
                .Invoke();

            Assert.Pass();
        }
    }

}