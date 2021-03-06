﻿using System;
using System.Runtime.Serialization;

#nullable enable

namespace DocumentStores
{
    internal class InvalidDocumentSearchOptionsException : ArgumentException
    {
        public InvalidDocumentSearchOptionsException(DocumentSearchOption options) : base("InvalidOptions: " + options.ToString()) { }

        public InvalidDocumentSearchOptionsException() { }

        public InvalidDocumentSearchOptionsException(string message) : base(message) { }

        public InvalidDocumentSearchOptionsException(string message, Exception innerException) : base(message, innerException) { }

        public InvalidDocumentSearchOptionsException(string message, string paramName) : base(message, paramName) { }

        public InvalidDocumentSearchOptionsException(string message, string paramName, Exception innerException) : base(message, paramName, innerException) { }

        protected InvalidDocumentSearchOptionsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

