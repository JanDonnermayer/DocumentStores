using System;

#nullable enable

namespace DocumentStores.Primitives
{
    /// <inheritdoc />
    public class DocumentException : Exception
    {
        /// <inheritdoc />
        public DocumentException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public DocumentException() : base()
        {
        }

        /// <inheritdoc />
        public DocumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

