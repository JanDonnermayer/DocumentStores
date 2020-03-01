using System;

#nullable enable

namespace DocumentStores
{
    /// <summary>
    /// The Exception that is thrown when serialization failed.
    /// </summary>
    public class SerializationException : Exception
    {
        /// <inheritdoc/>
        public SerializationException(string message) : base(message)
        {
        }

        /// <inheritdoc/>
        public SerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        public SerializationException()
        {
        }
    }
}

