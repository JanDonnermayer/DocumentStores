#nullable enable


using System;
using System.Collections.Generic;

namespace DocumentStores.Primitives
{
    public readonly struct DocumentKey
    {
        internal readonly DocumentTopicName Topic;

        public readonly string Value;

        internal DocumentKey(DocumentTopicName topic, string value)
        {
            if (string.IsNullOrEmpty(value))  throw new ArgumentNullException(nameof(value));
  
            this.Topic = topic;
            this.Value = value;
        }

        internal DocumentKey(string key) : this(DocumentTopicName.Default, key) { }

        public static DocumentKey Create(string key) => new DocumentKey(key);


        #region "Equals & GetHashCode"

        public override bool Equals(object? obj)
        {
            return obj is DocumentKey key &&
                   EqualityComparer<DocumentTopicName>.Default.Equals(Topic, key.Topic) &&
                   Value == key.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = 2057312302;
            hashCode = hashCode * -1521134295 + Topic.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        #endregion

        public static implicit operator DocumentKey(string key) => new DocumentKey(key);

        public static implicit operator string(DocumentKey key) => key.Value;
    }

}