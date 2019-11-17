using DocumentStores.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace DocumentStores
{
    /// <summary>
    /// Provides methods for working with async results,
    /// that is <see cref="Task{Result{T}}"/>
    /// </summary>
    static class AsyncResultBuilder
    {
        /// <summary>
        /// Executes the provided function within a try-catch-block,
        /// that catches exceptions for which the specified <paramref name="exceptionFilter"/> returns true.
        /// If an exception occurs, an error-result is returned, containg the exception.
        /// Else: an ok-result is returned.
        /// </summary>
        public static Func<Task<Result<Unit>>> WithTryCatch(this Func<Task> source,
            Func<Exception, bool> exceptionFilter) =>
            WithTryCatch(async () => { await source(); return Unit.Default; }, exceptionFilter);

        /// <summary>
        /// Executes the provided function within a try-catch-block,
        /// that catches exceptions.
        /// If an exception occurs, an error-result is returned, containg the exception.
        /// Else: an ok-result is returned.
        /// </summary>
        public static Func<Task<Result<Unit>>> WithTryCatch(this Func<Task> source) =>
            WithTryCatch(async () => { await source(); return Unit.Default; }, _ => true);

        /// <summary>
        /// Executes the provided function within a try-catch-block,
        /// that catches exceptions.
        /// If an exception occurs, an error-result is returned, containg the exception.
        /// Else: an ok-result is returned, containing <typeparamref name="T"/> data.
        /// </summary>
        public static Func<Task<Result<T>>> WithTryCatch<T>(this Func<Task<T>> source) where T : class =>
            WithTryCatch(source, _ => true);

        /// <summary>
        /// Executes the provided function within a try-catch-block,
        /// that catches exceptions for which the specified <paramref name="exceptionFilter"/> returns true.
        /// If an exception occurs, an error-result is returned, containg the exception.
        /// Else: an ok-result is returned, containing <typeparamref name="T"/> data.
        /// </summary>
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

        /// <summary>
        /// If the specified async result is successful, returns it.
        /// Else: Retries the operation within intervals prvided by the specified <paramref name="retrySpansProvider"/>
        /// until the result is successful or the sequence is exhausted.
        /// </summary>
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

        /// <summary>
        /// If the specified async result is successful, returns it.
        /// Else: Retries the operation within increasing intervals of length <paramref name="frequency"/> * 2^[tryCount],
        /// until the result is successful or <paramref name="count"/> is reached.
        /// </summary>
        public static Func<Task<Result<T>>> WithIncrementalRetryBehaviour<T>(
            this Func<Task<Result<T>>> producer,
            TimeSpan frequencySeed, uint count) where T : class =>
                producer.WithRetryBehaviour(GetIncrementalTimeSpans(frequencySeed, count));

        /// <summary>
        /// If the specified async result is successful, returns it.
        /// Else: Retries the operation within constant intervals of length <paramref name="frequency"/>,
        /// until the result is successful or <paramref name="count"/> is reached.
        /// </summary>
        public static Func<Task<Result<T>>> WithEquitemporalRetryBehaviour<T>(
            this Func<Task<Result<T>>> producer,
            TimeSpan frequency, uint count) where T : class =>
                producer.WithRetryBehaviour(GetConstantTimeSpans(frequency, count));

        /// <summary>
        /// If the specified async result is successful, passes the specified <paramref name="continuation"/>;
        /// Else: Returns a result containg the Error.
        /// </summary>
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

        /// <summary>
        /// Returns the specified async result.
        /// If the async result is successful, invokes <paramref name="onOk"/>.
        /// Else: Invokes <paramref name="onError"/>
        /// </summary>
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