using System;

#nullable enable

namespace DocumentStores
{
    internal static class Function
    {
        /// <summary>
        /// Creates a delegate from the specifed <paramref name="source"/> with the specified arguments applied.
        /// </summary>
        public static Func<U> ApplyArgs<T1, T2, T3, U>(this Func<T1, T2, T3, U> source, T1 arg1, T2 arg2, T3 arg3) =>
           () => source(arg1, arg2, arg3);

        /// <summary>
        /// Creates a delegate from the specifed <paramref name="source"/> with the specified arguments applied.
        /// </summary>
        public static Func<U> ApplyArgs<T1, T2, U>(this Func<T1, T2, U> source, T1 arg1, T2 arg2) =>
           () => source(arg1, arg2);

        /// <summary>
        /// Creates a delegate from the specifed <paramref name="source"/> with the specified arguments applied.
        /// </summary>
        public static Func<U> ApplyArgs<T1, U>(this Func<T1, U> source, T1 arg1) =>
           () => source(arg1);
    }
}