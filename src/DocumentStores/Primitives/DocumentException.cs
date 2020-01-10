using System;

#nullable enable

namespace DocumentStores.Primitives
{
    internal class DocumentException : Exception
    {
        public DocumentException(string message) : base(message)
        {
        }

        public DocumentException() : base()
        {
        }

        public DocumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}