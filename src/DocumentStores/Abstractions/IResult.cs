using System;

namespace DocumentStores
{
    /// <summary>
    /// Represents the result of a performed operation, 
    /// with return data of <typeparamref name="TData"/>,
    /// and possible exception of <typeparamref name="TException"/>.
    /// </summary>
    public interface IResult<TData, TException> where TData : class where TException : Exception
    {
        /// <summary>
        /// Returns whether this result is successfull.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// If the result is not successful: Returns the contained exception (otherwise null).
        /// </summary>
        TException? Exception { get; }

        /// <summary>
        /// If the result is successful: Returns the contained data (otherwise null).
        /// </summary>
        TData? Data { get; }
    }

    /// <summary>
    /// Represents the result of a performed operation, 
    /// with return data of a <typeparamref name="TData"/>,
    /// and possible Exception.
    /// </summary>
    public interface IResult<TData> : IResult<TData, Exception> where TData : class { }
}