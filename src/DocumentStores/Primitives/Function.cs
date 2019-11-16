using System;

#nullable enable

namespace DocumentStores.Primitives
{
    static class Function
    {
        public static Func<U> Apply<T1, T2, T3, U>(this Func<T1, T2, T3, U> source, T1 arg1, T2 arg2, T3 arg3) =>
           () => source(arg1, arg2, arg3);

        public static Func<U> Apply<T1, T2, U>(this Func<T1, T2, U> source, T1 arg1, T2 arg2) =>
           () => source(arg1, arg2);

        public static Func<U> Apply<T1, U>(this Func<T1, U> source, T1 arg1) =>
           () => source(arg1);
    }


}