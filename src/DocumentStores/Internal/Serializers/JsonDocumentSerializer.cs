using System.ComponentModel;
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

            if (data is null)
                throw new System.ArgumentNullException(nameof(data));

            var text = JsonConvert.SerializeObject(data, Formatting.Indented);
            var buffer = Encoding.UTF8.GetBytes(text);
            await stream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        async Task<T> IDocumentSerializer.DeserializeAsync<T>(Stream stream)
        {
            if (stream is null)
                throw new System.ArgumentNullException(nameof(stream));

            using var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream).ConfigureAwait(false);
            var buffer = memStream.ToArray();
            var text = Encoding.UTF8.GetString(buffer);

            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}
