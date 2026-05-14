namespace AdditionalHelpers.Services;

public interface ISerializer
{
    public string Serialize<T>(T obj);

    public Task Serialize<T>(T obj, Stream stream);

    public T Deserialize<T>(string text);

    public T Deserialize<T>(byte[] body);
    public Task<T> Deserialize<T>(Stream body);
}
