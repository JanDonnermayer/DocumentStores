using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static DocumentStores.Result;

#nullable enable

namespace DocumentStores.Internal
{
    /// <summary>
    /// Provides methods for working with async results.
    /// </summary>
    [DebuggerStepThrough]
    internal static class AsyncResultBuilder
    {
        /// <summary>
        /// Executes the provided function within a try-catch-block,
        /// that catches exceptions for which the specified <paramref name="exceptionFilter"/> returns true.
        /// If an exception occurs, an error-result is returned, containg the exception.
        /// Else: an ok-result is returned, containing <typeparamref name="T"/> data.
        /// </summary>
        public static Func<Task<Result<T>>> Catch<T>(this Func<Task<T>> source,
            Func<Exception, bool> exceptionFilter) where T : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (exceptionFilter is null)
                throw new ArgumentNullException(nameof(exceptionFilter));

            async Task<Result<T>> GetResultAsync()
            {
                try
                {
                    var result = await source().ConfigureAwait(false);
                    return Ok(result);
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
        /// Else: Retries the operation within intervals provided by the specified <paramref name="retrySpanProviders"/>
        /// until the result is successful or the sequence is exhausted.
        /// </summary>
        public static Func<Task<Result<T>>> Retry<T>(
            this Func<Task<Result<T>>> source,
            IEnumerable<Func<Exception, Option<TimeSpan>>> retrySpanProviders) where T : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (retrySpanProviders is null)
                throw new ArgumentNullException(nameof(retrySpanProviders));

            async Task<Result<T>> GetResultAsync()
            {
                var mut_res = await source.Invoke().ConfigureAwait(false);
                if (mut_res.Try(out Exception? mut_ex)) return mut_res;

                foreach (var retrySpanProvider in retrySpanProviders)
                {
                    if (!retrySpanProvider(mut_ex!).IsSome(out var span)) return mut_res;
                    await Task.Delay(span).ConfigureAwait(false);
                    mut_res = await source.Invoke().ConfigureAwait(false);
                    if (mut_res.Try(out mut_ex)) return mut_res;
                }

                return mut_res;
            }

            return GetResultAsync;
        }

        /// <summary>
        /// If the specified async result is successful, returns it.
        /// Else: Retries the operation within increasing intervals of length <paramref name="frequencySeed"/> * 2^[tryCount],
        /// until the result is successful or <paramref name="count"/> is reached.
        /// </summary>
        public static Func<Task<Result<T>>> RetryIncrementally<T>(
            this Func<Task<Result<T>>> producer,
            TimeSpan frequencySeed, uint count, Func<Exception, bool> exceptionFilter) where T : class =>
                producer.Retry(GetIncrementalTimeSpans(frequencySeed, count, exceptionFilter));

        /// <summary>
        /// If the specified async result is successful, returns it.
        /// Else: Retries the operation within constant intervals of length <paramref name="frequency"/>,
        /// until the result is successful or <paramref name="count"/> is reached.
        /// </summary>
        public static Func<Task<Result<T>>> RetryEquitemporal<T>(
            this Func<Task<Result<T>>> producer,
            TimeSpan frequency, uint count, Func<Exception, bool> exceptionFilter) where T : class =>
                producer.Retry(GetConstantTimeSpans(frequency, count, exceptionFilter));

        /// <summary>
        /// If the specified async result is successful, passes the specified <paramref name="continuation"/>;
        /// Else: Returns a result containg the Error.
        /// </summary>
        public static Func<Task<Result<V>>> Map<T, V>(
            this Func<Task<Result<T>>> source,
            Func<T, Task<Result<V>>> continuation) where T : class where V : class

        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (continuation is null)
                throw new ArgumentNullException(nameof(continuation));

            async Task<Result<V>> GetResultAsync()
            {
                if (!(await source().ConfigureAwait(false)).Try(out var val, out var ex)) return ex!;
                return await continuation(val!).ConfigureAwait(false);
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
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (onOk is null)
                throw new ArgumentNullException(nameof(onOk));
            if (onError is null)
                throw new ArgumentNullException(nameof(onError));

            async Task<Result<T>> GetResultAsync()
            {
                var res = await source().ConfigureAwait(false);
                if (res.Try(out var val, out var ex))
                    onOk(val!);
                else
                    onError(ex!);
                return res;
            }

            return GetResultAsync;
        }

        public static Func<Task<Result<T>>> Init<T>(
            this Func<Task<T>> source,
            Func<Func<Task<T>>, Func<Task<Result<T>>>> handler)  where T : class
        {
            return handler(source);
        }

        public static Func<Task<Result<T>>> Pipe<T>(
            this Func<Task<Result<T>>> source,
            Func<Func<Task<Result<T>>>, Func<Task<Result<T>>>> handler)  where T : class
        {
            return handler(source);
        }

        #region Private

        private static IEnumerable<Func<Exception, Option<TimeSpan>>> GetConstantTimeSpans(
            TimeSpan seed, uint count, Func<Exception, bool> exceptionFilter) =>
            Enumerable
                .Range(0, (int)count)
                .Select(_ => seed)
                .Select(t => new Func<Exception, Option<TimeSpan>>((Exception ex) =>
                   (exceptionFilter ?? throw new ArgumentNullException(nameof(exceptionFilter))).Invoke(ex)
                    ? Option.Some(t)
                    : Option.None<TimeSpan>()));

        private static IEnumerable<Func<Exception, Option<TimeSpan>>> GetIncrementalTimeSpans(
            TimeSpan seed, uint count, Func<Exception, bool> exceptionFilter) =>
            Enumerable
                .Range(0, (int)count)
                .Select(i => TimeSpan.FromMilliseconds(seed.TotalMilliseconds * Math.Pow(2, i)))
                .Select(t => new Func<Exception, Option<TimeSpan>>(ex =>
                    (exceptionFilter ?? throw new ArgumentNullException(nameof(exceptionFilter))).Invoke(ex)
                    ? Option.Some(t)
                    : Option.None<TimeSpan>()));

        #endregion
    }
}

