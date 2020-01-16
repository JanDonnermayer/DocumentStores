using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace DocumentStores.Internal
{
    internal static class CharShiftEncoder
    {

        private static readonly IImmutableDictionary<char, char> _encodingMap =
            Path
                .GetInvalidFileNameChars()
                .Select(c => new KeyValuePair<char, char>(c, (char)(c + 1000)))
                .ToImmutableDictionary();

        private static readonly IImmutableDictionary<char, char> _decodingMap =
            _encodingMap
                .Select(kvp => new KeyValuePair<char, char>(kvp.Value, kvp.Key))
                .ToImmutableDictionary();

        private static readonly Lazy<IImmutableDictionary<char, char>> encodingMap =
            new Lazy<IImmutableDictionary<char, char>>(() => _encodingMap);

        private static readonly Lazy<IImmutableDictionary<char, char>> decodingMap =
            new Lazy<IImmutableDictionary<char, char>>(() => _decodingMap);

        // Check whether key is null,
        // or contains anything from decoding map which would lead to collisions
        private static string CheckChars(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Value is null or empty!", nameof(value));
            if (value.Any(_decodingMap.Keys.Contains))
                throw new ArgumentException("Key contains invalid chars!", nameof(value));
            return value;
        }

        private static string Encode(string value) =>
            new string(value.Select(c => encodingMap.Value.TryGetValue(c, out var v) ? v : c).ToArray());

        private static string Decode(string encodedValue) =>
            new string(encodedValue.Select(c => decodingMap.Value.TryGetValue(c, out var v) ? v : c).ToArray());

        public static DocumentKey Encode(this DocumentKey source) =>
            source.MapValue(CheckChars).MapValue(Encode);

        public static DocumentRoute Encode(this DocumentRoute source) =>
            source.MapSegments(CheckChars).MapSegments(Encode);

        public static DocumentRoute Decode(this DocumentRoute source) =>
            source.MapSegments(Decode);

        public static DocumentKey Decode(this DocumentKey source) =>
            source.MapValue(Decode);
    }
}
