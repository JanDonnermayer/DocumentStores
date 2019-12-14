#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DocumentStores.Primitives;

namespace DocumentStores.Internal
{
    internal static class DocumentPathBuilder
    {
  
        public static string GetPath(this DocumentTopicName topic) =>
         string.Join(
             separator.ToString(),
             topic.Select(Encode));

        public static string GetPath(this DocumentKey key) =>
            string.Join(
                separator.ToString(),
                key.Topic.GetPath(),
                Encode(key.Value));

        public static DocumentKey GetKey(string path)
        {
            var segments = path.Split(separator);
            return DocumentTopicName
                .Create(segments
                    .Take(segments.Count() - 1)
                    .Select(Decode))
                .CreateKey(Decode(segments.Last()));
        }


        private static readonly char separator = Path.DirectorySeparatorChar;

        // Map invalid filename chars to some weird unicode
        private static readonly ImmutableDictionary<char, char> encodingMap =
            Path
                .GetInvalidFileNameChars()
                .Select((_, i) => new KeyValuePair<char, char>(_, (char)(i + 2000)))
                .ToImmutableDictionary();

        private static readonly IImmutableDictionary<char, char> decodingMap =
            encodingMap
                .Select(kvp => new KeyValuePair<char, char>(kvp.Value, kvp.Key))
                .ToImmutableDictionary();

        private static string Encode(string key) =>
            new string(key.Select(_ => encodingMap.TryGetValue(_, out var v) ? v : _).ToArray());

        private static string Decode(string encodedKey) =>
            new string(encodedKey.Select(_ => decodingMap.TryGetValue(_, out var v) ? v : _).ToArray());

        // Check whether key is null,
        // or contains anything from decoding map which would lead to collisions
        private static string CheckKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (key.Any(decodingMap.Keys.Contains))
                throw new ArgumentException("Key contains invalid chars!", nameof(key));
            return key;
        }


    }




}