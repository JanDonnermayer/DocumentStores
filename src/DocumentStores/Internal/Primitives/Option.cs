using System;

#nullable enable

namespace DocumentStores
{
    /// <summary>
    /// A wrapper, that can contain a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class Option<T>
    {
        private readonly bool hasValue;
        private readonly T value;

        public Option(bool hasValue, T value)
        {
            this.hasValue = hasValue;
            this.value = value;
        }

        /// <summary>
        /// Returns, whether a value is contained.
        /// </summary>
        /// <param name="value">The contained value</param>
        /// <returns></returns>
        public bool IsSome(out T value)
        {
            value = this.value;
            return hasValue;
        }
    }

    internal static class Option
    {
        /// <inheritdoc/>
        public static Option<T> Some<T>(T value) =>
            new Option<T>(true, value ?? throw new ArgumentNullException(nameof(value)));

        /// <inheritdoc/>
        public static Option<T> None<T>() =>
            new Option<T>(false, default!);
    }
}