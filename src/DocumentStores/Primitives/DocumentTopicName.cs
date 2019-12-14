#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DocumentStores.Primitives
{

    public readonly struct DocumentTopicName
    {
        private readonly ImmutableArray<string> segments;

        private DocumentTopicName(IEnumerable<string> segments) =>
            this.segments = (segments ?? throw new ArgumentNullException(nameof(segments)))
                .ToImmutableArray();

        public static DocumentTopicName Default => Create(Enumerable.Empty<string>());

        public static DocumentTopicName Create(IEnumerable<string> segments) =>
            new DocumentTopicName(segments);

        public static DocumentTopicName Create(params string[] segments) =>
            new DocumentTopicName(segments);


        public DocumentTopicName Prepend(DocumentTopicName topic) =>
            new DocumentTopicName(topic.segments.Concat(this.segments));

        public DocumentTopicName Append(DocumentTopicName topic) =>
            new DocumentTopicName(this.segments.Concat(topic.segments));

        public DocumentTopicName Select(Func<string, string> mapper) =>
            new DocumentTopicName(this.segments.Select(mapper));

        public DocumentKey CreateKey(string key) =>
            new DocumentKey(this, key);


            
        #region "Equals & GetHashCode"

        public override bool Equals(object? obj)
        {
            return obj is DocumentTopicName name &&
                Enumerable.SequenceEqual(segments, name.segments);
        }

        public override int GetHashCode()
        {
            return -312155673 + segments
                .Select(s => s.GetHashCode())
                .Aggregate((x,y) => x ^ y);
        }

        #endregion

        public static implicit operator DocumentTopicName(string name) => Create(name);

    }







}