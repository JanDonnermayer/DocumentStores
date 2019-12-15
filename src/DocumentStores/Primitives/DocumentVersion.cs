#nullable enable


using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentStores.Primitives
{
    public readonly struct DocumentVersion : IComparable<DocumentVersion>
    {
        private const string DEFAULT_VALUE = "*";

        public readonly string Value;

        private DocumentVersion(string value) => 
            Value = value ?? throw new ArgumentNullException(nameof(value));

        public static DocumentVersion Create(string value) =>
            new DocumentVersion(value);

        public static DocumentVersion Default =>
            new DocumentVersion(DEFAULT_VALUE);

        public int CompareTo(DocumentVersion other)
        {
            if (this.Equals(Default)) return 0;
            if (other.Equals(Default)) return 0;
            if (this.Equals(other)) return 0;     

            return Enumerable.Zip(
                this.Value.PadRight(other.Value.Length, '0').Where(char.IsDigit),
                other.Value.PadRight(this.Value.Length, '0').Where(char.IsDigit),
                (t, o) => t - o
            ).FirstOrDefault(i => i != 0);
        }
    }
}