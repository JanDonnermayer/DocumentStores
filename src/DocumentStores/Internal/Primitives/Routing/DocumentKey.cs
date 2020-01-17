#nullable enable

using System;
using System.Collections.Generic;

namespace DocumentStores
{
    /// <InheritDoc/>
    public readonly struct DocumentKey : IEquatable<DocumentKey>
    {
        /// <InheritDoc/>
        public string Value { get; }

        private DocumentKey(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Null or empty value!", nameof(value));
            this.Value = value;
        }

        /// <InheritDoc/>
        public static DocumentKey FromString(string value) => new DocumentKey(value);

       /// <InheritDoc/>
        public static DocumentKey Create(string value) => FromString(value);

        internal DocumentKey MapValue(Func<string, string> mapper) => FromString(mapper(Value));

        #region  Override

        /// <InheritDoc/>
        public override string ToString() => Value;

        /// <InheritDoc/>
        public override bool Equals(object? obj) =>
            obj is DocumentKey key && Equals(key);

        /// <InheritDoc/>
        public bool Equals(DocumentKey other) =>
            Value == other.Value;

        /// <InheritDoc/>
        public override int GetHashCode() =>
            -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);

        #endregion

        #region  Operators

        /// <InheritDoc/>
        public static implicit operator string(DocumentKey key) => key.Value;

        /// <InheritDoc/>
        public static implicit operator DocumentKey(string value) => FromString(value);

        /// <InheritDoc/>
        public static bool operator ==(DocumentKey left, DocumentKey right) => left.Equals(right);

        /// <InheritDoc/>
        public static bool operator !=(DocumentKey left, DocumentKey right) => !(left == right);

        #endregion
    }

}