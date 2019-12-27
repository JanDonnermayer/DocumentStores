#nullable enable

namespace DocumentStores.Primitives
{    
    internal class DocumentMissingException : DocumentException
    {
        public DocumentMissingException(DocumentAddress address) 
            : base($"_No such document: {address}")
        {
        }
    }

}