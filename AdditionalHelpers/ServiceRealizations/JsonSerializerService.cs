using AdditionalHelpers.Services;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdditionalHelpers.ServiceRealizations;

public class JsonSerializerService : ISerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        AllowOutOfOrderMetadataProperties = true
    };
    public string Serialize<T>(T obj) =>
        JsonSerializer.Serialize(obj, Options);

    public async Task Serialize<T>(T obj, Stream stream) =>
        await JsonSerializer.SerializeAsync(stream, obj, Options);

    public T Deserialize<T>(string text)
    {
        T? obj;
        try
        {
            obj = JsonSerializer.Deserialize<T>(text, Options);
        }
        catch
        {
            throw new InvalidDataException($"Invalid json for type: {typeof(T).Name} json: {text}");
        }
        return obj ?? throw new ArgumentNullException($"Data has been null: {text}");
    }

    public T Deserialize<T>(byte[] body)
    {
        T? obj;
        try
        {
            obj = JsonSerializer.Deserialize<T>(body, Options);
        }
        catch
        {
            throw new InvalidDataException($"Invalid json for type: {typeof(T).Name} body: {body}");
        }
        return obj ?? throw new ArgumentNullException($"Data has been null: {body}");
    }

    public async Task<T> Deserialize<T>(Stream body)
    {
        T? obj;
        try
        {
            obj = JsonSerializer.Deserialize<T>(body, Options);
        }
        catch
        {
            throw new InvalidDataException($"Invalid json for type: {typeof(T).Name} body: {body}");
        }
        return obj ?? throw new ArgumentNullException($"Data has been null: {body}");
    }
}
