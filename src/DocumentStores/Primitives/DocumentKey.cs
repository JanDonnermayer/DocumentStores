#nullable enable


using System;

namespace DocumentStores.Primitives
{
    public readonly struct DocumentKey
    {
        public readonly string Value;

        private DocumentKey(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Null or empty value!", nameof(value));
            this.Value = value;
        }

        public static DocumentKey Create(string value) => new DocumentKey(value);

        internal DocumentKey MapValue(Func<string, string> mapper) => Create(mapper(Value));

        #region  Override

        public override string ToString() => Value;

        #endregion

        #region  Operators

        public static implicit operator string(DocumentKey key) => key.Value;

        public static implicit operator DocumentKey(string value) => Create(value);

        #endregion
    }

}