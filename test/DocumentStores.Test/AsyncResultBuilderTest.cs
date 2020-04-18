using System;
using NUnit.Framework;
using System.Threading.Tasks;
using DocumentStores.Internal;

namespace DocumentStores.Test
{
    [TestFixture]
    internal class AsyncResultBuilderTest
    {
        private class TestException : Exception
        {
            public TestException(int count) =>
                this.Count = count;
            public int Count { get; }
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

            var tcsError = new TaskCompletionSource<Exception>();

            static bool ShouldCatch(Exception ex) => ex is TestException;

            static bool ShouldRetry(Exception ex) => ex is TestException;

            static Task<IResult<string>> Map(object o) =>
                Task.FromResult(Result.Ok(o.ToString()));

            const int TRY_COUNT_EQ = 2;
            const int TRY_COUNT_INCR = 2;

            var res = await GetSourceFunction()
                .Catch(ShouldCatch)
                .RetryEquitemporal(TimeSpan.FromMilliseconds(10), TRY_COUNT_EQ, ShouldRetry)
                .RetryIncrementally(TimeSpan.FromMilliseconds(10), TRY_COUNT_INCR, ShouldRetry)
                .Map(Map)
                .Do(
                    onOk: _ => { },
                    onError: tcsError.SetResult
                )
                .Invoke()
                .ConfigureAwait(false);

            var error = await tcsError.Task;

            Assert.IsInstanceOf<TestException>(error);
            Assert.IsFalse(res.Try(out Exception ex));
            Assert.IsInstanceOf<TestException>(ex);
            Assert.AreEqual(
                expected: ((TRY_COUNT_EQ + 1) * (TRY_COUNT_INCR + 1)) - 1,
                actual: ((TestException)ex).Count
            );

            Assert.Pass();
        }
    }
}
