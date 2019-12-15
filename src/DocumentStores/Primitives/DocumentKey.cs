#nullable enable


using System;

namespace DocumentStores.Primitives
{
    public readonly struct DocumentKey 
    {
        public readonly string Value;
        public readonly DocumentVersion Version;

        private DocumentKey(string value, DocumentVersion version)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value)); 
            this.Value = value;
            this.Version = version;
        }

        public static DocumentKey Create(string value, DocumentVersion version) => 
            new DocumentKey(value, version);

        public static DocumentKey Create(string value) => 
            Create(value, DocumentVersion.Default);

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