using System.Text.Encodings.Web;
using System.Text.Json;

namespace AdditionalHelpers;

public class JsonService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    public static string Serialize<T>(T obj) =>
        JsonSerializer.Serialize(obj, Options);

    public static T? Deserialize<T>(string json)
    {
        try
        {
            Console.WriteLine($"json: {json}");
            return JsonSerializer.Deserialize<T>(json, Options);
        }
        catch
        {
            throw new InvalidDataException($"Invalid json for type: {typeof(T).Name} json: {json}");
        }
    }
}
