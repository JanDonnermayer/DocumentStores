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
        /// If an exception occurs, an error-result is returned, containing the exception.
        /// Else: an ok-result is returned, containing <typeparamref name="T"/> data.
        /// </summary>
        public static Func<Task<IResult<T>>> Catch<T>(this Func<Task<T>> source,
            Func<Exception, bool> exceptionFilter) where T : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (exceptionFilter is null)
                throw new ArgumentNullException(nameof(exceptionFilter));

            async Task<IResult<T>> GetResultAsync()
            {
                try
                {
                    var result = await source().ConfigureAwait(false);
                    return Ok(result);
                }
                catch (Exception ex) when (exceptionFilter(ex))
                {
                    return Error<T>(ex);
                }
            }

            return GetResultAsync;
        }

        /// <summary>
        /// Executes the provided function within a try-catch-block,
        /// that catches exceptions of type TException.
        /// If an exception occurs, an error-result is returned, containing the exception.
        /// Else: an ok-result is returned, containing <typeparamref name="T"/> data.
        /// </summary>
        public static Func<Task<IResult<T>>> Catch<T, TException>(this Func<Task<T>> source) where T : class =>
           Catch(source, ex => ex.GetType() == typeof(TException));

        /// <summary>
        /// Executes the provided function within a try-catch-block.
        /// If an exception occurs, an error-result is returned, containing the exception.
        /// Else: an ok-result is returned, containing <typeparamref name="T"/> data.
        /// </summary>
        public static Func<Task<IResult<T>>> Catch<T>(this Func<Task<T>> source) where T : class =>
           Catch(source, _ => true);

        /// <summary>
        /// Executes the provided action within a try-catch-block,
        /// that catches exceptions for which the specified <paramref name="exceptionFilter"/> returns true.
        /// If an exception occurs, an error-result is returned, containing the exception.
        /// Else: an ok-result is returned.
        /// </summary>
        public static Func<Task<IResult<Unit>>> Catch(this Func<Task> source,
            Func<Exception, bool> exceptionFilter)
            => Catch(() => source().ContinueWith(_ => Unit.Default, TaskScheduler.Default), exceptionFilter);

        /// <summary>
        /// Executes the provided function within a try-catch-block,
        /// that catches exceptions of type TException.
        /// If an exception occurs, an error-result is returned, containing the exception.
        /// Else: an ok-result is returned.
        /// </summary>
        public static Func<Task<IResult<Unit>>> Catch<TException>(this Func<Task> source) =>
           Catch(source, ex => ex.GetType() == typeof(TException));

        /// <summary>
        /// Executes the provided function within a try-catch-block.
        /// If an exception occurs, an error-result is returned, containing the exception.
        /// Else: an ok-result is returned.
        /// </summary>
        public static Func<Task<IResult<Unit>>> Catch(this Func<Task> source) =>
           Catch(source, _ => true);

        /// <summary>
        /// If the specified async result is successful, returns it.
        /// Else: Retries the operation within intervals provided by the specified <paramref name="retrySpanProviders"/>
        /// until the result is successful or the sequence is exhausted.
        /// </summary>
        public static Func<Task<IResult<T>>> Retry<T>(
            this Func<Task<IResult<T>>> source,
            IEnumerable<Func<Exception, Option<TimeSpan>>> retrySpanProviders) where T : class
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (retrySpanProviders is null)
                throw new ArgumentNullException(nameof(retrySpanProviders));

            async Task<IResult<T>> GetResultAsync()
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
        public static Func<Task<IResult<T>>> RetryIncrementally<T>(
            this Func<Task<IResult<T>>> producer,
            TimeSpan frequencySeed, uint count, Func<Exception, bool> exceptionFilter) where T : class =>
                producer.Retry(GetIncrementalTimeSpans(frequencySeed, count, exceptionFilter));

        /// <summary>
        /// If the specified async result is successful, returns it.
        /// Else: Retries the operation within constant intervals of length <paramref name="frequency"/>,
        /// until the result is successful or <paramref name="count"/> is reached.
        /// </summary>
        public static Func<Task<IResult<T>>> RetryEquitemporal<T>(
            this Func<Task<IResult<T>>> producer,
            TimeSpan frequency, uint count, Func<Exception, bool> exceptionFilter) where T : class =>
                producer.Retry(GetConstantTimeSpans(frequency, count, exceptionFilter));

        /// <summary>
        /// If the specified async result is successful, passes the specified <paramref name="dataMapper"/>;
        /// Else: Returns a result containing the Error.
        /// </summary>
        public static Func<Task<IResult<V>>> Map<T, V>(
            this Func<Task<IResult<T>>> source,
            Func<T, Task<IResult<V>>> dataMapper) where T : class where V : class

        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (dataMapper is null)
                throw new ArgumentNullException(nameof(dataMapper));

            async Task<IResult<V>> GetResultAsync()
            {
                var res = await source().ConfigureAwait(false);
                if (res.Try(out var val, out var ex))
                    return await dataMapper(val!).ConfigureAwait(false);
                else
                    return Error<V>(ex!);
            }

            return GetResultAsync;
        }

        /// <summary>
        /// Returns the specified async result.
        /// If the async result is successful, invokes <paramref name="onOk"/>.
        /// Else: Invokes <paramref name="onError"/>
        /// </summary>
        public static Func<Task<IResult<T>>> Do<T>(
            this Func<Task<IResult<T>>> source,
            Action<T> onOk,
            Action<Exception> onError) where T : class

        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (onOk is null)
                throw new ArgumentNullException(nameof(onOk));
            if (onError is null)
                throw new ArgumentNullException(nameof(onError));

            async Task<IResult<T>> GetResultAsync()
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

        public static Func<Task<IResult<T>>> Init<T>(
            this Func<Task<T>> source,
            Func<Func<Task<T>>, Func<Task<IResult<T>>>> handler) where T : class
        {
            return handler(source);
        }

        public static Func<Task<IResult<T>>> Pipe<T>(
            this Func<Task<IResult<T>>> source,
            Func<Func<Task<IResult<T>>>, Func<Task<IResult<T>>>> handler) where T : class
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

