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
    }

}