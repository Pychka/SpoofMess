using System.ComponentModel;

namespace CommonObjects.Results;

public class Result<T> : IDisposable, IAsyncDisposable
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }


    public int StatusCode = 200;

    public T? Body { get; set; }

    private static Result<T> GetResult(bool success = true, string? message = null, string? error = null, int statusCode = 200, T? body = default) =>
     new()
     {
         Success = success,
         Error = error,
         StatusCode = statusCode,
         Message = message,
         Body = body,
     };

    [Obsolete("Используйте специализированные методы.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Result<T> SuccessResult(T body, string? message = "OK", int statusCode = 200) =>
        body is null 
        ? throw new NullReferenceException("Body can't be empty if statusCode success. Use Result if you don't need send body with response") 
        : GetResult(success: true, message: message, statusCode: statusCode, body: body);

    #pragma warning disable CS0618
    public static Result<T> OkResult(T body) =>
        SuccessResult(body);
    #pragma warning restore CS0618
    public static Result<T> NotFoundResult(string error) =>
        ErrorResult(error, 404);

    public static Result<T> Forbidden(string error = "Forbidden") =>
        ErrorResult(error, 403);
    public static Result<T> BadRequest(string error) =>
        ErrorResult(error, 400);

    public static Result<T> ErrorResult(string error, int statusCode = 500) =>
        GetResult(success: false, error: error, statusCode: statusCode);

    public static Result<T> UnAuthorized(string error) =>
        ErrorResult(error, 401);

    public static Result<T> InternalServerError(string error = "Internal server error") =>
        ErrorResult(error, 500);

    public static Result<T> From(Result result) =>
        GetResult(success: result.Success, message: result.Message, error: result.Error, statusCode: result.StatusCode);
    public static Result<T> From<TAnother>(Result<TAnother> result) =>
        GetResult(success: result.Success, message: result.Message, error: result.Error, statusCode: result.StatusCode);


    public static Result<T> Parse(string? message, T? body, int statusCode)
    {
        return statusCode switch
        {
            200 => OkResult(body ?? throw new NullReferenceException("Body can't be empty if statusCode success. Use Result if you don't need send body with response")),
            400 => BadRequest(message ?? ""),
            401 => UnAuthorized(message ?? ""),
            403 => Forbidden(message ?? ""),
            404 => NotFoundResult(message ?? ""),
            500 => ErrorResult(message ?? ""),
            _ => throw new InvalidOperationException(message),
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (Body is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (Body is IDisposable disposable)
            disposable.Dispose();
        GC.SuppressFinalize(this);
    }
}
