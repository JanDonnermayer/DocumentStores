using System;

#nullable enable

namespace DocumentStores.Primitives
{
    internal class ResultException : Exception
    {
        public ResultException(Exception innerException)
            : base("Operation failed.", innerException)
        {
        }

        public ResultException() : base()
        {
        }

        public ResultException(string message) : base(message)
        {
        }

        public ResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}