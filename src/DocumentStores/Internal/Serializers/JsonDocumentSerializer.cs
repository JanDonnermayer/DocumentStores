﻿using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DocumentStores.Internal
{
    internal class JsonDocumentSerializer : IDocumentSerializer
    {
        async Task IDocumentSerializer.SerializeAsync<T>(Stream stream, T data)
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            if (!stream.CanSeek)
                throw new System.ArgumentException("Stream does not support seeking!");

            if (data is null)
                throw new System.ArgumentNullException(nameof(data));

            var text = JsonConvert.SerializeObject(data, Formatting.Indented);
            var buffer = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            stream.SetLength(stream.Position); // Truncate surplus content.
        }

        async Task<T> IDocumentSerializer.DeserializeAsync<T>(Stream stream)
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            var text = Encoding.UTF8.GetString(buffer);
            return JsonConvert.DeserializeObject<T>(text); 
        }
    }
}
