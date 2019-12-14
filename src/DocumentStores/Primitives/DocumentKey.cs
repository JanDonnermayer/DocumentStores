#nullable enable


using System;
using System.Collections.Generic;

namespace DocumentStores.Primitives
{
    public readonly struct DocumentKey
    {
        public readonly string Value;

        internal DocumentKey(string value)
        {
            if (string.IsNullOrEmpty(value))  throw new ArgumentNullException(nameof(value)); 
            this.Value = value;
        }

        public static DocumentKey Create(string key) => new DocumentKey(key);

        public DocumentKey Map(Func<string, string> mapper) => Create(mapper(Value));
        

        public static implicit operator DocumentKey(string key) => new DocumentKey(key);

        public static implicit operator string(DocumentKey key) => key.Value;
    }

}