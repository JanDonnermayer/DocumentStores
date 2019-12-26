#nullable enable


using System;

namespace DocumentStores.Primitives
{
    /// <InheritDoc/>
    public readonly struct DocumentKey
    {
        /// <InheritDoc/>
        public readonly string Value;

        private DocumentKey(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Null or empty value!", nameof(value));
            this.Value = value;
        }

        /// <InheritDoc/>
        public static DocumentKey Create(string value) => new DocumentKey(value);

        internal DocumentKey MapValue(Func<string, string> mapper) => Create(mapper(Value));

        #region  Override

        /// <InheritDoc/>
        public override string ToString() => Value;

        #endregion

        #region  Operators

        /// <InheritDoc/>
        public static implicit operator string(DocumentKey key) => key.Value;

        /// <InheritDoc/>
        public static implicit operator DocumentKey(string value) => Create(value);

        #endregion
    }

}