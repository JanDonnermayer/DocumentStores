#nullable enable


using System;

namespace DocumentStores.Primitives
{
    public readonly partial struct DocumentKey
    {
        public readonly string Value;

        internal DocumentKey(string value)
        {
            if (string.IsNullOrEmpty(value))  throw new ArgumentNullException(nameof(value)); 
            this.Value = value;
        }

        public static DocumentKey Create(string value) => new DocumentKey(value);

        internal DocumentKey MapValue(Func<string, string> mapper) => Create(mapper(Value));
        
        #region  Override

        public override string ToString() => Value;

        #endregion

        #region  Operators

        public static implicit operator string(DocumentKey key) => key.Value;

        public static implicit operator DocumentKey(string value) => new DocumentKey(value);

        #endregion
    }

}