using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class StreamUtils
{
    public static async Task<Stream> CreateJsonStream<TInput>(TInput input, JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions();
        var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, input, options);
        stream.Position = 0;
        return stream;
    }
}
