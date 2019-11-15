using System;

#nullable enable

namespace DocumentStores.Primitives
{
    /// <summary>
    /// Represents the result of a performed operation.
    /// </summary>
    public class Result
    {
        private Result(Exception? exception)
        {
            this.exception = exception;
        }

        private readonly Exception? exception;


        /// <summary>
        /// Tests the result for success.
        /// </summary>
        public bool Try() => Try(out _);

        /// <summary>
        /// Tests the result for success.
        /// If the result is not successful: Yields the contained <see cref="Exception"/> (otherwise <see cref="null"/>).
        /// </summary>
        public bool Try(out Exception? ex)
        {
            ex = this.exception;
            return exception == null;
        }

#nullable restore

        /// <summary>
        /// If the result is successful: Does nothing;
        /// else: throws an <see cref="ResultException"/> containing the underlying <see cref="Exception"/>
        /// </summary>
        public void PassOrThrow()
        {
            if (!this.Try(out var ex)) throw new ResultException(ex);
        }

#nullable enable

        internal static Result Ok() =>
            new Result(exception: null);

        internal static Result Error(Exception exception) =>
            new Result(exception: exception ?? throw new ArgumentNullException(nameof(exception)));

        public static implicit operator Result(Exception ex) => Error(ex);

    }

}