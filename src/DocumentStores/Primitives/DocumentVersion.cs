#nullable enable


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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

            static IEnumerable<int> matchNumbers(string input)
            {
                const string MATCH_ONE_OR_MORE_DIGITS = "[0-9]{1,}";
                return Regex
                    .Matches(input, MATCH_ONE_OR_MORE_DIGITS)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .Select(int.Parse);
            }
            
            var n1 = matchNumbers(this.Value);
            var n2 = matchNumbers(other.Value);

            static IEnumerable<int> concatZeros(IEnumerable<int> source, int totalCount) =>
                source.Concat(Enumerable.Repeat(0, Math.Max(0, totalCount - source.Count())));

            return Enumerable.Zip(
                concatZeros(n1, n2.Count()),
                concatZeros(n2, n1.Count()),
                (t, o) => t - o
            ).FirstOrDefault(i => i != 0);
        }
    }
}