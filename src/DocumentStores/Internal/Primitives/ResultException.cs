using System;

#nullable enable

namespace DocumentStores
{
    /// <summary>
    /// The exception that is thrown when a result is not successful.
    /// </summary>
    public sealed class ResultException : Exception
    {
        ///<InheritDoc />
        public ResultException(Exception innerException)
            : base("Operation failed.", innerException)
        {
        }

        ///<InheritDoc />
        public ResultException() : base()
        {
        }

        ///<InheritDoc />
        public ResultException(string message) : base(message)
        {
        }

        ///<InheritDoc />
        public ResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}