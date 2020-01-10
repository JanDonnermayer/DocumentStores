#nullable enable

namespace DocumentStores.Primitives
{
    internal class DocumentMissingException : DocumentException
    {
        public DocumentMissingException(DocumentAddress address)
            : base($"_No such document: {address}")
        {
        }

        public DocumentMissingException(string message) : base(message)
        {
        }

        public DocumentMissingException() : base()
        {
        }

        public DocumentMissingException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }

}