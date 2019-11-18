using System;

#nullable enable

namespace DocumentStores.Primitives
{
    public class Option<T> 
    {
        private readonly bool hasValue;
        private readonly T value;

        private Option(bool hasValue, T value)
        {
            this.hasValue = hasValue;
            this.value = value;
        }

        public bool IsSome(out T value)
        {
            value = this.value;
            return hasValue;
        }

        public static Option<T> Some(T value) => 
            new Option<T>(true, value ?? throw new ArgumentNullException(nameof(value)));

        public static Option<T> None() =>
            new Option<T>(false, default!);
    }

}