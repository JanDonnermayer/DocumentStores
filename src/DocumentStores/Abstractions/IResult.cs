using System;

namespace DocumentStores
{
    /// <summary>
    /// Represents the result of a performed operation, with custom return data.
    /// </summary>
    public interface IResult<TData> where TData : class
    {
        /// <summary>
        /// Returns whether this result is successfull.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// If the result is not successful: Returns the contained exception (otherwise null).
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        /// If the result is successful: Returns the contained data (otherwise null).
        /// </summary>
        TData? Data { get; }
    }
}