using DocumentStores.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace DocumentStores
{
    /// <summary>
    /// Build results over async functions using try-catch-blocks.
    /// Allows to retry functions according to retry policies.
    /// </summary>
    static class AsyncResultBuilder
    {
        public static Func<Task<Result<Unit>>> WithTryCatch(this Func<Task> source,
            Func<Exception, bool> exceptionFilter) =>
            WithTryCatch(async () => { await source(); return Unit.Default; }, exceptionFilter);

        public static Func<Task<Result<Unit>>> WithTryCatch(this Func<Task> source) =>
            WithTryCatch(async () => { await source(); return Unit.Default; }, _ => true);

        public static Func<Task<Result<T>>> WithTryCatch<T>(this Func<Task<T>> source) where T : class =>
            WithTryCatch(source, _ => true);

        public static Func<Task<Result<T>>> WithTryCatch<T>(this Func<Task<T>> source,
            Func<Exception, bool> exceptionFilter) where T : class
        {
            async Task<Result<T>> GetResultAsync()
            {
                try
                {
                    var result = await source();
                    return Result<T>.Ok(result);
                }
                catch (Exception _) when (exceptionFilter(_))
                {
                    return _;
                }
            }

            return GetResultAsync;
        }

        public static Func<Task<Result<T>>> WithRetryBehaviour<T>(
            this Func<Task<Result<T>>> source,
            IEnumerable<TimeSpan> retrySpansProvider) where T : class
        {
            async Task<Result<T>> GetResultAsync()
            {
                var res = await source.Invoke();
                if (res.Try()) return res;

                foreach (var retrySpan in retrySpansProvider)
                {
                    var nextRes = await source.Invoke();
                    if (nextRes.Try()) return nextRes;
                    await Task.Delay(retrySpan);
                }

                return res;
            }

            return GetResultAsync;
        }

        public static Func<Task<Result<T>>> WithIncrementalRetryBehaviour<T>(
            this Func<Task<Result<T>>> producer,
            TimeSpan frequencySeed, uint count) where T : class =>
                WithRetryBehaviour(producer, GetIncrementalTimeSpans(frequencySeed, count));

        public static Func<Task<Result<T>>> WithEquitemporalRetryBehaviour<T>(
            this Func<Task<Result<T>>> producer,
            TimeSpan frequency, uint count) where T : class =>
                WithRetryBehaviour(producer, GetConstantTimeSpans(frequency, count));

        public static Func<Task<Result<T>>> Map<V, T>(
            this Func<Task<Result<V>>> source,
            Func<V, Task<Result<T>>> continuation) where V : class where T : class

        {
            async Task<Result<T>> GetResultAsync()
            {
                if (!(await source()).Try(out var val, out var ex)) return ex!;
                return await continuation(val!);
            }

            return GetResultAsync;
        }

        public static Func<Task<Result<T>>> Do<T>(
            this Func<Task<Result<T>>> source,
            Action<T> onOk,
            Action<Exception> onError) where T : class

        {
            async Task<Result<T>> GetResultAsync()
            {
                var res = await source();
                if (res.Try(out var val, out var ex))
                    onOk(val!);
                else
                    onError(ex!);
                return res;
            }

            return GetResultAsync;
        }

        #region Private
        
        private static IEnumerable<TimeSpan> GetConstantTimeSpans(TimeSpan seed, uint count) =>
            Enumerable
                .Range(0, (int)count)
                .Select(_ => seed);

        private static IEnumerable<TimeSpan> GetIncrementalTimeSpans(TimeSpan seed, uint count) =>
            Enumerable
            .Range(0, (int)count)
            .Select(i => TimeSpan.FromMilliseconds(seed.TotalMilliseconds * Math.Pow(2, i)));

        #endregion
    }
}