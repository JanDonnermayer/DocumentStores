using System;

#nullable enable

namespace DocumentStores.Primitives
{
    internal class DocumentException : Exception
    {
        public DocumentException(string message) : base(message)
        {
        }
    }

}